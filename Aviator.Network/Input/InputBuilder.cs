using Microsoft.Extensions.Logging;

namespace Aviator.Network.Input;

public class InputBuilder(ILoggerFactory loggerFactory) : IBuilder<IInput>
{
    private static UdpInput CreateUdpInput(string host, int port) => new(host, port);

    private TcpInput CreateTcpInput(string host, int port) => new(loggerFactory.CreateLogger<TcpInput>(), host, port);

    public IInput Create(Protocol protocol, string host, int port)
    {
        return protocol switch
        {
            Protocol.Tcp => CreateTcpInput(host, port),
            Protocol.Udp => CreateUdpInput(host, port),
            _ => throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null)
        };
    }
}