namespace Orleans.TelemetryConsumers.ECS;

public sealed class BaseAddressEnvironmentVariableFixture : IDisposable
{
    private const string ENVIRONMENT_VARIABLE = "ECS_CONTAINER_METADATA_URI";

    private readonly string? originalValue;

    private bool changed;

    public BaseAddressEnvironmentVariableFixture()
    {
        this.originalValue = Environment.GetEnvironmentVariable(ENVIRONMENT_VARIABLE);
        if (this.originalValue != null)
        {
            this.Reset();
        }
    }

    public void Dispose()
    {
        if (this.changed)
        {
            Environment.SetEnvironmentVariable(ENVIRONMENT_VARIABLE, this.originalValue);
        }
    }

    public void Reset() =>
        this.Set(null);

    public void Set(string? value)
    {
        this.changed = true;
        Environment.SetEnvironmentVariable(ENVIRONMENT_VARIABLE, value);
    }
}
