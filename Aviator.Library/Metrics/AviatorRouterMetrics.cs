using Aviator.Library.Acars;
using Prometheus;

namespace Aviator.Library.Metrics;

public static class AviatorRouterMetrics
{
    private static readonly Counter ReceivedMessagesTotal = Prometheus.Metrics.CreateCounter("received_messages_total", help: "Total received messages", labelNames:
        ["type", "channel"]);

    private static readonly Gauge SigLevel = Prometheus.Metrics.CreateGauge("sig_level", help: "Signal Levels", labelNames: ["type", "channel"]);
    
    private static readonly Gauge NoiseLevel = Prometheus.Metrics.CreateGauge("noise_level", help: "Noise Levels", labelNames: ["type", "channel"]);

    public static void IncReceivedMessagesTotal(BasicAcars basicAcars)
    {
        ReceivedMessagesTotal
            .WithLabels([basicAcars.Type, basicAcars.Freq])
            .Inc();
    }

    public static void AddSigLevel(BasicAcars basicAcars, double level)
    {
        SigLevel
            .WithLabels([basicAcars.Type, basicAcars.Freq])
            .Set(level);
    }
    
    public static void AddNoiseLevel(BasicAcars basicAcars, double level)
    {
        NoiseLevel
            .WithLabels([basicAcars.Type, basicAcars.Freq])
            .Set(level);
    }
}