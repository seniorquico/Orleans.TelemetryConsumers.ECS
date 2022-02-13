using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Statistics;

namespace Orleans.TelemetryConsumers.ECS;

/// <summary>
///     An Orleans host environment statistics provider that obtains values from the ECS Task Metadata Endpoint.
/// </summary>
internal sealed class EcsTaskHostEnvironmentStatistics : IDisposable, IHostEnvironmentStatistics, ILifecycleObserver, ILifecycleParticipant<IClusterClientLifecycle>, ILifecycleParticipant<ISiloLifecycle>
{
    /// <summary>A useful constant.</summary>
    private const float KB = 1024f;

    /// <summary>The delay between ECS Task Metadata Endpoint polling attempts (in milliseconds).</summary>
    private const int POLLING_DELAY = 5000;

    /// <summary>
    ///     The pre-compiled error message logged when the service fails to fetch host environment statistics.
    /// </summary>
    private static readonly Action<ILogger, Exception?> LogFetchFailure = LoggerMessage.Define(LogLevel.Error, 1, "Failed to fetch host environment statistics.");

    /// <summary>The pre-compiled info message logged when the service is starting.</summary>
    private static readonly Action<ILogger, Exception?> LogStart = LoggerMessage.Define(LogLevel.Information, 1, "Starting ECS Task host environment statistics service.");

    /// <summary>The pre-compiled info message logged when the service is stopping.</summary>
    private static readonly Action<ILogger, Exception?> LogStop = LoggerMessage.Define(LogLevel.Information, 1, "Stopping ECS Task host environment statistics service.");

    /// <summary>The pre-compiled trace message logged when updating the host environment statistics.</summary>
    private static readonly Action<ILogger, float?, long?, long?, Exception?> LogTrace = LoggerMessage.Define<float?, long?, long?>(LogLevel.Trace, 1, "CpuUsage = {CpuUsage}, AvailableMemory = {AvailableMemory}, TotalPhysicalMemory = {TotalPhysicalMemory}");

    /// <summary>
    ///     The pre-compiled error message logged when the service fails to update host environment statistics.
    /// </summary>
    private static readonly Action<ILogger, Exception?> LogUpdateFailure = LoggerMessage.Define(LogLevel.Error, 1, "Failed to update host environment statistics.");

    /// <summary>The cancellation token source used to stop the background process.</summary>
    private readonly CancellationTokenSource cancellationTokenSource;

    /// <summary>The ECS Task Metadata Endpoint client factory.</summary>
    private readonly IEcsTaskMetadataClientFactory clientFactory;

    /// <summary>The logger.</summary>
    private readonly ILogger<EcsTaskHostEnvironmentStatistics>? logger;

    /// <summary>The synchronization context used to start or stop the background process.</summary>
    private readonly object taskLock;

    /// <summary>A value indicating whether the object has been disposed.</summary>
    private bool disposed;

    /// <summary>The task that represents the background process.</summary>
    private Task? task;

    /// <summary>Initializes a new instance of the <see cref="EcsTaskHostEnvironmentStatistics"/> class.</summary>
    /// <param name="clientFactory">The Task Metadata Endpoint client factory.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="clientFactory"/> is <c>null</c>.</exception>
    public EcsTaskHostEnvironmentStatistics(IEcsTaskMetadataClientFactory? clientFactory, ILogger<EcsTaskHostEnvironmentStatistics>? logger)
    {
        this.cancellationTokenSource = new CancellationTokenSource();
        this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        this.logger = logger;
        this.taskLock = new object();
    }

    /// <summary>
    ///     Gets the total available memory (free for allocation) on the host (in bytes). For example, 14426476000L is
    ///     equal to 14 GB.
    /// </summary>
    public long? AvailableMemory { get; private set; }

    /// <summary>
    ///     Gets the percentage of CPU usage on the host as a floating point value from 0.0 to 100.0 (inclusive). For
    ///     example, 70.5f is equal to 70.5%.
    /// </summary>
    public float? CpuUsage { get; private set; }

    /// <summary>
    ///     Gets the total physical memory on the host (in bytes). For example, 16426476000L is equal to 16 GB.
    /// </summary>
    public long? TotalPhysicalMemory { get; private set; }

    /// <summary>Disposes the managed resources.</summary>
    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        try
        {
            if (this.task != null)
            {
                if (this.logger != null)
                {
                    LogStop(this.logger, null);
                }

                this.cancellationTokenSource.Cancel();
                this.task.ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation errors.
        }
        finally
        {
            this.cancellationTokenSource.Dispose();
            this.task?.Dispose();
        }
    }

