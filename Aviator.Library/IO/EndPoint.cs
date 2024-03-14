using System.Text.Json.Serialization;

namespace Aviator.Library.IO;

public class EndPoint(IoProtocol protocol, string host, int port)
{
    [JsonIgnore]
    public string Name { get; init; } = string.Empty;
    public IoProtocol Protocol { get; } = protocol;
    public string Host { get; } = host;
    public int Port { get; } = port;

    public override string ToString()
    {
        return $"{Name}:{Protocol}:{Host}:{Port}";
    }
}