using Prometheus;

namespace Aviator.Library.Metrics;

public static class AviatorRouterMetrics
{
    public static Counter ReceivedMessagesTotal = Prometheus.Metrics.CreateCounter("received_messages_total", help: "Total received messages", labelNames:
        ["type", "channel"]);
}