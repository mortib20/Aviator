using Microsoft.Extensions.Logging;

namespace Aviator.Network.Input;

public class InputBuilder(ILoggerFactory loggerFactory) : IBuilder<IInput>
{
    public IInput Create(Protocol protocol, string host, int port)
    {
        return protocol switch
        {
            Protocol.Tcp => CreateTcpInput(host, port),
            Protocol.Udp => CreateUdpInput(host, port),
            _ => throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null)
        };
    }

    private static UdpInput CreateUdpInput(string host, int port)
    {
        return new UdpInput(host, port);
    }

    private TcpInput CreateTcpInput(string host, int port)
    {
        return new TcpInput(loggerFactory.CreateLogger<TcpInput>(), host, port);
    }
}