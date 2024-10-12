namespace Aviator.Network.Output;

public interface IOutput
{
    public ValueTask WriteAsync(byte[] buffer, CancellationToken cancellationToken = default);
}