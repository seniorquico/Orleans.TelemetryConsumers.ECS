using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Runtime;
using Xunit;
using Xunit.Abstractions;

namespace Orleans.TelemetryConsumers.ECS;

public sealed class EcsTaskHostEnvironmentStatisticsTest : IDisposable
{
    private const int PRECISION = 4;

    private readonly ILoggerFactory loggerFactory;

    private EcsTaskHostEnvironmentStatistics? statistics;

    public EcsTaskHostEnvironmentStatisticsTest(ITestOutputHelper? outputHelper)
    {
        this.loggerFactory = new LoggerFactory();
        this.loggerFactory.AddXUnit(outputHelper ?? throw new ArgumentNullException(nameof(outputHelper)));
        this.statistics = null;
    }

    public static IEnumerable<object?[]> UpdatesStatsData =>
        new List<object?[]>
        {
                new object?[] { null, null, null, null },
                new object?[] { new EcsContainerStats(), null, null, null },
                new object?[]
                {
                    new EcsContainerStats
                    {
                        CpuStats = new EcsContainerCpuStats(),
                        MemoryStats = new EcsContainerMemoryStats(),
                        PreviousCpuStats = new EcsContainerCpuStats(),
                    },
                    null,
                    null,
                    null,
                },
                new object?[]
                {
                    new EcsContainerStats
                    {
                        CpuStats = new EcsContainerCpuStats
                        {
                            CpuUsage = new EcsContainerCpuUsageStats(),
                        },
                        MemoryStats = new EcsContainerMemoryStats
                        {
                            Limit = 8388608L,
                        },
                        PreviousCpuStats = new EcsContainerCpuStats
                        {
                            CpuUsage = new EcsContainerCpuUsageStats(),
                        },
                    },
                    null,
                    null,
                    8388608L,
                },
                new object?[]
                {
                    new EcsContainerStats
                    {
                        CpuStats = new EcsContainerCpuStats
                        {
                            CpuUsage = new EcsContainerCpuUsageStats
                            {
                                TotalUsage = 7000000L,
                            },
                            SystemCpuUsage = 18000000L,
                        },
                        MemoryStats = new EcsContainerMemoryStats
                        {
                            Limit = 8388608L,
                            Usage = 524288L,
                        },
                        PreviousCpuStats = new EcsContainerCpuStats
                        {
                            CpuUsage = new EcsContainerCpuUsageStats
                            {
                                TotalUsage = 6000000L,
                            },
                            SystemCpuUsage = 15000000L,
                        },
                    },
                    7864320L,
                    33.3333f,
                    8388608L,
                },
                new object?[]
                {
                    new EcsContainerStats
                    {
                        CpuStats = new EcsContainerCpuStats
                        {
                            CpuUsage = new EcsContainerCpuUsageStats
                            {
                                TotalUsage = 10000000L,
                            },
                            SystemCpuUsage = 18000000L,
                        },
                        MemoryStats = new EcsContainerMemoryStats
                        {
                            Limit = ulong.MaxValue,
                            Usage = 524288L,
                        },
                        PreviousCpuStats = new EcsContainerCpuStats
                        {
                            CpuUsage = new EcsContainerCpuUsageStats
                            {
                                TotalUsage = 6000000L,
                            },
                            SystemCpuUsage = 15000000L,
                        },
                    },
                    long.MaxValue,
                    133.3333f,
                    long.MaxValue,
                },
        };

    public void Dispose()
    {
        if (this.statistics != null)
        {
            this.statistics.Dispose();
            this.statistics = null;
        }

        this.loggerFactory.Dispose();
    }

    [Fact]
    public void GetInitialProperties()
    {
        // Arrange
        var clientFactoryMock = new Mock<IEcsTaskMetadataClientFactory>(MockBehavior.Strict);

        // Act
        this.statistics = new EcsTaskHostEnvironmentStatistics(clientFactoryMock.Object, new Logger<EcsTaskHostEnvironmentStatistics>(this.loggerFactory));

        // Assert
        Assert.Null(this.statistics.AvailableMemory);
        Assert.Null(this.statistics.CpuUsage);
        Assert.Null(this.statistics.TotalPhysicalMemory);
    }

    [Fact]
    public void InstantiateWithoutClientFactoryThrowsError()
    {
        // Act
        var exception = Record.Exception(() =>
        {
            this.statistics = new EcsTaskHostEnvironmentStatistics(null, new Logger<EcsTaskHostEnvironmentStatistics>(this.loggerFactory));
        });

        // Assert
        Assert.NotNull(exception);
        var argumentNullException = Assert.IsType<ArgumentNullException>(exception);
        Assert.Equal("clientFactory", argumentNullException.ParamName);
    }

