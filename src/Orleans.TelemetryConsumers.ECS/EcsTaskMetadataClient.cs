using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Orleans.TelemetryConsumers.ECS
{
    /// <summary>
    ///     An ECS Task Metadata Endpoint client. This client may be used to get the Docker container statistics.
    /// </summary>
    internal sealed class EcsTaskMetadataClient :
        IEcsTaskMetadataClient
    {
        /// <summary>The HTTP request timeout (in seconds).</summary>
        private const int TIMEOUT = 5;

        /// <summary>The HTTP client.</summary>
        private readonly HttpClient httpClient;

        /// <summary>The logger.</summary>
        private readonly ILogger<EcsTaskMetadataClient>? logger;

        /// <summary>The JSON serializer.</summary>
        private readonly JsonSerializer serializer;

        /// <summary>Initializes a new instance of the <see cref="EcsTaskMetadataClient"/> class.</summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="baseAddressProvider">The base address provider.</param>
        /// <param name="serializerProvider">The JSON serializer provider.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="httpClient"/>, <paramref name="baseAddressProvider"/>, or
        ///     <paramref name="serializerProvider"/> are <c>null</c>.
        /// </exception>
        public EcsTaskMetadataClient(
            HttpClient? httpClient,
            IEcsTaskMetadataBaseAddressProvider? baseAddressProvider,
            IEcsTaskMetadataSerializerProvider? serializerProvider,
            ILogger<EcsTaskMetadataClient>? logger)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (baseAddressProvider == null)
            {
                throw new ArgumentNullException(nameof(baseAddressProvider));
            }

            if (serializerProvider == null)
            {
                throw new ArgumentNullException(nameof(serializerProvider));
            }

            httpClient.BaseAddress = baseAddressProvider.BaseAddress;
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
            httpClient.Timeout = TimeSpan.FromSeconds(TIMEOUT);

            this.httpClient = httpClient;
            this.logger = logger;
            this.serializer = serializerProvider.Serializer;
        }

        /// <summary>Asynchronously gets the Docker stats for the container running the current process.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result returns the Docker stats or
        ///     <c>null</c> if an error occurs.
        /// </returns>
        public Task<EcsContainerStats?> GetContainerStatsAsync(CancellationToken cancellationToken) =>
            this.GetAsync<EcsContainerStats>("stats", cancellationToken);

        /// <summary>
        ///     Asynchronously gets the collection of Docker stats for all containers belonging to the ECS Task running
        ///     the current process.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result returns the map of Docker container
        ///     identifier to Docker stats or <c>null</c> if an error occurs.
        /// </returns>
        public Task<Dictionary<string, EcsContainerStats>?> GetTaskStatsAsync(CancellationToken cancellationToken) =>
            this.GetAsync<Dictionary<string, EcsContainerStats>>("task/stats", cancellationToken);

        /// <summary>Asynchronously gets a JSON document from the ECS Task Metadata Endpoint.</summary>
        /// <typeparam name="T">The type of the deserialized JSON document.</typeparam>
        /// <param name="relativePath">
        ///     The relative path of the request appended to the ECS Task Metadata Endpoint base address.
        /// </param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result returns the deserialized JSON document
        ///     or <c>null</c> if an error occurs.
        /// </returns>
        private async Task<T?> GetAsync<T>(string relativePath, CancellationToken cancellationToken)
            where T : class
        {
            using var response = await this.httpClient.GetAsync(
                relativePath,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            try
            {
                using var jsonTextReader = new JsonTextReader(new StreamReader(await response.Content.ReadAsStreamAsync().ConfigureAwait(false)))
                {
                    DateParseHandling = DateParseHandling.None,
                    MaxDepth = 5
                };

                return this.serializer.Deserialize<T>(jsonTextReader);
            }
            catch (JsonException e)
            {
                this.logger?.LogError(e, "Failed to parse ECS Task Metadata Endpoint JSON document");
                return default;
            }
        }
    }
}
