using Newtonsoft.Json;

namespace Orleans.TelemetryConsumers.ECS
{
    /// <summary>
    ///     A partial representation of the JSON document returned by the "stats" and "task/stats" APIs of the ECS Task
    ///     Metadata Endpoint.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class EcsContainerStats
    {
        /// <summary>Gets or sets the current CPU stats.</summary>
        /// <value>The current CPU stats.</value>
        [JsonProperty(PropertyName = "cpu_stats")]
        public EcsContainerCpuStats? CpuStats { get; set; }

        /// <summary>Gets or sets the current memory stats.</summary>
        /// <value>The current memory stats.</value>
        [JsonProperty(PropertyName = "memory_stats")]
        public EcsContainerMemoryStats? MemoryStats { get; set; }

        /// <summary>Gets or sets the previous CPU stats.</summary>
        /// <value>The previous CPU stats.</value>
        [JsonProperty(PropertyName = "precpu_stats")]
        public EcsContainerCpuStats? PreviousCpuStats { get; set; }
    }
}