    [Fact]
    public void OnStartAfterDisposeThrowsError()
    {
        // Arrange
        var clientFactoryMock = new Mock<IEcsTaskMetadataClientFactory>(MockBehavior.Strict);

        this.statistics = new EcsTaskHostEnvironmentStatistics(clientFactoryMock.Object, new Logger<EcsTaskHostEnvironmentStatistics>(this.loggerFactory));
        this.statistics.Dispose();

        // Act
        var exception = Record.Exception(() =>
        {
            this.statistics.OnStart(CancellationToken.None);
        });

        // Assert
        Assert.NotNull(exception);
        var objectDisposedException = Assert.IsType<ObjectDisposedException>(exception);
        Assert.Equal("Orleans.TelemetryConsumers.ECS.EcsTaskHostEnvironmentStatistics", objectDisposedException.ObjectName);
    }

    [Fact]
    public void OnStartWithExistingTaskThrowsError()
    {
        // Arrange
        var clientFactoryMock = new Mock<IEcsTaskMetadataClientFactory>(MockBehavior.Strict);

        this.statistics = new EcsTaskHostEnvironmentStatistics(clientFactoryMock.Object, new Logger<EcsTaskHostEnvironmentStatistics>(this.loggerFactory));

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var task = this.statistics.OnStart(cancellationTokenSource.Token);

        // Act
        var exception = Record.Exception(() =>
        {
            this.statistics.OnStart(CancellationToken.None);
        });

        // Assert
        Assert.NotNull(task);
        Assert.NotNull(exception);
        var invalidOperationException = Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains("has already been started", invalidOperationException.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void OnStartWithIsCancellationRequestedCancels()
    {
        // Arrange
        var clientFactoryMock = new Mock<IEcsTaskMetadataClientFactory>(MockBehavior.Strict);

        this.statistics = new EcsTaskHostEnvironmentStatistics(clientFactoryMock.Object, new Logger<EcsTaskHostEnvironmentStatistics>(this.loggerFactory));

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var task = this.statistics.OnStart(cancellationTokenSource.Token);

        // Assert
        Assert.NotNull(task);
        Assert.True(task.IsCompleted);
        Assert.True(task.IsCanceled);
    }

    [Fact]
    public void OnStartWithOperationCanceledExceptionCancels()
    {
        // Arrange
        var exception = new OperationCanceledException();

        var clientMock = new Mock<IEcsTaskMetadataClient>(MockBehavior.Strict);
        clientMock
            .Setup(o => o.GetContainerStatsAsync(It.IsAny<CancellationToken>()))
            .Throws(exception);

        var clientFactoryMock = new Mock<IEcsTaskMetadataClientFactory>(MockBehavior.Strict);
        clientFactoryMock
            .Setup(o => o.Create())
            .Returns(clientMock.Object);

        this.statistics = new EcsTaskHostEnvironmentStatistics(clientFactoryMock.Object, new Logger<EcsTaskHostEnvironmentStatistics>(this.loggerFactory));

        // Act
        var task = this.statistics.OnStart(CancellationToken.None);

        // Assert
        Assert.NotNull(task);
        Assert.True(task.IsCompleted);
        Assert.True(task.IsCanceled);
    }

    [Fact]
    public void OnStopAfterDisposeThrowsError()
    {
        // Arrange
        var clientFactoryMock = new Mock<IEcsTaskMetadataClientFactory>(MockBehavior.Strict);

        this.statistics = new EcsTaskHostEnvironmentStatistics(clientFactoryMock.Object, new Logger<EcsTaskHostEnvironmentStatistics>(this.loggerFactory));
        this.statistics.Dispose();

        // Act
        var exception = Record.Exception(() =>
        {
            this.statistics.OnStop(CancellationToken.None);
        });

        // Assert
        Assert.NotNull(exception);
        var objectDisposedException = Assert.IsType<ObjectDisposedException>(exception);
        Assert.Equal("Orleans.TelemetryConsumers.ECS.EcsTaskHostEnvironmentStatistics", objectDisposedException.ObjectName);
    }

    [Fact]
    public void OnStopWithoutExistingTaskThrowsError()
    {
        // Arrange
        var clientFactoryMock = new Mock<IEcsTaskMetadataClientFactory>(MockBehavior.Strict);

        this.statistics = new EcsTaskHostEnvironmentStatistics(clientFactoryMock.Object, new Logger<EcsTaskHostEnvironmentStatistics>(this.loggerFactory));

        // Act
        var exception = Record.Exception(() =>
        {
            this.statistics.OnStop(CancellationToken.None);
        });

        // Assert
        Assert.NotNull(exception);
        var invalidOperationException = Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains("has not been started", invalidOperationException.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParticipateInClusterClientLifecycle()
    {
        // Arrange
        var clientFactoryMock = new Mock<IEcsTaskMetadataClientFactory>(MockBehavior.Strict);

        this.statistics = new EcsTaskHostEnvironmentStatistics(clientFactoryMock.Object, new Logger<EcsTaskHostEnvironmentStatistics>(this.loggerFactory));

        var lifecycleMock = new Mock<IClusterClientLifecycle>(MockBehavior.Strict);
        lifecycleMock
            .Setup(o => o.Subscribe(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ILifecycleObserver>()))
            .Returns<IDisposable>(null);

        // Act
        this.statistics.Participate(lifecycleMock.Object);

        // Assert
        lifecycleMock.Verify(o => o.Subscribe("Orleans.TelemetryConsumers.ECS.EcsTaskHostEnvironmentStatistics", ServiceLifecycleStage.RuntimeInitialize, this.statistics), Times.Once());
    }

    [Fact]
    public void ParticipateInNullClusterClientLifecycleThrowsError()
    {
        // Arrange
        var clientFactoryMock = new Mock<IEcsTaskMetadataClientFactory>(MockBehavior.Strict);

        this.statistics = new EcsTaskHostEnvironmentStatistics(clientFactoryMock.Object, new Logger<EcsTaskHostEnvironmentStatistics>(this.loggerFactory));

        // Act
        var exception = Record.Exception(() =>
        {
            this.statistics.Participate((IClusterClientLifecycle?)null);
        });

        // Assert
        Assert.NotNull(exception);
        var argumentNullException = Assert.IsType<ArgumentNullException>(exception);
        Assert.Equal("lifecycle", argumentNullException.ParamName);
    }

    [Fact]
    public void ParticipateInNullSiloLifecycleThrowsError()
    {
        // Arrange
        var clientFactoryMock = new Mock<IEcsTaskMetadataClientFactory>(MockBehavior.Strict);

        this.statistics = new EcsTaskHostEnvironmentStatistics(clientFactoryMock.Object, new Logger<EcsTaskHostEnvironmentStatistics>(this.loggerFactory));

        // Act
        var exception = Record.Exception(() =>
        {
            this.statistics.Participate((ISiloLifecycle?)null);
        });

        // Assert
        Assert.NotNull(exception);
        var argumentNullException = Assert.IsType<ArgumentNullException>(exception);
        Assert.Equal("lifecycle", argumentNullException.ParamName);
    }

    [Fact]
    public void ParticipateInSiloLifecycle()
    {
        // Arrange
        var clientFactoryMock = new Mock<IEcsTaskMetadataClientFactory>(MockBehavior.Strict);

        this.statistics = new EcsTaskHostEnvironmentStatistics(clientFactoryMock.Object, new Logger<EcsTaskHostEnvironmentStatistics>(this.loggerFactory));

        var lifecycleMock = new Mock<ISiloLifecycle>(MockBehavior.Strict);
        lifecycleMock
            .Setup(o => o.Subscribe(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ILifecycleObserver>()))
            .Returns<IDisposable>(null);

        // Act
        this.statistics.Participate(lifecycleMock.Object);

        // Assert
        lifecycleMock.Verify(o => o.Subscribe("Orleans.TelemetryConsumers.ECS.EcsTaskHostEnvironmentStatistics", ServiceLifecycleStage.RuntimeInitialize, this.statistics), Times.Once());
    }

    [MemberData(nameof(UpdatesStatsData))]
    [Theory]
    public async Task UpdatesStats(EcsContainerStats? stats, long? expectedAvailableMemory, float? expectedCpuUsage, long? expectedTotalPhysicalMemory)
    {
        // Arrange
        var taskCompletionSource = new TaskCompletionSource<object?>();

        var clientMock = new Mock<IEcsTaskMetadataClient>(MockBehavior.Strict);
        clientMock
            .Setup(o => o.GetContainerStatsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(stats))
            .Callback(() =>
            {
                taskCompletionSource.TrySetResult(null);
            });

        var clientFactoryMock = new Mock<IEcsTaskMetadataClientFactory>(MockBehavior.Strict);
        clientFactoryMock
            .Setup(o => o.Create())
            .Returns(clientMock.Object);

        this.statistics = new EcsTaskHostEnvironmentStatistics(clientFactoryMock.Object, new Logger<EcsTaskHostEnvironmentStatistics>(this.loggerFactory));

        // Act
        await this.statistics.OnStart(CancellationToken.None).ConfigureAwait(true);
        await taskCompletionSource.Task.ConfigureAwait(true);
        await this.statistics.OnStop(CancellationToken.None).ConfigureAwait(true);

        // Assert
        if (expectedAvailableMemory.HasValue)
        {
            Assert.True(this.statistics.AvailableMemory.HasValue);
            Assert.Equal(expectedAvailableMemory.Value, this.statistics.AvailableMemory!.Value);
        }
        else
        {
            Assert.False(this.statistics.AvailableMemory.HasValue);
        }

        if (expectedCpuUsage.HasValue)
        {
            Assert.True(this.statistics.CpuUsage.HasValue);
            Assert.Equal(expectedCpuUsage.Value, this.statistics.CpuUsage!.Value, PRECISION);
        }
        else
        {
            Assert.False(this.statistics.CpuUsage.HasValue);
        }

        if (expectedTotalPhysicalMemory.HasValue)
        {
            Assert.True(this.statistics.TotalPhysicalMemory.HasValue);
            Assert.Equal(expectedTotalPhysicalMemory.Value, this.statistics.TotalPhysicalMemory!.Value);
        }
        else
        {
            Assert.False(this.statistics.TotalPhysicalMemory.HasValue);
        }
    }
}
