namespace Aviator.Network.Output;

public interface IOutput
{
    public string EndPoint { init; get; }
    
    public ValueTask WriteAsync(byte[] buffer, CancellationToken cancellationToken = default);
}