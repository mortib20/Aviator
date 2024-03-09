using Prometheus;

namespace Aviator.Library.Metrics;

public class AviatorRouterMetrics
{
    public Counter ReceivedMessagesTotal = Prometheus.Metrics.CreateCounter("received_messages_total", help: "Total received messages", labelNames:
        ["label", "flight_number", "flight", "icao", "type"]);
}