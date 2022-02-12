using Newtonsoft.Json;

namespace Orleans.TelemetryConsumers.ECS;

/// <summary>
///     A partial representation of the container CPU stats included in the JSON document returned by the "stats" and
///     "task/stats" APIs of the ECS Task Metadata Endpoint.
/// </summary>
[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public sealed class EcsContainerCpuUsageStats
{
    /// <summary>Gets or sets the container CPU usage in nanoseconds.</summary>
    /// <value>The container CPU usage in nanoseconds.</value>
    [JsonProperty(PropertyName = "total_usage")]
    public ulong? TotalUsage { get; set; }
}
