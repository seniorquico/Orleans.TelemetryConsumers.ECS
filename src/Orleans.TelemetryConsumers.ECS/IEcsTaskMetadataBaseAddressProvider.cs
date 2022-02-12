namespace Orleans.TelemetryConsumers.ECS;

/// <summary>Represents a provider of the ECS Task Metadata Endpoint base address.</summary>
internal interface IEcsTaskMetadataBaseAddressProvider
{
    /// <summary>Gets the ECS Task Metadata Endpoint base address.</summary>
    /// <returns>The ECS Task Metadata Endpoint base address.</returns>
    /// <remarks>The base address must end with a trailing slash.</remarks>
    Uri BaseAddress { get; }
}
