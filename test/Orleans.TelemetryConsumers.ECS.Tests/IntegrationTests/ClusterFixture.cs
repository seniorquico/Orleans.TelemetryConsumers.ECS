// using System;
// using System.Threading.Tasks;
// using Orleans.Hosting;
// using Orleans.TestingHost;
// using Xunit;
//
// namespace Orleans.TelemetryConsumers.ECS
// {
//     public sealed class ClusterFixture : IAsyncLifetime, IDisposable
//     {
//         public ClusterFixture()
//         {
//             var builder = new TestClusterBuilder();
//             builder.Options.ConfigureFileLogging = true;
//             builder.AddSiloBuilderConfigurator<SiloBuilder>();
//             this.Cluster = builder.Build();
//         }
//
//         public TestCluster Cluster { get; }
//
//         public void Dispose() =>
//             this.Cluster.StopAllSilos();
//
//         public Task DisposeAsync() =>
//             this.Cluster.StopAllSilosAsync();
//
//         public Task InitializeAsync() =>
//             this.Cluster.DeployAsync();
//
//         private sealed class SiloBuilder : ISiloBuilderConfigurator
//         {
//             public void Configure(ISiloHostBuilder hostBuilder)
//             {
//                 if (hostBuilder == null)
//                 {
//                     throw new ArgumentNullException(nameof(hostBuilder));
//                 }
//
//                 hostBuilder.UseEcsTaskHostEnvironmentStatistics();
//             }
//         }
//     }
// }
