using Aviator.Library.Acars;
using Prometheus;

namespace Aviator.Library.Metrics.Prometheus;

public class PrometheusMetrics : IAviatorMetrics
{
    private static readonly Counter MessagesTotal = 
        global::Prometheus.Metrics.CreateCounter("aviator_messages_total", "Total received messages", "type", "channel");

    private static readonly Gauge SignalLevel = global::Prometheus.Metrics.CreateGauge("aviator_signal_level", "Signal Levels", "type", "channel");
    
    private static readonly Gauge NoiseLevel = global::Prometheus.Metrics.CreateGauge("aviator_noise_level", "Noise Levels", "type", "channel");

    private static readonly Gauge ConnectedOutputs =
        global::Prometheus.Metrics.CreateGauge("aviator_connected_outputs", "Output Status", "endpoint");

    public void IncreaseMessagesTotal(BasicAcars basicAcars)
    {
        MessagesTotal
            .WithLabels([basicAcars.Type, basicAcars.Freq])
            .Inc();
    }

    public void AddSignalLevel(BasicAcars basicAcars, double? level)
    {
        if (level is null)
        {
            return;
        }
        SignalLevel
            .WithLabels([basicAcars.Type, basicAcars.Freq])
            .Set((double)level);
    }
    
    public void AddNoiseLevel(BasicAcars basicAcars, double? level)
    {
        if (level is null)
        {
            return;
        }
        NoiseLevel
            .WithLabels([basicAcars.Type, basicAcars.Freq])
            .Set((double)level);
    }

    public void SetOutputStatus(string endpoint, bool connected)
    {
        ConnectedOutputs
            .WithLabels([endpoint])
            .Set(connected ? 1 : 0);
    }
}