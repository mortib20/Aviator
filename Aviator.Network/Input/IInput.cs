namespace Aviator.Network.Input;

public delegate Task InputHandler(byte[] input, CancellationToken cancel);

public interface IInput
{
    public Task ReceiveAsync(InputHandler onReceive, CancellationToken cancellationToken = default);
}