using Aviator.Acars.Config;
using Aviator.Acars.Entities;
using InfluxDB3.Client;
using InfluxDB3.Client.Write;

namespace Aviator.Acars.Metrics;

public class InfluxDbMetrics(InfluxDbConfig influxConfig) : IAcarsMetrics
{
    private readonly InfluxDBClient _client = new(influxConfig.Url, influxConfig.Token,
        influxConfig.Organization, influxConfig.Bucket);

    public async Task Increase(AcarsType type, BasicAcars acars, CancellationToken cancellationToken = default)
    {
        var point = PointData.Measurement("messages")
            .SetTag("type", type.ToString())
            .SetTag("channel", acars.Freq)
            .SetField("value", 1);

        await _client.WritePointAsync(point, cancellationToken: cancellationToken);
    }
}