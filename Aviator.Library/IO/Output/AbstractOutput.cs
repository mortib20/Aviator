using Aviator.Library.Metrics;

namespace Aviator.Library.IO.Output;

public abstract class AbstractOutput(EndPoint endPoint) : IOutput
{
    public OutputState State { get; private set; } = OutputState.Initialized;
    public EndPoint EndPoint { get; private set; } = endPoint;

    public abstract Task SendAsync(byte[] buffer, CancellationToken cancellationToken = default);

    protected void StateToInitialized() => State = OutputState.Initialized;
    protected void StateToStopped()
    {
        State = OutputState.Stopped;
        AviatorRouterMetrics.SetOutputStatus("", EndPoint.ToString(), false);
    }

    protected void StateToRunning()
    {
        State = OutputState.Running;
        AviatorRouterMetrics.SetOutputStatus("", EndPoint.ToString(), true);
    }
}