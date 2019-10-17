using System;

namespace Orleans.TelemetryConsumers.ECS
{
    /// <summary>A provider of the ECS Task Metadata Endpoint base address.</summary>
    internal sealed class EcsTaskMetadataBaseAddressProvider :
        IEcsTaskMetadataBaseAddressProvider
    {
        /// <summary>The name of the <c>ECS_CONTAINER_METADATA_URI</c> environment variable.</summary>
        private const string ENVIRONMENT_VARIABLE = "ECS_CONTAINER_METADATA_URI";

        /// <summary>
        ///     The exception message used when the <c>ECS_CONTAINER_METADATA_URI</c> environment variable value is
        ///     invalid.
        /// </summary>
        private const string INVALID_ERROR = "The ECS_CONTAINER_METADATA_URI environment variable is not a valid URL.";

        /// <summary>
        ///     The exception message used when the <c>ECS_CONTAINER_METADATA_URI</c> environment variable value is null
        ///     or empty.
        /// </summary>
        private const string UNDEFINED_ERROR = "The ECS_CONTAINER_METADATA_URI environment variable is not defined.";

        /// <summary>
        ///     Initializes a new instance of the <see cref="EcsTaskMetadataBaseAddressProvider"/> class. The ECS Task
        ///     Metadata Endpoint base address is parsed from the <c>ECS_CONTAINER_METADATA_URI</c> environment variable.
        ///     This variable is defined when running in a supported ECS environment.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     If the <c>ECS_CONTAINER_METADATA_URI</c> environment variable is undefined or not a valid URL.
        /// </exception>
        public EcsTaskMetadataBaseAddressProvider()
        {
            var rawUri = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE);
            if (string.IsNullOrEmpty(rawUri))
            {
                throw new InvalidOperationException(UNDEFINED_ERROR);
            }

            if (!rawUri.EndsWith("/", StringComparison.Ordinal))
            {
                rawUri += "/";
            }

            if (!Uri.TryCreate(rawUri, UriKind.Absolute, out var parsedUri) ||
                (parsedUri.Scheme != Uri.UriSchemeHttp && parsedUri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException(INVALID_ERROR);
            }

            this.BaseAddress = parsedUri;
        }

        /// <summary>
        ///     Gets the ECS Task Metadata Endpoint base address. The base address is parsed from the
        ///     <c>ECS_CONTAINER_METADATA_URI</c> environment variable. This variable is defined when running in a
        ///     supported ECS environment.
        /// </summary>
        /// <returns>The ECS Task Metadata Endpoint base address.</returns>
        public Uri BaseAddress { get; }
    }
}
