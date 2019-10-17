using Newtonsoft.Json;

namespace Orleans.TelemetryConsumers.ECS
{
    /// <summary>
    ///     A partial representation of the CPU stats included in the JSON document returned by the "stats" and
    ///     "task/stats" APIs of the ECS Task Metadata Endpoint.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class EcsContainerCpuStats
    {
        /// <summary>Gets or sets the container CPU usage structure.</summary>
        /// <value>The container CPU usage structure.</value>
        [JsonProperty(PropertyName = "cpu_usage")]
        public EcsContainerCpuUsageStats? CpuUsage { get; set; }

        /// <summary>Gets or sets the system CPU usage in nanoseconds.</summary>
        /// <value>The system CPU usage in nanoseconds.</value>
        [JsonProperty(PropertyName = "system_cpu_usage")]
        public ulong? SystemCpuUsage { get; set; }
    }
}
