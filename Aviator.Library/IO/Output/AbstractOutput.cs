using System.Net;

namespace Aviator.Library.IO.Output;

public abstract class AbstractOutput(EndPoint endPoint) : IOutput
{
    public OutputState State { get; private set; } = OutputState.Initialized;
    public EndPoint EndPoint { get; private set; } = endPoint;

    public abstract Task SendAsync(byte[] buffer, CancellationToken cancellationToken = default);

    protected void StateInitialized() => State = OutputState.Initialized;
    protected void StateConfigured() => State = OutputState.Configured;
    protected void StateRunning() => State = OutputState.Running;
}