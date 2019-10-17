// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using Xunit;
//
// namespace Orleans.TelemetryConsumers.ECS
// {
//     internal sealed class DockerComposeFixture : IAsyncLifetime, IDisposable
//     {
//         private const int INITIALIZED = 0;
//         private const int STARTED = 1;
//         private const int STOPPED = 2;
//
//         private readonly TaskCompletionSource<object> setupTaskSource;
//         private readonly TaskCompletionSource<object> teardownTaskSource;
//         private int state;
//
//         public DockerComposeFixture()
//         {
//             this.setupTaskSource = new TaskCompletionSource<object>(TaskCreationOptions.DenyChildAttach | TaskCreationOptions.RunContinuationsAsynchronously);
//             this.state = INITIALIZED;
//             this.teardownTaskSource = new TaskCompletionSource<object>(TaskCreationOptions.DenyChildAttach | TaskCreationOptions.RunContinuationsAsynchronously);
//         }
//
//         public void Dispose()
//         {
//             var previousState = Interlocked.CompareExchange(ref this.state, STOPPED, INITIALIZED);
//             if (previousState == INITIALIZED)
//             {
//                 // This thread caused the state to transition from INITIALIZED to STOPPED without ever running Docker
//                 // Compose. There is no Docker Compose environment to teardown.
//                 this.teardownTaskSource.TrySetResult(null);
//                 return;
//             }
//
//             previousState = Interlocked.CompareExchange(ref this.state, STOPPED, STARTED);
//             Task teardownTask;
//             if (previousState == STOPPED)
//             {
//                 // Another thread already started to teardown the Docker Compose environment.
//                 teardownTask = this.teardownTaskSource.Task;
//             }
//             else
//             {
//                 // This thread caused the state to transition from STARTED to STOPPED. There is a Docker Compose
//                 // environment to teardown.
//                 teardownTask = this.TeardownAsync();
//             }
//
//             // Wait (synchronously) for the teardown task to complete.
//             teardownTask.ConfigureAwait(false).GetAwaiter().GetResult();
//         }
//
//         public Task DisposeAsync()
//         {
//             var previousState = Interlocked.CompareExchange(ref this.state, STOPPED, INITIALIZED);
//             if (previousState == INITIALIZED)
//             {
//                 // This thread caused the state to transition from INITIALIZED to STOPPED without ever running Docker
//                 // Compose. There is no Docker Compose environment to teardown.
//                 this.teardownTaskSource.TrySetResult(null);
//                 return Task.CompletedTask;
//             }
//
//             previousState = Interlocked.CompareExchange(ref this.state, STOPPED, STARTED);
//             Task teardownTask;
//             if (previousState == STOPPED)
//             {
//                 // Another thread already started to teardown the Docker Compose environment.
//                 teardownTask = this.teardownTaskSource.Task;
//             }
//             else
//             {
//                 // This thread caused the state to transition from STARTED to STOPPED. There is a Docker Compose
//                 // environment to teardown.
//                 teardownTask = this.TeardownAsync();
//             }
//
//             return teardownTask;
//         }
//
//         public Task InitializeAsync()
//         {
//             var previousState = Interlocked.CompareExchange(ref this.state, STARTED, INITIALIZED);
//             Task setupTask;
//             if (previousState != INITIALIZED)
//             {
//                 // Another thread already started to setup or teardown the Docker Compose environment.
//                 setupTask = this.setupTaskSource.Task;
//             }
//             else
//             {
//                 // This thread caused the state to transition from INITIALIZED to STARTED.
//                 setupTask = this.SetupAsync();
//             }
//
//             return setupTask;
//         }
//
//         private async Task SetupAsync()
//         {
//             try
//             {
//                 // TODO: Run `docker-compose up`.
//                 await Task.Delay(5000);
//             }
//             catch (Exception e)
//             {
//                 this.setupTaskSource.TrySetException(e);
//             }
//             finally
//             {
//                 this.setupTaskSource.TrySetResult(null);
//             }
//         }
//
//         private async Task TeardownAsync()
//         {
//             try
//             {
//                 await this.setupTaskSource.Task;
//
//                 // TODO: Run `docker-compose down`.
//                 await Task.Delay(2500);
//             }
//             catch (Exception e)
//             {
//                 this.teardownTaskSource.TrySetException(e);
//             }
//             finally
//             {
//                 this.teardownTaskSource.TrySetResult(null);
//             }
//         }
//     }
// }
