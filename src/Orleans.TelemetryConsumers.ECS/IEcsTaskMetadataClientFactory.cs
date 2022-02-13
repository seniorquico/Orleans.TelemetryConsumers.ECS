namespace Orleans.TelemetryConsumers.ECS;

/// <summary>Represents an ECS Task Metadata Endpoint client factory.</summary>
internal interface IEcsTaskMetadataClientFactory
{
    /// <summary>Creates and returns a new ECS Task Metadata Endpoint client.</summary>
    /// <returns>The new ECS Task Metadata Endpoint client.</returns>
    IEcsTaskMetadataClient Create();
}
