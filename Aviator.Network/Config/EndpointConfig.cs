namespace Aviator.Network.Config;

public class EndpointConfig
{
    public Protocol Protocol { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }

    public override string ToString()
    {
        return $"{Protocol}://{Host}:{Port}";
    }
}