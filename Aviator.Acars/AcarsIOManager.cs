using Aviator.Acars.Config;
using Aviator.Network.Input;
using Aviator.Network.Output;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars;

public class AcarsIoManager(ILogger<AcarsIoManager> logger, AcarsConfig config, InputBuilder inputBuilder, OutputBuilder outputBuilder)
{
    private readonly IInput _input = inputBuilder.Create(config.Input!.Protocol, config.Input.Host, config.Input.Port);
    private readonly Dictionary<AcarsType, List<IOutput>> _outputs = CreateOutputs(config, outputBuilder);

    private static Dictionary<AcarsType, List<IOutput>> CreateOutputs(AcarsConfig config, OutputBuilder outputBuilder)
    {
        var outputsConfig = config!.Outputs;

        if (outputsConfig == null)
        {
            throw new InvalidOperationException(nameof(outputsConfig));
        }
        
        var types = outputsConfig.Select(x => x.Type).Distinct().ToList();

        return types.ToDictionary(type => type, type => outputsConfig.Where(x => x.Type == type)
            .Select(x => outputBuilder.Create(x.Protocol, x.Host, x.Port))
            .ToList());
    }

    public async Task StartInputAsync(InputHandler onReceivedAsync, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Start Input on {Endpoint}", config.Input);
        
        await _input.ReceiveAsync(onReceivedAsync, cancellationToken).ConfigureAwait(false);
    }

    public async Task WriteToTypeAsync(AcarsType acarsType, byte[] buffer,
        CancellationToken cancellationToken = default)
    {
        await Task.Run(() => _outputs[acarsType].ToList().ForEach(s => s.WriteAsync(buffer, cancellationToken).ConfigureAwait(false)), cancellationToken).ConfigureAwait(false);
    }
}