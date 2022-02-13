using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Orleans.TelemetryConsumers.ECS;

public sealed class EcsTaskMetadataClientFactoryTest
{
    [Fact]
    public void Create()
    {
        // Arrange
        var client = new TestEcsTaskMetadataClient();

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IEcsTaskMetadataClient>(client)
            .BuildServiceProvider();

        // Act
        var actualClientFactory = new EcsTaskMetadataClientFactory(serviceProvider);
        var actualClient = actualClientFactory.Create();

        // Assert
        Assert.Same(client, actualClient);
    }

    [Fact]
    public void InstantiateWithoutServiceProviderThrowsError() =>
        Assert.Throws<ArgumentNullException>("serviceProvider", () => new EcsTaskMetadataClientFactory(null));

    private sealed class TestEcsTaskMetadataClient : IEcsTaskMetadataClient
    {
        public Task<EcsContainerStats?> GetContainerStatsAsync(CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<Dictionary<string, EcsContainerStats>?> GetTaskStatsAsync(CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }
}
