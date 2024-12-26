using Aviator.Network.Config;

namespace Aviator.Acars.Config;

public class OutputEndpointConfig : EndpointConfig
{
    public List<AcarsType> Types { get; set; } = [];
}