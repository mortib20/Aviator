using Aviator.Network.Input;

namespace Aviator.Network.Output;

public class OutputBuilder : IBuilder<IOutput>
{
    private static TcpOutput CreateTcpOutput(string host, int port) => new(host, port);

    private static UdpOutput CreateUdpOutput(string host, int port) => new(host, port);

    public IOutput Create(Protocol protocol, string host, int port)
    {
        return protocol switch
        {
            Protocol.Tcp => CreateTcpOutput(host, port),
            Protocol.Udp => CreateUdpOutput(host, port),
            _ => throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null)
        };
    }
}