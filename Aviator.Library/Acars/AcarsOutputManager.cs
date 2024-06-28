using System.Collections.Immutable;
using System.Text.Json;
using Aviator.Library.Acars.Settings;
using Aviator.Library.IO;
using Aviator.Library.IO.Output;
using Aviator.Library.Metrics;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EndPoint = Aviator.Library.IO.EndPoint;

namespace Aviator.Library.Acars;

public class AcarsOutputManager(ILoggerFactory loggerFactory, IOptions<AcarsRouterSettings> options, AviatorMetrics metrics, IHubContext<AcarsHub> acarsHub) : IOutputManager
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new();

    private readonly Dictionary<AcarsType, List<IOutput>>
        _outputs = CreateOutputs(options.Value.Outputs, loggerFactory, metrics);

    public ImmutableDictionary<AcarsType, List<IOutput>> Outputs => _outputs.ToImmutableDictionary();

    public async Task SendAcarsHub(BasicAcars basicAcars, CancellationToken cancellationToken = default)
    {
        await acarsHub.Clients.All.SendAsync("acars", JsonSerializer.Serialize(basicAcars, _jsonSerializerOptions), cancellationToken).ConfigureAwait(false);
    }
    
    public async Task SendAsync(AcarsType type, byte[] buffer, CancellationToken cancellationToken = default)
    {
        foreach (var output in _outputs[type])
        {
            await output.SendAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
    }

    private static Dictionary<AcarsType, List<IOutput>> CreateOutputs(Dictionary<string, List<EndPoint>> outputs, ILoggerFactory loggerFactory, AviatorMetrics metrics)
    {
        var s = outputs
            .ToDictionary(
                k => Enum.Parse<AcarsType>(k.Key),
                v => v.Value.Select(s => CreateOutput(v.Key, s, loggerFactory, metrics)).ToList());

        return s;
    }

    private static IOutput CreateOutput(string key, EndPoint endpoint, ILoggerFactory loggerFactory, AviatorMetrics metrics)
    {
        if (endpoint.Host is null) throw new ArgumentNullException(endpoint.Host);

        var endPoint = new EndPoint(endpoint.Protocol, endpoint.Host, endpoint.Port) { Name = key };

        return endpoint.Protocol switch
        {
            IoProtocol.Tcp => new TcpOutput(
                loggerFactory.CreateLogger($"Tcp:{key}:{endPoint.Host}:{endPoint.Port}"), endPoint, metrics),
            IoProtocol.Udp => new UdpOutput(
                loggerFactory.CreateLogger($"Udp:{key}:{endPoint.Host}:{endPoint.Port}"), endPoint, metrics),
            _ => throw new ArgumentOutOfRangeException(endpoint.Protocol.ToString())
        };
    }
}

public interface IOutputManager
{
    public Task SendAsync(AcarsType type, byte[] buffer, CancellationToken cancellationToken = default);
}