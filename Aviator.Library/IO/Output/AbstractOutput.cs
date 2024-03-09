namespace Aviator.Library.IO.Output;

public abstract class AbstractOutput : IOutput
{
    public abstract bool Connected { get; }
    public abstract Task SendAsync(byte[] buffer, CancellationToken cancellationToken = default);
}