using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Statistics;
using Xunit;

namespace Orleans.TelemetryConsumers.ECS;

public sealed class ClientBuilderExtensionsTest : BaseAddressEnvironmentVariableTest
{
    public ClientBuilderExtensionsTest(BaseAddressEnvironmentVariableFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public void UseEcsTaskHostEnvironmentStatisticsConfiguresServices()
    {
        // Arrange
        this.SetBaseAddress("http://localhost/v3/");

        var serviceCollection = new ServiceCollection();

        var clientBuilder = new Mock<IClientBuilder>(MockBehavior.Strict);
        clientBuilder
            .Setup(o => o.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
            .Returns(clientBuilder.Object)
            .Callback((Action<HostBuilderContext?, IServiceCollection?> action) => action(null, serviceCollection));

        // Act
        clientBuilder.Object.UseEcsTaskHostEnvironmentStatistics();
        using var serviceProvider = serviceCollection.BuildServiceProvider();
        var hostEnvironmentStatistics = serviceProvider.GetService<IHostEnvironmentStatistics?>();
        var clientLifecycle = serviceProvider.GetService<ILifecycleParticipant<IClusterClientLifecycle>?>();
        var siloLifecycle = serviceProvider.GetService<ILifecycleParticipant<ISiloLifecycle>?>();

        // Assert
        Assert.NotNull(hostEnvironmentStatistics);
        Assert.NotNull(clientLifecycle);
        Assert.Null(siloLifecycle);
    }

    [Fact]
    public void UseEcsTaskHostEnvironmentStatisticsDoesNotConfigureServices()
    {
        // Arrange
        this.ResetBaseAddress();

        var serviceCollection = new ServiceCollection();

        var clientBuilder = new Mock<IClientBuilder>(MockBehavior.Strict);
        clientBuilder
            .Setup(o => o.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
            .Returns(clientBuilder.Object)
            .Callback((Action<HostBuilderContext?, IServiceCollection?> action) => action(null, serviceCollection));

        // Act
        clientBuilder.Object.UseEcsTaskHostEnvironmentStatistics();
        using var serviceProvider = serviceCollection.BuildServiceProvider();
        var hostEnvironmentStatistics = serviceProvider.GetService<IHostEnvironmentStatistics?>();
        var clientLifecycle = serviceProvider.GetService<ILifecycleParticipant<IClusterClientLifecycle>?>();
        var siloLifecycle = serviceProvider.GetService<ILifecycleParticipant<ISiloLifecycle>?>();

        // Assert
        Assert.Null(hostEnvironmentStatistics);
        Assert.Null(clientLifecycle);
        Assert.Null(siloLifecycle);
    }

    [Fact]
    public void UseEcsTaskHostEnvironmentStatisticsWithNullBuilderThrowsError()
    {
        // Arrange
        IClientBuilder? clientBuilder = null;

        // Act
        var exception = Record.Exception(() =>
        {
            clientBuilder.UseEcsTaskHostEnvironmentStatistics();
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
        var clientBuilder = new Mock<IClientBuilder>(MockBehavior.Strict);
        clientBuilder
            .Setup(o => o.ConfigureServices(It.IsAny<Action<HostBuilderContext?, IServiceCollection?>>()))
            .Returns(clientBuilder.Object)
            .Callback((Action<HostBuilderContext?, IServiceCollection?> action) => action(null, null));

        // Act
        var exception = Record.Exception(() =>
        {
            clientBuilder.Object.UseEcsTaskHostEnvironmentStatistics();
        });

        // Assert
        Assert.NotNull(exception);
        var argumentNullException = Assert.IsType<ArgumentNullException>(exception);
        Assert.Equal("services", argumentNullException.ParamName);
    }
}
