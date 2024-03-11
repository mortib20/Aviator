namespace Aviator.Library.IO;

public class EndPoint(IoProtocol protocol, string host, int port)
{
    public IoProtocol Protocol { get; } = protocol;
    public string Host { get; } = host;
    public int Port { get; } = port;
}