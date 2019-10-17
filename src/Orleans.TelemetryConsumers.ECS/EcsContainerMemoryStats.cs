using Newtonsoft.Json;

namespace Orleans.TelemetryConsumers.ECS
{
    /// <summary>
    ///     A partial representation of the memory stats included in the JSON document returned by the "stats" and
    ///     "task/stats" APIs of the ECS Task Metadata Endpoint.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class EcsContainerMemoryStats
    {
        /// <summary>Gets or sets the container memory limit in bytes.</summary>
        /// <value>The container memory limit in bytes.</value>
        [JsonProperty(PropertyName = "limit")]
        public ulong? Limit { get; set; }

        /// <summary>Gets or sets the container memory usage in bytes.</summary>
        /// <value>The container memory usage in bytes.</value>
        [JsonProperty(PropertyName = "usage")]
        public ulong? Usage { get; set; }
    }
}
