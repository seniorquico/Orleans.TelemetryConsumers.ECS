using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace Orleans.TelemetryConsumers.ECS;

/// <summary>
///     Provides extension methods for <see cref="IServiceCollection"/> that register an ECS Task Metadata Endpoint
///     client factory.
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>The pre-compiled warning message logged when the circuit breaker policy is triggered.</summary>
    private static readonly Action<ILogger, Exception?> LogCircuitBreaker = LoggerMessage.Define(LogLevel.Warning, 2, "Delaying for 30s before making additional requests.");

    /// <summary>The pre-compiled warning message logged when the retry policy is triggered.</summary>
    private static readonly Action<ILogger, double, int, Exception?> LogRetry = LoggerMessage.Define<double, int>(LogLevel.Warning, 1, "Delaying for {Delay}ms before making retry request #{Retry}.");

    /// <summary>
    ///     Registers an ECS Task Metadata Endpoint client factory and its dependencies if a supported ECS environment
    ///     is detected.
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

        // The circuit breaker policy is stateful, and it's necessary to use a shared instance.
        var circuitBreaker = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(60), (response, timespan, context) =>
            {
                var logger = context.GetLogger();
                if (logger != null)
                {
                    LogCircuitBreaker(logger, null);
                }
            }, (context) => { });
        services
            .AddHttpClient<IEcsTaskMetadataClient, EcsTaskMetadataClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                UseCookies = false,
                UseDefaultCredentials = false,
                UseProxy = false,
            })
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(250), (response, timespan, retryCount, context) =>
            {
                var logger = context.GetLogger();
                if (logger != null)
                {
                    LogRetry(logger, timespan.TotalMilliseconds, retryCount, null);
                }
            }))
            .AddPolicyHandler(circuitBreaker);

        services.AddSingleton<IEcsTaskMetadataClientFactory, EcsTaskMetadataClientFactory>();
        return true;
    }
}
