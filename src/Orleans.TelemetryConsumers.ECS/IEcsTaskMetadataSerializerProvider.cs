using Newtonsoft.Json;

namespace Orleans.TelemetryConsumers.ECS
{
    /// <summary>
    ///     Represents a provider of the JSON serializer to use when deserializing the ECS Task Metadata Endpoint
    ///     responses.
    /// </summary>
    internal interface IEcsTaskMetadataSerializerProvider
    {
        /// <summary>
        ///     Gets the JSON serializer to use when deserializing the ECS Task Metadata Endpoint responses.
        /// </summary>
        /// <value>The JSON serializer.</value>
        JsonSerializer Serializer { get; }
    }
}
