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
    private readonly Dictionary<AcarsType, List<IOutput>>
        _outputs = CreateOutputs(options.Value.Outputs, loggerFactory, metrics);

    public ImmutableDictionary<AcarsType, List<IOutput>> Outputs => _outputs.ToImmutableDictionary();

    public async Task SendAcarsHub(BasicAcars basicAcars, CancellationToken cancellationToken = default)
    {
        await acarsHub.Clients.All.SendAsync("acars", JsonSerializer.Serialize(basicAcars), cancellationToken).ConfigureAwait(false);
    }
    
    public async Task SendAsync(AcarsType type, byte[] buffer, CancellationToken cancellationToken = default)
    {
        foreach (var output in _outputs[type]) await output.SendAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    private static Dictionary<AcarsType, List<IOutput>> CreateOutputs(Outputs outputs, ILoggerFactory loggerFactory, AviatorMetrics metrics)
    {
        return new Dictionary<AcarsType, List<IOutput>>
        {
            [AcarsType.Aero] = CreateOutputList(AcarsType.Aero.ToString(), outputs.Aero, loggerFactory, metrics),
            [AcarsType.Vdl2] = CreateOutputList(AcarsType.Vdl2.ToString(), outputs.Vdl2, loggerFactory, metrics),
            [AcarsType.Hfdl] = CreateOutputList(AcarsType.Hfdl.ToString(), outputs.Hfdl, loggerFactory, metrics),
            [AcarsType.Acars] = CreateOutputList(AcarsType.Acars.ToString(), outputs.Acars, loggerFactory, metrics),
            [AcarsType.Iridium] = CreateOutputList(AcarsType.Iridium.ToString(), outputs.Iridium, loggerFactory, metrics)
        };
    }

    private static List<IOutput> CreateOutputList(string key, IEnumerable<EndPoint> endpoints,
        ILoggerFactory loggerFactory, AviatorMetrics metrics)
    {
        return endpoints.Select(e => CreateOutput(key, e, loggerFactory, metrics)).ToList();
    }

    private static IOutput CreateOutput(string key, EndPoint endpoint, ILoggerFactory loggerFactory, AviatorMetrics metrics)
    {
        if (endpoint.Host is null) throw new NullReferenceException("Host cannot be null");

        var endPoint = new EndPoint(endpoint.Protocol, endpoint.Host, endpoint.Port) { Name = key };

        return endpoint.Protocol switch
        {
            IoProtocol.Tcp => new TcpOutput(
                loggerFactory.CreateLogger($"{typeof(TcpOutput).FullName}:{endPoint.Host}:{endPoint.Port}"), endPoint, metrics),
            IoProtocol.Udp => new UdpOutput(
                loggerFactory.CreateLogger($"{typeof(UdpOutput).FullName}:{endPoint.Host}:{endPoint.Port}"), endPoint, metrics),
            _ => throw new ArgumentOutOfRangeException(nameof(endpoint.Protocol))
        };
    }
}

public interface IOutputManager
{
    public Task SendAsync(AcarsType type, byte[] buffer, CancellationToken cancellationToken = default);
}