using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using Xunit.Abstractions;

namespace Orleans.TelemetryConsumers.ECS;

public sealed class EcsTaskMetadataClientTest : IDisposable
{
    private const string BASE_ADDRESS = "http://localhost/v3/";

    private readonly ILoggerFactory loggerFactory;

    public EcsTaskMetadataClientTest(ITestOutputHelper outputHelper)
    {
        if (outputHelper == null)
        {
            throw new ArgumentNullException(nameof(outputHelper));
        }

        this.loggerFactory = new LoggerFactory();
        this.loggerFactory.AddXUnit(outputHelper);
    }

    private interface IHttpMessageHandlerProtectedMembers
    {
        void Dispose(bool disposing);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }

    public void Dispose() =>
        this.loggerFactory.Dispose();

    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [Theory]
    public async Task GetContainerStatsAsyncWithErrorResponseReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var serializerProvider = new EcsTaskMetadataSerializerProvider();

        var baseAddressProviderMock = new Mock<IEcsTaskMetadataBaseAddressProvider>(MockBehavior.Strict);
        baseAddressProviderMock
            .SetupGet(o => o.BaseAddress)
            .Returns(new Uri(BASE_ADDRESS, UriKind.Absolute));

        using var httpResponseMessage = new HttpResponseMessage(statusCode);
        var httpRequestMessages = new List<HttpRequestMessage>();
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock
            .As<IDisposable>()
            .Setup(o => o.Dispose());
        httpMessageHandlerMock
            .Protected()
            .As<IHttpMessageHandlerProtectedMembers>()
            .Setup(o => o.Dispose(It.IsAny<bool>()));
        httpMessageHandlerMock
            .Protected()
            .As<IHttpMessageHandlerProtectedMembers>()
            .Setup(o => o.SendAsync(Capture.In(httpRequestMessages), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponseMessage);

        using var httpClient = new HttpClient(httpMessageHandlerMock.Object);

        var client = new EcsTaskMetadataClient(httpClient, baseAddressProviderMock.Object, serializerProvider, new Logger<EcsTaskMetadataClient>(this.loggerFactory));

        // Act
        var stats = await client.GetContainerStatsAsync(CancellationToken.None).ConfigureAwait(true);

        // Assert
        Assert.Null(stats);

        var httpRequestMessage = Assert.Single(httpRequestMessages);
        Assert.NotNull(httpRequestMessage);
        Assert.Equal("http://localhost/v3/stats", httpRequestMessage?.RequestUri?.AbsoluteUri);
    }

