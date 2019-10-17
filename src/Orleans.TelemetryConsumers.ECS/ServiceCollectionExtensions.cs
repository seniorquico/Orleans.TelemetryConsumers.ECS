using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace Orleans.TelemetryConsumers.ECS
{
    /// <summary>
    ///     Provides extension methods for <see cref="IServiceCollection"/> that register an ECS Task Metadata Endpoint
    ///     client factory.
    /// </summary>
    internal static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Registers an ECS Task Metadata Endpoint client factory and its dependencies if a supported ECS
        ///     environment is detected.
        /// </summary>
        /// <param name="services">The collection of service descriptors.</param>
        public static bool AddEcsTaskMetadataClientFactory(this IServiceCollection services)
        {
            IEcsTaskMetadataBaseAddressProvider baseAddressProvider;
            try
            {
                baseAddressProvider = new EcsTaskMetadataBaseAddressProvider();
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            services.AddSingleton(baseAddressProvider);
            services.AddSingleton<IEcsTaskMetadataSerializerProvider, EcsTaskMetadataSerializerProvider>();
            services
                .AddHttpClient<IEcsTaskMetadataClient, EcsTaskMetadataClient>()
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    UseCookies = false,
                    UseDefaultCredentials = false,
                    UseProxy = false,
                })
                .AddPolicyHandler((serviceProvider, request) => HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(
                        3, _ => TimeSpan.FromMilliseconds(250), (response, timespan, retryCount, context) =>
                        {
                            var logger = serviceProvider.GetService<ILogger<IEcsTaskMetadataClient>>();
                            logger?.LogWarning(
                                "Delaying for {delay}ms before making retry request #{retry}.",
                                timespan.TotalMilliseconds,
                                retryCount);
                        }))
                .AddPolicyHandler((serviceProvider, request) => HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .CircuitBreakerAsync(3, TimeSpan.FromSeconds(30), (response, timespan) =>
                        {
                            var logger = serviceProvider.GetService<ILogger<IEcsTaskMetadataClient>>();
                            logger?.LogWarning("Delaying for 30s before making additional requests.");
                        }, () => { }));
            services.AddSingleton<IEcsTaskMetadataClientFactory, EcsTaskMetadataClientFactory>();
            return true;
        }
    }
}
