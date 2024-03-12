using Aviator.Library.Acars;
using Prometheus;

namespace Aviator.Library.Metrics;

public static class AviatorRouterMetrics
{
    private static Counter ReceivedMessagesTotal = Prometheus.Metrics.CreateCounter("received_messages_total", help: "Total received messages", labelNames:
        ["type", "channel"]);

    public static void IncReceivedMessagesTotal(BasicAcars basicAcars)
    {
        ReceivedMessagesTotal
            .WithLabels([basicAcars.Type, basicAcars.Freq])
            .Inc();
    }
}