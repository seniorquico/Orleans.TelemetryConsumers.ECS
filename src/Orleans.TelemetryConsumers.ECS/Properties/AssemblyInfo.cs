using System.Runtime.CompilerServices;

/*
 * The "DynamicProxyGenAssembly2" a well-known assembly name used by Moq (and Castle DynamicProxy, one of its underlying
 * libraries) to introduce dynamic types. This must be declared to allow Moq to generate dynamic types that extend or
 * inherit internal types.
 */
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Orleans.TelemetryConsumers.ECS.Tests")]
[assembly: CLSCompliant(false)]
