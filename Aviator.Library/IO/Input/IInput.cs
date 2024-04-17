namespace Aviator.Library.IO.Input;

public interface IInput
{
    public Task StartReceiveAsync(int port, Action<byte[], CancellationToken> handler,  CancellationToken cancellationToken = default);
}