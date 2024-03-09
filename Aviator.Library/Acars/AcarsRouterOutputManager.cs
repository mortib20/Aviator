using System.Net;
using Aviator.Library.Acars.Settings;
using Aviator.Library.IO.Output;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aviator.Library.Acars;

public class AcarsRouterOutputManager(ILoggerFactory loggerFactory, IOptions<AcarsRouterSettings> options)
{
    private readonly Dictionary<AcarsType, List<IOutput>> _outputs = CreateOutputs(options.Value.Outputs, loggerFactory);
    
    public async Task SendAsync(AcarsType type, byte[] buffer, CancellationToken cancellationToken = default)
    {
        foreach (var output in _outputs[type])
        {
            await output.SendAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
    }
    
    private static Dictionary<AcarsType, List<IOutput>> CreateOutputs(Outputs outputs, ILoggerFactory loggerFactory)
    {
        return new Dictionary<AcarsType, List<IOutput>>()
        {
            [AcarsType.Aero] = CreateOutputList(outputs.Aero, loggerFactory),
            [AcarsType.Vdl2] = CreateOutputList(outputs.Vdl2, loggerFactory),
            [AcarsType.Hfdl] = CreateOutputList(outputs.Hfdl, loggerFactory),
            [AcarsType.Acars] = CreateOutputList(outputs.Acars, loggerFactory),
            [AcarsType.Iridium] = CreateOutputList(outputs.Iridium, loggerFactory),
        };
    }

    private static List<IOutput> CreateOutputList(IEnumerable<Endpoint> endpoints, ILoggerFactory loggerFactory)
    {
        return endpoints.Select(e => CreateOutput(e, loggerFactory)).ToList();
    }
    
    private static IOutput CreateOutput(Endpoint endpoint, ILoggerFactory loggerFactory)
    {
        if (endpoint.Address is null)
        {
            throw new NullReferenceException("Address cannot be null");
        }

        var dnsEndPoint = new DnsEndPoint(endpoint.Address, endpoint.Port);
        
        return endpoint.Protocol switch
        {
            OutputProtocol.Tcp => new TcpOutput(loggerFactory.CreateLogger($"{typeof(TcpOutput).FullName}:{dnsEndPoint.Host}:{dnsEndPoint.Port}"), dnsEndPoint),
            OutputProtocol.Udp => new UdpOutput(loggerFactory.CreateLogger($"{typeof(UdpOutput).FullName}:{dnsEndPoint.Host}:{dnsEndPoint.Port}"), dnsEndPoint),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}