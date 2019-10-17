// using System;
// using System.Threading.Tasks;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
// using Newtonsoft.Json;
// using Orleans.Hosting;
// using Orleans.TelemetryConsumers.ECS.Grains;
// using Orleans.TestingHost;
// using Xunit;
// using Xunit.Abstractions;
//
// namespace Orleans.TelemetryConsumers.ECS
// {
//     public sealed class ClusterLogTest : IDisposable
//     {
//         private static ITestOutputHelper outputHelper;
//
//         private readonly TestCluster cluster;
//
//         public ClusterLogTest(ITestOutputHelper outputHelper)
//         {
//             ClusterLogTest.outputHelper = outputHelper;
//
//             var builder = new TestClusterBuilder();
//             builder.Options.ConfigureFileLogging = true;
//             builder.AddSiloBuilderConfigurator<SiloBuilder>();
//             this.cluster = builder.Build();
//         }
//
//         public void Dispose()
//         {
//             this.cluster.StopAllSilos();
//             outputHelper = null;
//         }
//
//         [Fact]
//         public async Task Test()
//         {
//             var grain = this.cluster.Client.GetGrain<ITestGrain>(0);
//             var (metadata, stats) = await grain.RunAsync();
//             outputHelper.WriteLine("Metadata: %s", JsonConvert.SerializeObject(metadata));
//             outputHelper.WriteLine("Stats: %s", JsonConvert.SerializeObject(stats));
//         }
//
//         private sealed class SiloBuilder : ISiloBuilderConfigurator
//         {
//             public void Configure(ISiloHostBuilder hostBuilder)
//             {
//                 if (hostBuilder == null)
//                 {
//                     throw new ArgumentNullException(nameof(hostBuilder));
//                 }
//
//                 if (outputHelper != null)
//                 {
//                     hostBuilder.ConfigureServices(services =>
//                     {
//                         services.AddLogging(logBuilder =>
//                         {
//                             logBuilder.AddXUnit(outputHelper);
//                         });
//                     });
//                 }
//
//                 hostBuilder.UseEcsTaskHostEnvironmentStatistics();
//             }
//         }
//     }
// }
