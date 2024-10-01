using Aviator.Acars.Config;
using Aviator.Network.Input;
using Aviator.Network.Output;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars;

public class AcarsIoManager(ILogger<AcarsIoManager> logger, AcarsConfig config, InputBuilder inputBuilder, OutputBuilder outputBuilder)
{
    private readonly IInput _input = inputBuilder.Create(config.Input!.Protocol, config.Input.Host, config.Input.Port);

    private readonly Dictionary<AcarsType, List<IOutput>> _outputs = CreateOutputs();

    private static Dictionary<AcarsType, List<IOutput>> CreateOutputs(AcarsConfig config)
    {
        var outputEndpointConfigs = config.Outputs;
        var outputs = new Dictionary<AcarsType, List<IOutput>>();

        // do output here
        
        return outputs;
    }

    public Task StartInputAsync(InputHandler onReceivedAsync, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Start Input on {Endpoint}", config.Input);
        
        return _input.ReceiveAsync(onReceivedAsync, cancellationToken);
    }
}