    [InlineData("")]
    [InlineData("null")]
    [InlineData("0")]
    [InlineData("\"\"")]
    [InlineData("[]")]
    [InlineData("{")]
    [InlineData("{\"cpu_stats\":{}]")]
    [Theory]
    public async Task GetContainerStatsAsyncWithInvalidResponseReturnsNull(string responseContent)
    {
        // Arrange
        var serializerProvider = new EcsTaskMetadataSerializerProvider();

        var baseAddressProviderMock = new Mock<IEcsTaskMetadataBaseAddressProvider>(MockBehavior.Strict);
        baseAddressProviderMock
            .SetupGet(o => o.BaseAddress)
            .Returns(new Uri(BASE_ADDRESS, UriKind.Absolute));

        using var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json"),
        };
        var httpRequestMessages = new List<HttpRequestMessage>();
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock
            .As<IDisposable>()
            .Setup(o => o.Dispose());
        httpMessageHandlerMock
            .Protected()
            .As<IHttpMessageHandlerProtectedMembers>()
            .Setup(o => o.Dispose(It.IsAny<bool>()));
        httpMessageHandlerMock
            .Protected()
            .As<IHttpMessageHandlerProtectedMembers>()
            .Setup(o => o.SendAsync(Capture.In(httpRequestMessages), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponseMessage);

        using var httpClient = new HttpClient(httpMessageHandlerMock.Object);

        var client = new EcsTaskMetadataClient(httpClient, baseAddressProviderMock.Object, serializerProvider, new Logger<EcsTaskMetadataClient>(this.loggerFactory));

        // Act
        var stats = await client.GetContainerStatsAsync(CancellationToken.None).ConfigureAwait(true);

        // Assert
        Assert.Null(stats);

        var httpRequestMessage = Assert.Single(httpRequestMessages);
        Assert.NotNull(httpRequestMessage);
        Assert.Equal("http://localhost/v3/stats", httpRequestMessage?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetContainerStatsAsyncWithOkResponseReturnsNotNull()
    {
        // Arrange
        var serializerProvider = new EcsTaskMetadataSerializerProvider();

        var baseAddressProviderMock = new Mock<IEcsTaskMetadataBaseAddressProvider>(MockBehavior.Strict);
        baseAddressProviderMock
            .SetupGet(o => o.BaseAddress)
            .Returns(new Uri(BASE_ADDRESS, UriKind.Absolute));

        using var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"cpu_stats\":{\"cpu_usage\":{\"total_usage\":2000},\"system_cpu_usage\":200000}," +
                    "\"memory_stats\":{\"limit\":25000,\"usage\":2500},\"precpu_stats\":{\"cpu_usage\":" +
                    "{\"total_usage\":1000},\"system_cpu_usage\":100000}}",
                Encoding.UTF8,
                "application/json"),
        };
        var httpRequestMessages = new List<HttpRequestMessage>();
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock
            .As<IDisposable>()
            .Setup(o => o.Dispose());
        httpMessageHandlerMock
            .Protected()
            .As<IHttpMessageHandlerProtectedMembers>()
            .Setup(o => o.Dispose(It.IsAny<bool>()));
        httpMessageHandlerMock
            .Protected()
            .As<IHttpMessageHandlerProtectedMembers>()
            .Setup(o => o.SendAsync(Capture.In(httpRequestMessages), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponseMessage);

        using var httpClient = new HttpClient(httpMessageHandlerMock.Object);

        var client = new EcsTaskMetadataClient(httpClient, baseAddressProviderMock.Object, serializerProvider, new Logger<EcsTaskMetadataClient>(this.loggerFactory));

        // Act
        var stats = await client.GetContainerStatsAsync(CancellationToken.None).ConfigureAwait(true);

        // Assert
        Assert.NotNull(stats);
        Assert.NotNull(stats!.CpuStats);
        Assert.NotNull(stats!.CpuStats!.CpuUsage);
        Assert.True(stats!.CpuStats!.CpuUsage!.TotalUsage.HasValue);
        Assert.Equal((ulong)2000, stats!.CpuStats!.CpuUsage!.TotalUsage!.Value);
        Assert.True(stats!.CpuStats!.SystemCpuUsage.HasValue);
        Assert.Equal((ulong)200000, stats!.CpuStats!.SystemCpuUsage!.Value);
        Assert.NotNull(stats!.MemoryStats);
        Assert.True(stats!.MemoryStats!.Limit.HasValue);
        Assert.Equal((ulong)25000, stats!.MemoryStats!.Limit!.Value);
        Assert.True(stats!.MemoryStats!.Usage.HasValue);
        Assert.Equal((ulong)2500, stats!.MemoryStats!.Usage!.Value);
        Assert.NotNull(stats!.PreviousCpuStats);
        Assert.NotNull(stats!.PreviousCpuStats!.CpuUsage);
        Assert.True(stats!.PreviousCpuStats!.CpuUsage!.TotalUsage.HasValue);
        Assert.Equal((ulong)1000, stats!.PreviousCpuStats!.CpuUsage!.TotalUsage!.Value);
        Assert.True(stats!.PreviousCpuStats!.SystemCpuUsage.HasValue);
        Assert.Equal((ulong)100000, stats!.PreviousCpuStats!.SystemCpuUsage!.Value);

        var httpRequestMessage = Assert.Single(httpRequestMessages);
        Assert.NotNull(httpRequestMessage);
        Assert.Equal("http://localhost/v3/stats", httpRequestMessage?.RequestUri?.AbsoluteUri);
    }

    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [Theory]
    public async Task GetTaskStatsAsyncWithErrorResponseReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var serializerProvider = new EcsTaskMetadataSerializerProvider();

        var baseAddressProviderMock = new Mock<IEcsTaskMetadataBaseAddressProvider>(MockBehavior.Strict);
        baseAddressProviderMock
            .SetupGet(o => o.BaseAddress)
            .Returns(new Uri(BASE_ADDRESS, UriKind.Absolute));

        using var httpResponseMessage = new HttpResponseMessage(statusCode);
        var httpRequestMessages = new List<HttpRequestMessage>();
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock
            .As<IDisposable>()
            .Setup(o => o.Dispose());
        httpMessageHandlerMock
            .Protected()
            .As<IHttpMessageHandlerProtectedMembers>()
            .Setup(o => o.Dispose(It.IsAny<bool>()));
        httpMessageHandlerMock
            .Protected()
            .As<IHttpMessageHandlerProtectedMembers>()
            .Setup(o => o.SendAsync(Capture.In(httpRequestMessages), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponseMessage);

        using var httpClient = new HttpClient(httpMessageHandlerMock.Object);

        var client = new EcsTaskMetadataClient(httpClient, baseAddressProviderMock.Object, serializerProvider, new Logger<EcsTaskMetadataClient>(this.loggerFactory));

        // Act
        var taskStats = await client.GetTaskStatsAsync(CancellationToken.None).ConfigureAwait(true);

        // Assert
        Assert.Null(taskStats);

        var httpRequestMessage = Assert.Single(httpRequestMessages);
        Assert.NotNull(httpRequestMessage);
        Assert.Equal("http://localhost/v3/task/stats", httpRequestMessage?.RequestUri?.AbsoluteUri);
    }

    [InlineData("")]
    [InlineData("null")]
    [InlineData("0")]
    [InlineData("\"\"")]
    [InlineData("[]")]
    [InlineData("{")]
    [InlineData("{\"1b6b094d94dc4ad5977cb1793db0cfeb\":{\"cpu_stats\":{}}]")]
    [Theory]
    public async Task GetTaskStatsAsyncWithInvalidResponseReturnsNull(string responseContent)
    {
        // Arrange
        var serializerProvider = new EcsTaskMetadataSerializerProvider();

        var baseAddressProviderMock = new Mock<IEcsTaskMetadataBaseAddressProvider>(MockBehavior.Strict);
        baseAddressProviderMock
            .SetupGet(o => o.BaseAddress)
            .Returns(new Uri(BASE_ADDRESS, UriKind.Absolute));

        using var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json"),
        };
        var httpRequestMessages = new List<HttpRequestMessage>();
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock
            .As<IDisposable>()
            .Setup(o => o.Dispose());
        httpMessageHandlerMock
            .Protected()
            .As<IHttpMessageHandlerProtectedMembers>()
            .Setup(o => o.Dispose(It.IsAny<bool>()));
        httpMessageHandlerMock
            .Protected()
            .As<IHttpMessageHandlerProtectedMembers>()
            .Setup(o => o.SendAsync(Capture.In(httpRequestMessages), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponseMessage);

        using var httpClient = new HttpClient(httpMessageHandlerMock.Object);

        var client = new EcsTaskMetadataClient(httpClient, baseAddressProviderMock.Object, serializerProvider, new Logger<EcsTaskMetadataClient>(this.loggerFactory));

        // Act
        var taskStats = await client.GetTaskStatsAsync(CancellationToken.None).ConfigureAwait(true);

        // Assert
        Assert.Null(taskStats);

        var httpRequestMessage = Assert.Single(httpRequestMessages);
        Assert.NotNull(httpRequestMessage);
        Assert.Equal("http://localhost/v3/task/stats", httpRequestMessage?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    public async Task GetTaskStatsAsyncWithOkResponseReturnsNotNull()
    {
        // Arrange
        var serializerProvider = new EcsTaskMetadataSerializerProvider();

        var baseAddressProviderMock = new Mock<IEcsTaskMetadataBaseAddressProvider>(MockBehavior.Strict);
        baseAddressProviderMock
            .SetupGet(o => o.BaseAddress)
            .Returns(new Uri(BASE_ADDRESS, UriKind.Absolute));

        using var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"1b6b094d94dc4ad5977cb1793db0cfeb\":{\"cpu_stats\":{\"cpu_usage\":{\"total_usage\":2000}," +
                    "\"system_cpu_usage\":200000},\"memory_stats\":{\"limit\":25000,\"usage\":2500}," +
                    "\"precpu_stats\":{\"cpu_usage\":{\"total_usage\":1000},\"system_cpu_usage\":100000}}," +
                    "\"b6853bccfb6249e4bb6df75aae59e1b1\":null}",
                Encoding.UTF8,
                "application/json"),
        };
        var httpRequestMessages = new List<HttpRequestMessage>();
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock
            .As<IDisposable>()
            .Setup(o => o.Dispose());
        httpMessageHandlerMock
            .Protected()
            .As<IHttpMessageHandlerProtectedMembers>()
            .Setup(o => o.Dispose(It.IsAny<bool>()));
        httpMessageHandlerMock
            .Protected()
            .As<IHttpMessageHandlerProtectedMembers>()
            .Setup(o => o.SendAsync(Capture.In(httpRequestMessages), It.IsAny<CancellationToken>()))
            .ReturnsAsync(httpResponseMessage);

        using var httpClient = new HttpClient(httpMessageHandlerMock.Object);

        var client = new EcsTaskMetadataClient(httpClient, baseAddressProviderMock.Object, serializerProvider, new Logger<EcsTaskMetadataClient>(this.loggerFactory));

        // Act
        var taskStats = await client.GetTaskStatsAsync(CancellationToken.None).ConfigureAwait(true);

        // Assert
        Assert.NotNull(taskStats);
        Assert.Equal(2, taskStats!.Count);

        var stats = Assert.Contains("1b6b094d94dc4ad5977cb1793db0cfeb", (IDictionary<string, EcsContainerStats>)taskStats);
        Assert.NotNull(stats);
        Assert.NotNull(stats!.CpuStats);
        Assert.NotNull(stats!.CpuStats!.CpuUsage);
        Assert.True(stats!.CpuStats!.CpuUsage!.TotalUsage.HasValue);
        Assert.Equal((ulong)2000, stats!.CpuStats!.CpuUsage!.TotalUsage!.Value);
        Assert.True(stats!.CpuStats!.SystemCpuUsage.HasValue);
        Assert.Equal((ulong)200000, stats!.CpuStats!.SystemCpuUsage!.Value);
        Assert.NotNull(stats!.MemoryStats);
        Assert.True(stats!.MemoryStats!.Limit.HasValue);
        Assert.Equal((ulong)25000, stats!.MemoryStats!.Limit!.Value);
        Assert.True(stats!.MemoryStats!.Usage.HasValue);
        Assert.Equal((ulong)2500, stats!.MemoryStats!.Usage!.Value);
        Assert.NotNull(stats!.PreviousCpuStats);
        Assert.NotNull(stats!.PreviousCpuStats!.CpuUsage);
        Assert.True(stats!.PreviousCpuStats!.CpuUsage!.TotalUsage.HasValue);
        Assert.Equal((ulong)1000, stats!.PreviousCpuStats!.CpuUsage!.TotalUsage!.Value);
        Assert.True(stats!.PreviousCpuStats!.SystemCpuUsage.HasValue);
        Assert.Equal((ulong)100000, stats!.PreviousCpuStats!.SystemCpuUsage!.Value);

        stats = Assert.Contains("b6853bccfb6249e4bb6df75aae59e1b1", (IDictionary<string, EcsContainerStats>)taskStats);
        Assert.Null(stats);

        var httpRequestMessage = Assert.Single(httpRequestMessages);
        Assert.NotNull(httpRequestMessage);
        Assert.Equal("http://localhost/v3/task/stats", httpRequestMessage?.RequestUri?.AbsoluteUri);
    }

    [Fact]
    [SuppressMessage("Minor Code Smell", "S1481:Unused local variables should be removed")]
    public void InstantiateNullBaseAddressProvierThrows()
    {
        // Arrange
        using var httpClient = new HttpClient();

        // Act
        var exception = Record.Exception(() =>
        {
            var provider = new EcsTaskMetadataClient(httpClient, null, null, null);
        });

        // Assert
        Assert.NotNull(exception);
        var argumentNullException = Assert.IsType<ArgumentNullException>(exception);
        Assert.Equal("baseAddressProvider", argumentNullException.ParamName);
    }

    [Fact]
    [SuppressMessage("Minor Code Smell", "S1481:Unused local variables should be removed")]
    public void InstantiateNullHttpClientThrows()
    {
        // Act
        var exception = Record.Exception(() =>
        {
            var provider = new EcsTaskMetadataClient(null, null, null, null);
        });

        // Assert
        Assert.NotNull(exception);
        var argumentNullException = Assert.IsType<ArgumentNullException>(exception);
        Assert.Equal("httpClient", argumentNullException.ParamName);
    }

    [Fact]
    [SuppressMessage("Minor Code Smell", "S1481:Unused local variables should be removed")]
    public void InstantiateNullSerializerProviderThrows()
    {
        // Arrange
        var baseAddressProviderMock = new Mock<IEcsTaskMetadataBaseAddressProvider>(MockBehavior.Strict);

        using var httpClient = new HttpClient();

        // Act
        var exception = Record.Exception(() =>
        {
            var provider = new EcsTaskMetadataClient(httpClient, baseAddressProviderMock.Object, null, null);
        });

        // Assert
        Assert.NotNull(exception);
        var argumentNullException = Assert.IsType<ArgumentNullException>(exception);
        Assert.Equal("serializerProvider", argumentNullException.ParamName);
    }
}
