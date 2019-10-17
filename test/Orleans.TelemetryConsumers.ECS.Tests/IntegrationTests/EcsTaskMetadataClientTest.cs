// using System;
// using System.Threading.Tasks;
// using Newtonsoft.Json;
// using Orleans.TelemetryConsumers.ECS.Grains;
// using Orleans.TestingHost;
// using Xunit;
// using Xunit.Abstractions;
//
// namespace Orleans.TelemetryConsumers.ECS
// {
//     public sealed class EcsTaskMetadataClientTest : IClassFixture<ClusterFixture>
//     {
//         private readonly TestCluster cluster;
//         private readonly ITestOutputHelper testOutput;
//
//         public EcsTaskMetadataClientTest(ClusterFixture clusterFixture, ITestOutputHelper testOutput)
//         {
//             if (clusterFixture == null)
//             {
//                 throw new ArgumentNullException(nameof(clusterFixture));
//             }
//
//             this.cluster = clusterFixture.Cluster;
//             this.testOutput = testOutput ?? throw new ArgumentNullException(nameof(testOutput));
//         }
//
//         [Fact]
//         public async Task Test()
//         {
//             var grain = this.cluster.Client.GetGrain<ITestGrain>(0);
//             var (metadata, stats) = await grain.RunAsync();
//             this.testOutput.WriteLine("Metadata: %s", JsonConvert.SerializeObject(metadata));
//             this.testOutput.WriteLine("Stats: %s", JsonConvert.SerializeObject(stats));
//         }
//     }
// }
