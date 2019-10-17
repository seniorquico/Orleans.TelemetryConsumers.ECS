using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Orleans.TelemetryConsumers.ECS
{
    /// <summary>Represents an ECS Task Metadata Endpoint client.</summary>
    public interface IEcsTaskMetadataClient
    {
        /// <summary>Asynchronously gets the Docker stats for the container running the current process.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result returns the Docker stats or
        ///     <c>null</c> if an error occurs.
        /// </returns>
        Task<EcsContainerStats?> GetContainerStatsAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     Asynchronously gets the collection of Docker stats for all containers belonging to the ECS Task running
        ///     the current process.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result returns the map of Docker container
        ///     identifier to Docker stats or <c>null</c> if an error occurs.
        /// </returns>
        Task<Dictionary<string, EcsContainerStats>?> GetTaskStatsAsync(CancellationToken cancellationToken);
    }
}
