using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Orleans.Runtime;
using Orleans.Statistics;
using Xunit;
using ISiloBuilder = Orleans.Hosting.ISiloBuilder;

namespace Orleans.TelemetryConsumers.ECS;

public sealed class SiloHostBuilderExtensionsTest : BaseAddressEnvironmentVariableTest
{
    public SiloHostBuilderExtensionsTest(BaseAddressEnvironmentVariableFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void UseEcsTaskHostEnvironmentStatisticsConfiguresServices()
    {
        // Arrange
        this.SetBaseAddress("http://localhost/v3/");

        var serviceCollection = new ServiceCollection();

        var siloBuilder = new Mock<ISiloBuilder>(MockBehavior.Strict);
        siloBuilder
            .Setup(o => o.ConfigureServices(It.IsAny<Action<HostBuilderContext?, IServiceCollection?>>()))
            .Returns(siloBuilder.Object)
            .Callback((Action<HostBuilderContext?, IServiceCollection?> action) => action(null, serviceCollection));

        // Act
        siloBuilder.Object.UseEcsTaskHostEnvironmentStatistics();
        using var serviceProvider = serviceCollection.BuildServiceProvider();
        var hostEnvironmentStatistics = serviceProvider.GetService<IHostEnvironmentStatistics?>();
        var siloLifecycle = serviceProvider.GetService<ILifecycleParticipant<ISiloLifecycle>?>();
        var clientLifecycle = serviceProvider.GetService<ILifecycleParticipant<IClusterClientLifecycle>?>();

        // Assert
        Assert.NotNull(hostEnvironmentStatistics);
        Assert.NotNull(siloLifecycle);
        Assert.Null(clientLifecycle);
    }

    [Fact]
    public void UseEcsTaskHostEnvironmentStatisticsConfiguresServices2()
    {
        // Arrange
        this.SetBaseAddress("http://localhost/v3/");

        var serviceCollection = new ServiceCollection();

        var siloBuilder = new Mock<Hosting.ISiloHostBuilder>(MockBehavior.Strict);
        siloBuilder
            .Setup(o => o.ConfigureServices(It.IsAny<Action<Hosting.HostBuilderContext?, IServiceCollection?>>()))
            .Returns(siloBuilder.Object)
            .Callback((Action<Hosting.HostBuilderContext?, IServiceCollection?> action) => action(null, serviceCollection));

        // Act
        siloBuilder.Object.UseEcsTaskHostEnvironmentStatistics();
        using var serviceProvider = serviceCollection.BuildServiceProvider();
        var hostEnvironmentStatistics = serviceProvider.GetService<IHostEnvironmentStatistics?>();
        var siloLifecycle = serviceProvider.GetService<ILifecycleParticipant<ISiloLifecycle>?>();
        var clientLifecycle = serviceProvider.GetService<ILifecycleParticipant<IClusterClientLifecycle>?>();

        // Assert
        Assert.NotNull(hostEnvironmentStatistics);
        Assert.NotNull(siloLifecycle);
        Assert.Null(clientLifecycle);
    }

    [Fact]
    public void UseEcsTaskHostEnvironmentStatisticsDoesNotConfigureServices()
    {
        // Arrange
        this.ResetBaseAddress();

        var serviceCollection = new ServiceCollection();

        var siloBuilder = new Mock<ISiloBuilder>(MockBehavior.Strict);
        siloBuilder
            .Setup(o => o.ConfigureServices(It.IsAny<Action<HostBuilderContext?, IServiceCollection?>>()))
            .Returns(siloBuilder.Object)
            .Callback((Action<HostBuilderContext?, IServiceCollection?> action) => action(null, serviceCollection));

        // Act
        siloBuilder.Object.UseEcsTaskHostEnvironmentStatistics();
        using var serviceProvider = serviceCollection.BuildServiceProvider();
        var hostEnvironmentStatistics = serviceProvider.GetService<IHostEnvironmentStatistics?>();
        var siloLifecycle = serviceProvider.GetService<ILifecycleParticipant<ISiloLifecycle>?>();
        var clientLifecycle = serviceProvider.GetService<ILifecycleParticipant<IClusterClientLifecycle>?>();

        // Assert
        Assert.Null(hostEnvironmentStatistics);
        Assert.Null(siloLifecycle);
        Assert.Null(clientLifecycle);
    }

    [Fact]
    public void UseEcsTaskHostEnvironmentStatisticsDoesNotConfigureServices2()
    {
        // Arrange
        this.ResetBaseAddress();

        var serviceCollection = new ServiceCollection();

        var siloBuilder = new Mock<Hosting.ISiloHostBuilder>(MockBehavior.Strict);
        siloBuilder
            .Setup(o => o.ConfigureServices(It.IsAny<Action<Hosting.HostBuilderContext?, IServiceCollection?>>()))
            .Returns(siloBuilder.Object)
            .Callback((Action<Hosting.HostBuilderContext?, IServiceCollection?> action) => action(null, serviceCollection));

        // Act
        siloBuilder.Object.UseEcsTaskHostEnvironmentStatistics();
        using var serviceProvider = serviceCollection.BuildServiceProvider();
        var hostEnvironmentStatistics = serviceProvider.GetService<IHostEnvironmentStatistics?>();
        var siloLifecycle = serviceProvider.GetService<ILifecycleParticipant<ISiloLifecycle>?>();
        var clientLifecycle = serviceProvider.GetService<ILifecycleParticipant<IClusterClientLifecycle>?>();

        // Assert
        Assert.Null(hostEnvironmentStatistics);
        Assert.Null(siloLifecycle);
        Assert.Null(clientLifecycle);
    }

    [Fact]
    public void UseEcsTaskHostEnvironmentStatisticsWithNullBuilderThrowsError()
    {
        // Arrange
        ISiloBuilder? siloBuilder = null;

        // Act
        var exception = Record.Exception(() =>
        {
            siloBuilder.UseEcsTaskHostEnvironmentStatistics();
        });

        // Assert
        Assert.NotNull(exception);
        var argumentNullException = Assert.IsType<ArgumentNullException>(exception);
        Assert.Equal("builder", argumentNullException.ParamName);
    }

    [Fact]
    public void UseEcsTaskHostEnvironmentStatisticsWithNullBuilderThrowsError2()
    {
        // Arrange
        Hosting.ISiloHostBuilder? siloBuilder = null;

        // Act
        var exception = Record.Exception(() =>
        {
            siloBuilder.UseEcsTaskHostEnvironmentStatistics();
        });

        // Assert
        Assert.NotNull(exception);
        var argumentNullException = Assert.IsType<ArgumentNullException>(exception);
        Assert.Equal("builder", argumentNullException.ParamName);
    }

    [Fact]
    public void UseEcsTaskHostEnvironmentStatisticsWithNullServiceCollectionThrowsError()
    {
        // Arrange
        var siloBuilder = new Mock<ISiloBuilder>(MockBehavior.Strict);
        siloBuilder
            .Setup(o => o.ConfigureServices(It.IsAny<Action<HostBuilderContext?, IServiceCollection?>>()))
            .Returns(siloBuilder.Object)
            .Callback((Action<HostBuilderContext?, IServiceCollection?> action) => action(null, null));

        // Act
        var exception = Record.Exception(() =>
        {
            siloBuilder.Object.UseEcsTaskHostEnvironmentStatistics();
        });

        // Assert
        Assert.NotNull(exception);
        var argumentNullException = Assert.IsType<ArgumentNullException>(exception);
        Assert.Equal("services", argumentNullException.ParamName);
    }

    [Fact]
    public void UseEcsTaskHostEnvironmentStatisticsWithNullServiceCollectionThrowsError2()
    {
        // Arrange
        var siloBuilder = new Mock<Hosting.ISiloHostBuilder>(MockBehavior.Strict);
        siloBuilder
            .Setup(o => o.ConfigureServices(It.IsAny<Action<Hosting.HostBuilderContext?, IServiceCollection?>>()))
            .Returns(siloBuilder.Object)
            .Callback((Action<Hosting.HostBuilderContext?, IServiceCollection?> action) => action(null, null));

        // Act
        var exception = Record.Exception(() =>
        {
            siloBuilder.Object.UseEcsTaskHostEnvironmentStatistics();
        });

        // Assert
        Assert.NotNull(exception);
        var argumentNullException = Assert.IsType<ArgumentNullException>(exception);
        Assert.Equal("services", argumentNullException.ParamName);
    }
}