    /// <summary>
    ///     Asynchronously starts monitoring the host environment statistics. This starts a background process that
    ///     periodically polls an external API to get the host environment statistics.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">If this service has already been started.</exception>
    public Task OnStart(CancellationToken ct)
    {
        lock (this.taskLock)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (this.task != null)
            {
                throw new InvalidOperationException("The statistics service has already been started.");
            }

            ct.Register(this.cancellationTokenSource.Cancel);

            if (this.logger != null)
            {
                LogStart(this.logger, null);
            }

            this.task = this.cancellationTokenSource.IsCancellationRequested ? Task.FromCanceled(this.cancellationTokenSource.Token) : this.ExecuteAsync(this.cancellationTokenSource.Token);
            if (this.task.IsCompleted)
            {
                return this.task;
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    ///     Asynchronously stops monitoring the host environment statistics. This waits for the background process to
    ///     stop until canceled by the <paramref name="ct"/>.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task OnStop(CancellationToken ct)
    {
        Task? currentTask;
        lock (this.taskLock)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            currentTask = this.task;
            if (currentTask == null)
            {
                throw new InvalidOperationException("The statistics service has not been started.");
            }

            if (this.logger != null)
            {
                LogStop(this.logger, null);
            }

            this.cancellationTokenSource.Cancel();
        }

        return Task.WhenAny(currentTask, Task.Delay(Timeout.Infinite, ct));
    }

    /// <summary>
    ///     Registers with the cluster client lifecycle to run <see cref="OnStart(CancellationToken)"/> on startup and
    ///     <see cref="OnStop(CancellationToken)"/> on shutdown.
    /// </summary>
    /// <param name="lifecycle">The cluster client lifecycle.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="lifecycle"/> is <c>null</c>.</exception>
    public void Participate(IClusterClientLifecycle? lifecycle)
    {
        if (lifecycle == null)
        {
            throw new ArgumentNullException(nameof(lifecycle));
        }

        lifecycle.Subscribe(ServiceLifecycleStage.RuntimeInitialize, this);
    }

    /// <summary>
    ///     Registers with the silo lifecycle to run <see cref="OnStart(CancellationToken)"/> on startup and
    ///     <see cref="OnStop(CancellationToken)"/> shutdown.
    /// </summary>
    /// <param name="lifecycle">The silo lifecycle.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="lifecycle"/> is <c>null</c>.</exception>
    public void Participate(ISiloLifecycle? lifecycle)
    {
        if (lifecycle == null)
        {
            throw new ArgumentNullException(nameof(lifecycle));
        }

        lifecycle.Subscribe(ServiceLifecycleStage.RuntimeInitialize, this);
    }

    /// <summary>Uses reflection to create an Orleans statistic with a float value.</summary>
    /// <param name="statisticName">The Orleans statistic name.</param>
    /// <param name="fetcher">The function used to get the next float value.</param>
    private static void FindOrCreateFloatValueStatistic(StatisticName statisticName, Func<float> fetcher)
    {
        var coreAssembly = typeof(ICounter).Assembly;

        var floatValueStatisticType = coreAssembly.GetType("Orleans.Runtime.FloatValueStatistic");
        if (floatValueStatisticType == null)
        {
            throw new PlatformNotSupportedException("The Orleans.Runtime.FloatValueStatistic type does not exist");
        }

        var findOrCreateMethod = floatValueStatisticType.GetMethods().Single(method => method.IsPublic && method.IsStatic && "FindOrCreate".Equals(method.Name, StringComparison.Ordinal) && method.GetParameters().Length == 2);
        if (findOrCreateMethod == null)
        {
            throw new PlatformNotSupportedException("The FindOrCreate method of the Orleans.Runtime.FloatValueStatistic type does not exist");
        }

        findOrCreateMethod.Invoke(null, new object[] { statisticName, fetcher });
    }

    /// <summary>Uses reflection to create an Orleans statistic with an integer value.</summary>
    /// <param name="statisticName">The Orleans statistic name.</param>
    /// <param name="fetcher">The function used to get the next integer value.</param>
    private static void FindOrCreateIntValueStatistic(StatisticName statisticName, Func<long> fetcher)
    {
        var coreAssembly = typeof(ICounter).Assembly;

        var intValueStatisticType = coreAssembly.GetType("Orleans.Runtime.IntValueStatistic");
        if (intValueStatisticType == null)
        {
            throw new PlatformNotSupportedException("The Orleans.Runtime.IntValueStatistic type does not exist");
        }

        var findOrCreateMethod = intValueStatisticType.GetMethod("FindOrCreate", BindingFlags.Public | BindingFlags.Static);
        if (findOrCreateMethod == null)
        {
            throw new PlatformNotSupportedException("The FindOrCreate method of the Orleans.Runtime.IntValueStatistic type does not exist");
        }

        findOrCreateMethod.Invoke(null, new object[] { statisticName, fetcher, Type.Missing });
    }

    /// <summary>
    ///     Asynchronously polls the external API on a continuous schedule to get the host environment statistics until
    ///     canceled by the <paramref name="cancellationToken"/>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Ensures the long-running task continues after transient errors.")]
    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this.SetupStats();
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = this.clientFactory.Create();
                var stats = await client.GetContainerStatsAsync(cancellationToken).ConfigureAwait(false);
                if (stats == null && this.logger != null)
                {
                    LogFetchFailure(this.logger, null);
                }

                this.UpdateStats(stats);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (this.logger != null)
                {
                    LogUpdateFailure(this.logger, e);
                }
            }

