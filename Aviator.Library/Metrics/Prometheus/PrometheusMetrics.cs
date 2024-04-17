using Aviator.Library.Acars;
using Prometheus;

namespace Aviator.Library.Metrics.Prometheus;

public class PrometheusMetrics : IAviatorMetrics
{
    private static readonly Counter ReceivedMessagesTotal = global::Prometheus.Metrics.CreateCounter("received_messages_total", help: "Total received messages", labelNames:
        ["type", "channel"]);

    private static readonly Gauge SigLevel = global::Prometheus.Metrics.CreateGauge("sig_level", help: "Signal Levels", labelNames: ["type", "channel"]);
    
    private static readonly Gauge NoiseLevel = global::Prometheus.Metrics.CreateGauge("noise_level", help: "Noise Levels", labelNames: ["type", "channel"]);

    private static readonly Gauge ConnectedOutputs =
        global::Prometheus.Metrics.CreateGauge("connected_outputs", help: "Status of the outputs", labelNames: ["type", "endpoint"]);

    public void IncReceivedMessagesTotal(BasicAcars basicAcars)
    {
        ReceivedMessagesTotal
            .WithLabels([basicAcars.Type, basicAcars.Freq])
            .Inc();
    }

    public void AddSigLevel(BasicAcars basicAcars, double? level)
    {
        if (level is null)
        {
            return;
        }
        SigLevel
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

    public void SetOutputStatus(string type, string endpoint, bool connected)
    {
        ConnectedOutputs
            .WithLabels([type, endpoint])
            .Set(connected ? 1 : 0);
    }
}