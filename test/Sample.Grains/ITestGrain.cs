using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.TelemetryConsumers.ECS;

namespace Sample.Grains
{
    public interface ITestGrain : IGrainWithIntegerKey
    {
        Task<EcsContainerStats> GetContainerStatsAsync();

        Task<Dictionary<string, EcsContainerStats>> GetTaskStatsAsync();
    }
}
