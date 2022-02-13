using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly.CircuitBreaker;
using Xunit;

namespace Orleans.TelemetryConsumers.ECS;

public sealed class ServiceCollectionExtensionsTest : BaseAddressEnvironmentVariableTest
{
    public ServiceCollectionExtensionsTest(BaseAddressEnvironmentVariableFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task InjectsPollyCircuitBreakerPolicy()
    {
        // Arrange
        this.SetBaseAddress("http://localhost/v3/");

        var counter = new RequestsCounter();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(counter);
        serviceCollection.AddTransient<IHttpMessageHandlerBuilderFilter, RequestsFilter>();
        serviceCollection.AddEcsTaskMetadataClientFactory();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var client = serviceProvider.GetService<IEcsTaskMetadataClient>();

        // Act
        var firstResponse = await client!.GetContainerStatsAsync(CancellationToken.None).ConfigureAwait(true);
        var firstRequestCount = counter.Requests;

        var secondResponse = await Assert.ThrowsAsync<BrokenCircuitException<HttpResponseMessage>>(() => client!.GetContainerStatsAsync(CancellationToken.None)).ConfigureAwait(true);
        var secondRequestCount = counter.Requests;

        var thirdResponse = await Assert.ThrowsAsync<BrokenCircuitException<HttpResponseMessage>>(() => client!.GetContainerStatsAsync(CancellationToken.None)).ConfigureAwait(true);
        var thirdRequestCount = counter.Requests;

        // Assert
        Assert.Null(firstResponse);
        Assert.Equal(4, firstRequestCount);

        Assert.NotNull(secondResponse);
        Assert.Equal(5, secondRequestCount);

        Assert.NotNull(thirdResponse);
        Assert.Equal(5, thirdRequestCount);
    }

    [Fact]
    public async Task InjectsPollyRetryPolicy()
    {
        // Arrange
        this.SetBaseAddress("http://localhost/v3/");

        var counter = new RequestsCounter();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(counter);
        serviceCollection.AddTransient<IHttpMessageHandlerBuilderFilter, RequestsFilter>();
        serviceCollection.AddEcsTaskMetadataClientFactory();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var client = serviceProvider.GetService<IEcsTaskMetadataClient>();

        // Act
        var response = await client!.GetContainerStatsAsync(CancellationToken.None).ConfigureAwait(true);

        // Assert
        Assert.Null(response);
        Assert.Equal(4, counter.Requests);
    }

    private sealed class RequestsCounter
    {
        private int requests;

        public int Requests =>
            this.requests;

        public void IncrementRequests() =>
            Interlocked.Increment(ref this.requests);
    }

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Created by DI container")]
    private sealed class RequestsFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly RequestsCounter counter;

        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Because it's not smart enough?")]
        [SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "Created by DI container")]
        public RequestsFilter(RequestsCounter counter) =>
            this.counter = counter;

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next) =>
            (builder) =>
            {
                next(builder);
                builder.AdditionalHandlers.Add(new RequestsHandler(this.counter));
            };
    }

    private sealed class RequestsHandler : DelegatingHandler
    {
        private readonly RequestsCounter counter;

        public RequestsHandler(RequestsCounter counter) =>
            this.counter = counter;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.counter.IncrementRequests();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        }
    }
}
