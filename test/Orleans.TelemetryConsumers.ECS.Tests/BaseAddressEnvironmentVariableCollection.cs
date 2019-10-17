using Xunit;

namespace Orleans.TelemetryConsumers.ECS
{
    [CollectionDefinition(NAME)]
    public sealed class BaseAddressEnvironmentVariableCollection :
        ICollectionFixture<BaseAddressEnvironmentVariableFixture>
    {
        public const string NAME = "ECS_CONTAINER_METADATA_URI environment variable collection";
    }
}
