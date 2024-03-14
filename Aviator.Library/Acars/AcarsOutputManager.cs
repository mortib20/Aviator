using System.Collections.Immutable;
using Aviator.Library.Acars.Settings;
using Aviator.Library.IO;
using Aviator.Library.IO.Output;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EndPoint = Aviator.Library.IO.EndPoint;

namespace Aviator.Library.Acars;

public class AcarsOutputManager(ILoggerFactory loggerFactory, IOptions<AcarsRouterSettings> options)
{
    private readonly Dictionary<AcarsType, List<IOutput>>
        _outputs = CreateOutputs(options.Value.Outputs, loggerFactory);

    public ImmutableDictionary<AcarsType, List<IOutput>> Outputs => _outputs.ToImmutableDictionary();

    public Dictionary<AcarsType, List<AcarsOutputState>> OutputStates
    {
        get
        {
            return _outputs
                .ToDictionary(keyValuePair => keyValuePair.Key, valuePair => valuePair.Value.Select(output =>
                    new AcarsOutputState
                    {
                        EndPoint = output.EndPoint,
                        State = output.State.ToString()
                    }).ToList());
        }
    }

    public async Task SendAsync(AcarsType type, byte[] buffer, CancellationToken cancellationToken = default)
    {
        foreach (var output in _outputs[type]) await output.SendAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    private static Dictionary<AcarsType, List<IOutput>> CreateOutputs(Outputs outputs, ILoggerFactory loggerFactory)
    {
        return new Dictionary<AcarsType, List<IOutput>>
        {
            [AcarsType.Aero] = CreateOutputList(AcarsType.Aero.ToString(), outputs.Aero, loggerFactory),
            [AcarsType.Vdl2] = CreateOutputList(AcarsType.Vdl2.ToString(), outputs.Vdl2, loggerFactory),
            [AcarsType.Hfdl] = CreateOutputList(AcarsType.Hfdl.ToString(), outputs.Hfdl, loggerFactory),
            [AcarsType.Acars] = CreateOutputList(AcarsType.Acars.ToString(), outputs.Acars, loggerFactory),
            [AcarsType.Iridium] = CreateOutputList(AcarsType.Iridium.ToString(), outputs.Iridium, loggerFactory)
        };
    }

    private static List<IOutput> CreateOutputList(string key, IEnumerable<EndPoint> endpoints,
        ILoggerFactory loggerFactory)
    {
        return endpoints.Select(e => CreateOutput(key, e, loggerFactory)).ToList();
    }

    private static IOutput CreateOutput(string key, EndPoint endpoint, ILoggerFactory loggerFactory)
    {
        if (endpoint.Host is null) throw new NullReferenceException("Host cannot be null");

        var endPoint = new EndPoint(endpoint.Protocol, endpoint.Host, endpoint.Port) { Name = key };

        return endpoint.Protocol switch
        {
            IoProtocol.Tcp => new TcpOutput(
                loggerFactory.CreateLogger($"{typeof(TcpOutput).FullName}:{endPoint.Host}:{endPoint.Port}"), endPoint),
            IoProtocol.Udp => new UdpOutput(
                loggerFactory.CreateLogger($"{typeof(UdpOutput).FullName}:{endPoint.Host}:{endPoint.Port}"), endPoint),
            _ => throw new ArgumentOutOfRangeException(nameof(endpoint.Protocol))
        };
    }
}