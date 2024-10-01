namespace Aviator.Network.Output;

public interface IOutput
{
    public Task WriteAsync(byte[] buffer, CancellationToken cancellationToken = default);
}