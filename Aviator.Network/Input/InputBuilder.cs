using Microsoft.Extensions.Logging;

namespace Aviator.Network.Input;

public class InputBuilder(ILoggerFactory loggerFactory)
{
    private UdpInput CreateUdpInput(string host, int port)
    {
        return new UdpInput(loggerFactory.CreateLogger<UdpInput>(), host, port);
    }

    public BaseInput Create(Protocol protocol, string host, int port)
    {
        return protocol switch
        {
            Protocol.Tcp => throw new NotImplementedException(),
            Protocol.Udp => CreateUdpInput(host, port),
            _ => throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null)
        };
    }
}