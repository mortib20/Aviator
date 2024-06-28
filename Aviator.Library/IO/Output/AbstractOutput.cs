using Aviator.Library.Metrics;
using Aviator.Library.Metrics.Prometheus;

namespace Aviator.Library.IO.Output;

public abstract class AbstractOutput(EndPoint endPoint, AviatorMetrics metrics) : IOutput, IDisposable
{
    public bool Enabled { get; protected set; } = true;
    public OutputState State { get; private set; } = OutputState.Initialized;
    public DateTime? LastError { get; protected set; } = null;
    public EndPoint EndPoint { get; private set; } = endPoint;

    public abstract Task SendAsync(byte[] buffer, CancellationToken cancellationToken = default);

    protected void StateToInitialized() => State = OutputState.Initialized;
    protected void StateToStopped()
    {
        State = OutputState.Stopped;
        metrics.SetOutputStatus(EndPoint.ToString(), false);
    }

    protected void StateToRunning()
    {
        State = OutputState.Running;
        metrics.SetOutputStatus(EndPoint.ToString(), true);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            State = OutputState.Disposed;
        }
    }
}