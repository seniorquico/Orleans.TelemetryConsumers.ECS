using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Orleans.TelemetryConsumers.ECS;

[CollectionDefinition(NAME)]
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "The class name identifies the Xunit fixture scope.")]
public sealed class BaseAddressEnvironmentVariableCollection : ICollectionFixture<BaseAddressEnvironmentVariableFixture>
{
    public const string NAME = "ECS_CONTAINER_METADATA_URI environment variable collection";
}
