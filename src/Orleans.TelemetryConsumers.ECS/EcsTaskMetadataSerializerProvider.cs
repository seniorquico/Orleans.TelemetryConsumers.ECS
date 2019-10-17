using Newtonsoft.Json;

namespace Orleans.TelemetryConsumers.ECS
{
    /// <summary>
    ///     A provider of the JSON serializer to use when deserializing the ECS Task Metadata Endpoint responses. The
    ///     JSON serializer is configured to not accept special Json.NET properties (e.g. "$id" and "$type"), not to
    ///     parse Date objects from strings.
    /// </summary>
    internal sealed class EcsTaskMetadataSerializerProvider :
        IEcsTaskMetadataSerializerProvider
    {
        /// <summary>
        ///     Gets the JSON serializer to use when deserializing the ECS Task Metadata Endpoint responses.
        /// </summary>
        /// <value>The JSON serializer.</value>
        public JsonSerializer Serializer { get; } = new JsonSerializer
        {
            DateParseHandling = DateParseHandling.None,
            MaxDepth = 5,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            TypeNameHandling = TypeNameHandling.None
        };
    }
}
