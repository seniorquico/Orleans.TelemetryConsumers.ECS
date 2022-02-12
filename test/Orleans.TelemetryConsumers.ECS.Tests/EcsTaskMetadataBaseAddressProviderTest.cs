using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Orleans.TelemetryConsumers.ECS;

public sealed class EcsTaskMetadataBaseAddressProviderTest : BaseAddressEnvironmentVariableTest
{
    public EcsTaskMetadataBaseAddressProviderTest(BaseAddressEnvironmentVariableFixture fixture) : base(fixture)
    {
    }

    [InlineData("http://localhost/v3")]
    [InlineData("http://localhost/v3/")]
    [Theory]
    public void GetBaseAddress(string value)
    {
        // Arrange
        this.SetBaseAddress(value);

        // Act
        var baseAddressProvider = new EcsTaskMetadataBaseAddressProvider();
        var baseAddress = baseAddressProvider.BaseAddress;

        // Assert
        Assert.Equal(new Uri("http://localhost/v3/", UriKind.Absolute), baseAddress);
    }

    [InlineData("/v3/", "is not a valid URL")]
    [InlineData("http", "is not a valid URL")]
    [InlineData("ftp://localhost/v3/", "is not a valid URL")]
    [InlineData(null, "is not defined")]
    [InlineData("", "is not defined")]
    [Theory]
    [SuppressMessage("Minor Code Smell", "S1481:Unused local variables should be removed")]
    public void InstantiateThrowsError(string value, string messageFragment)
    {
        // Arrange
        if (value == null)
        {
            this.ResetBaseAddress();
        }
        else
        {
            this.SetBaseAddress(value);
        }

        // Act
        var exception = Record.Exception(() =>
        {
            var provider = new EcsTaskMetadataBaseAddressProvider();
        });

        // Assert
        Assert.NotNull(exception);
        var invalidOperationException = Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains(messageFragment, invalidOperationException.Message, StringComparison.Ordinal);
    }
}
