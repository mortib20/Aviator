using Aviator.Acars.Config;
using InfluxDB3.Client;
using InfluxDB3.Client.Write;

namespace Aviator.Acars;

public class AcarsMetrics(InfluxDbConfig influxConfig)
{
    private InfluxDBClient _client = new(host: influxConfig.Url, token: influxConfig.Token,
        organization: influxConfig.Organization, influxConfig.Bucket);

    public async Task Increase(AcarsType type, CancellationToken cancellationToken = default)
    {
        var point = PointData.Measurement("messages")
            .SetTag("type", type.ToString())
            .SetField("value", 1);

        await _client.WritePointAsync(point, cancellationToken: cancellationToken);
    }
}