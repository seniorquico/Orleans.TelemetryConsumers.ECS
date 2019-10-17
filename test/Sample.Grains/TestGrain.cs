using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.TelemetryConsumers.ECS;

namespace Sample.Grains
{
    internal sealed class TestGrain : Grain, ITestGrain
    {
        private readonly IEcsTaskMetadataClient client;

        public TestGrain(IEcsTaskMetadataClient client) =>
            this.client = client ?? throw new ArgumentNullException(nameof(client));

        public Task<EcsContainerStats> GetContainerStatsAsync() =>
            this.client.GetContainerStatsAsync(CancellationToken.None);

        public Task<Dictionary<string, EcsContainerStats>> GetTaskStatsAsync() =>
            this.client.GetTaskStatsAsync(CancellationToken.None);
    }
}
