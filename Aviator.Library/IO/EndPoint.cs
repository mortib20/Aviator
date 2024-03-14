using System.Text.Json.Serialization;

namespace Aviator.Library.IO;

public class EndPoint(string name, IoProtocol protocol, string host, int port)
{
    [JsonIgnore]
    public string Name { get; set; } = name;
    public IoProtocol Protocol { get; } = protocol;
    public string Host { get; } = host;
    public int Port { get; } = port;

    public override string ToString()
    {
        return $"{Name}:{Protocol}:{Host}:{Port}";
    }
}