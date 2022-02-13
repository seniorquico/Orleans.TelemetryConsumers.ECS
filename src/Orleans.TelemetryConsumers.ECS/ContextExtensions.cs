using Microsoft.Extensions.Logging;
using Polly;

namespace Orleans.TelemetryConsumers.ECS;

/// <summary>
///     Provides extension methods for <see cref="Context"/> that enable sharing an <see cref="ILogger"/> instance.
/// </summary>
internal static class ContextExtensions
{
    /// <summary>The context key.</summary>
    private const string LoggerKey = "Logger";

    /// <summary>Gets a shared <see cref="ILogger"/> from the context.</summary>
    /// <param name="context">The context.</param>
    /// <returns>An <see cref="ILogger"/> or <c>null</c> if one was not provided.</returns>
    public static ILogger? GetLogger(this Context context)
    {
        if (context.TryGetValue(LoggerKey, out var logger))
        {
            return logger as ILogger;
        }

        return null;
    }

    /// <summary>Puts a shared <see cref="ILogger"/> in the context.</summary>
    /// <param name="context">The context.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>The context.</returns>
    public static Context WithLogger(this Context context, ILogger? logger)
    {
        if (logger != null)
        {
            context[LoggerKey] = logger;
        }

        return context;
    }
}
