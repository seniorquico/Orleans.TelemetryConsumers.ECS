using Newtonsoft.Json;
using Xunit;

namespace Orleans.TelemetryConsumers.ECS;

public sealed class EcsTaskMetadataSerializerProviderTest
{
    [Fact]
    public void GetSerializer()
    {
        // Act
        var serializerProvider = new EcsTaskMetadataSerializerProvider();
        var serializer = serializerProvider.Serializer;

        // Assert
        Assert.NotNull(serializer);

        Assert.Equal(DateParseHandling.None, serializer.DateParseHandling);
        Assert.Equal(PreserveReferencesHandling.None, serializer.PreserveReferencesHandling);
        Assert.Equal(TypeNameHandling.None, serializer.TypeNameHandling);
    }
}
