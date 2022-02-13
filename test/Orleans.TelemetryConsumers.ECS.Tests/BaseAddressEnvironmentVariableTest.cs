using Xunit;

namespace Orleans.TelemetryConsumers.ECS;

[Collection(BaseAddressEnvironmentVariableCollection.NAME)]
public abstract class BaseAddressEnvironmentVariableTest : IDisposable
{
    private readonly BaseAddressEnvironmentVariableFixture fixture;

    private bool disposed;

    protected BaseAddressEnvironmentVariableTest(BaseAddressEnvironmentVariableFixture? fixture) =>
        this.fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this.disposed)
        {
            return;
        }

        if (disposing)
        {
            this.fixture.Reset();
        }

        this.disposed = true;
    }

    protected void ResetBaseAddress() =>
        this.fixture.Reset();

    protected void SetBaseAddress(string? value) =>
        this.fixture.Set(value);
}