            await Task.Delay(POLLING_DELAY, cancellationToken).ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <summary>Uses reflection to create the Orleans statistics.</summary>
    private void SetupStats()
    {
        FindOrCreateFloatValueStatistic(StatisticNames.RUNTIME_CPUUSAGE, () => this.CpuUsage ?? 0);

        FindOrCreateIntValueStatistic(StatisticNames.RUNTIME_GC_TOTALMEMORYKB, () => (long)((GC.GetTotalMemory(false) + KB - 1.0) / KB));
        FindOrCreateIntValueStatistic(StatisticNames.RUNTIME_MEMORY_TOTALPHYSICALMEMORYMB, () => (long)(((this.TotalPhysicalMemory ?? 0) / KB) / KB));
        FindOrCreateIntValueStatistic(StatisticNames.RUNTIME_MEMORY_AVAILABLEMEMORYMB, () => (long)(((this.AvailableMemory ?? 0) / KB) / KB));

        FindOrCreateIntValueStatistic(StatisticNames.RUNTIME_DOT_NET_THREADPOOL_INUSE_WORKERTHREADS, () =>
        {
            ThreadPool.GetMaxThreads(out var maXworkerThreads, out var maXcompletionPortThreads);

            // GetAvailableThreads Retrieves the difference between the maximum number of thread pool threads and the
            // number currently active. So max-Available is the actual number in use. If it goes beyond min, it means we
            // are stressing the thread pool.
            ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
            return maXworkerThreads - workerThreads;
        });
        FindOrCreateIntValueStatistic(StatisticNames.RUNTIME_DOT_NET_THREADPOOL_INUSE_COMPLETIONPORTTHREADS, () =>
        {
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);

            ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
            return maxCompletionPortThreads - completionPortThreads;
        });
    }

    /// <summary>Updates the CPU usage, available memory, and total physical memory values.</summary>
    /// <param name="stats">The values obtained from the ECS Task Metadata Endpoint.</param>
    private void UpdateStats(EcsContainerStats? stats)
    {
        if (stats != null)
        {
            if (stats.MemoryStats != null && stats.MemoryStats.Limit.HasValue)
            {
                if (stats.MemoryStats.Usage.HasValue)
                {
                    var available = stats.MemoryStats.Limit.Value - stats.MemoryStats.Usage.Value;
                    if (available <= long.MaxValue)
                    {
                        this.AvailableMemory = (long)available;
                    }
                    else
                    {
                        this.AvailableMemory = long.MaxValue;
                    }
                }
                else
                {
                    this.AvailableMemory = null;
                }

                if (stats.MemoryStats.Limit.Value <= long.MaxValue)
                {
                    this.TotalPhysicalMemory = (long)stats.MemoryStats.Limit.Value;
                }
                else
                {
                    this.TotalPhysicalMemory = long.MaxValue;
                }
            }
            else
            {
                this.AvailableMemory = null;
                this.TotalPhysicalMemory = null;
            }

            if (stats.CpuStats != null && stats.CpuStats.CpuUsage != null &&
                stats.CpuStats.CpuUsage.TotalUsage.HasValue && stats.CpuStats.SystemCpuUsage.HasValue &&
                stats.PreviousCpuStats != null && stats.PreviousCpuStats.CpuUsage != null &&
                stats.PreviousCpuStats.CpuUsage.TotalUsage.HasValue &&
                stats.PreviousCpuStats.SystemCpuUsage.HasValue)
            {
                var containerUsage =
                    stats.CpuStats.CpuUsage.TotalUsage.Value - stats.PreviousCpuStats.CpuUsage.TotalUsage.Value;
                var systemUsage =
                    stats.CpuStats.SystemCpuUsage.Value - stats.PreviousCpuStats.SystemCpuUsage.Value;
                this.CpuUsage = (float)((double)containerUsage / systemUsage) * 100f;
            }
            else
            {
                this.CpuUsage = null;
            }
        }
        else
        {
            this.AvailableMemory = null;
            this.CpuUsage = null;
            this.TotalPhysicalMemory = null;
        }

        if (this.logger != null)
        {
            LogTrace(this.logger, this.CpuUsage, this.AvailableMemory, this.TotalPhysicalMemory, null);
        }
    }
}
