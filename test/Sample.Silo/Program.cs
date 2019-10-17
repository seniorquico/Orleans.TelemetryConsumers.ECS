using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.TelemetryConsumers.ECS;
using Sample.Grains;

namespace Sample.Silo
{
    internal static class Program
    {
        private static bool RunningInContainer => "true".Equals(
            Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
            StringComparison.OrdinalIgnoreCase);

        private static Task Main() =>
            new HostBuilder()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddConsole(options =>
                    {
                        options.DisableColors = RunningInContainer;
                    });

                    loggingBuilder.SetMinimumLevel(LogLevel.Information);

                    loggingBuilder.AddFilter("System", LogLevel.Warning);
                    loggingBuilder.AddFilter("Microsoft", LogLevel.Warning);
                    loggingBuilder.AddFilter("Orleans", LogLevel.Warning);

                    loggingBuilder.AddFilter("System.Net.Http.HttpClient", LogLevel.Trace);
                    loggingBuilder.AddFilter("Orleans.RuntimeSiloLogStatistics", LogLevel.Information);
                    loggingBuilder.AddFilter("Orleans.TelemetryConsumers.ECS", LogLevel.Trace);
                })
                .UseOrleans((context, siloBuilder) =>
                {
                    siloBuilder
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "dev";
                            options.ServiceId = "dev";
                        })
                        .Configure<StatisticsOptions>(options =>
                        {
                            options.LogWriteInterval = TimeSpan.FromSeconds(30);
                        })
                        .ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000)
                        .UseLocalhostClustering()
                        .UseEcsTaskHostEnvironmentStatistics()
                        .ConfigureApplicationParts(parts =>
                        {
                            parts.AddApplicationPart(typeof(ITestGrain).Assembly).WithReferences();
                        })
                        .UseDashboard(options =>
                        {
                            options.CounterUpdateIntervalMs = 5000;
                            options.HideTrace = true;
                            options.Host = "*";
                            options.HostSelf = true;
                            options.Port = 8080;
                        });
                })
                .RunConsoleAsync();
    }
}
