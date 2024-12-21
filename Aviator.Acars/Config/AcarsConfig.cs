using Aviator.Network.Config;

namespace Aviator.Acars.Config;

public class AcarsConfig
{
    public const string Section = "Acars";
    public EndpointConfig? Input { get; set; }
    public List<OutputEndpointConfig>? Outputs { get; set; }
    public InfluxDbConfig? InfluxDb { get; set; }
}