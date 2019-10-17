using System;
using Microsoft.Extensions.DependencyInjection;

namespace Orleans.TelemetryConsumers.ECS
{
    /// <summary>
    ///     An ECS Task Metadata Endpoint client factory. These clients may be used to get the Docker container
    ///     statistics.
    /// </summary>
    internal sealed class EcsTaskMetadataClientFactory :
        IEcsTaskMetadataClientFactory
    {
        /// <summary>The service provider.</summary>
        private readonly IServiceProvider serviceProvider;

        /// <summary>Initializes a new instance of the <see cref="EcsTaskMetadataClientFactory"/> class.</summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="serviceProvider"/> is <c>null</c>.</exception>
        public EcsTaskMetadataClientFactory(IServiceProvider? serviceProvider) =>
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        /// <summary>Creates a new ECS Task Metadata client.</summary>
        /// <returns>The new ECS Task Metadata client.</returns>
        /// <exception cref="InvalidOperationException">
        ///     If the service provider is not correctly configured.
        /// </exception>
        public IEcsTaskMetadataClient Create() =>
            this.serviceProvider.GetRequiredService<IEcsTaskMetadataClient>();
    }
}
