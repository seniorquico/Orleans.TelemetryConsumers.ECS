﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Statistics;

namespace Orleans.TelemetryConsumers.ECS
{
    /// <summary>
    ///     Provides extension methods for <see cref="IClientBuilder"/> that register an ECS host environment statistics
    ///     provider.
    /// </summary>
    public static class ClientBuilderExtensions
    {
        /// <summary>
        ///     Registers an ECS host environment statistics provider if a supported ECS environment is detected.
        /// </summary>
        /// <param name="builder">The client builder.</param>
        /// <returns>The client builder.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="builder"/> is <c>null</c>.</exception>
        public static IClientBuilder UseEcsTaskHostEnvironmentStatistics(this IClientBuilder? builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.ConfigureServices(ConfigureServices);
        }

        /// <summary>
        ///     Registers an ECS host environment statistics provider and its dependencies if a supported ECS environment
        ///     is detected.
        /// </summary>
        /// <param name="services">The collection of service descriptors.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="services"/> is <c>null</c>.</exception>
        private static void ConfigureServices(IServiceCollection? services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (!services.AddEcsTaskMetadataClientFactory())
            {
                return;
            }

            services.AddSingleton<EcsTaskHostEnvironmentStatistics>();
            services.AddSingleton<IHostEnvironmentStatistics>(s => s.GetRequiredService<EcsTaskHostEnvironmentStatistics>());
            services.AddSingleton<ILifecycleParticipant<IClusterClientLifecycle>>(s => s.GetRequiredService<EcsTaskHostEnvironmentStatistics>());
        }
    }
}
