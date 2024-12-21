using Aviator.Acars.Config;
using Aviator.Acars.Entities;
using InfluxDB3.Client;
using InfluxDB3.Client.Config;
using InfluxDB3.Client.Write;
using Microsoft.Extensions.Logging;

namespace Aviator.Acars.Metrics;

public class InfluxDbMetrics(InfluxDbConfig influxConfig, ILogger<InfluxDbMetrics> logger)
    : IAcarsMetrics
{
    private readonly InfluxDBClient _client = new(new ClientConfig()
    {
        Host = influxConfig.Url,
        Organization = influxConfig.Organization,
        Database = influxConfig.Bucket,
        Token = influxConfig.Token,
        WriteOptions = new WriteOptions()
        {
            Precision = WritePrecision.S,
        },
        Timeout = TimeSpan.FromSeconds(60)
    });

    public async Task Increase(AcarsType type, BasicAcars acars, CancellationToken cancellationToken = default)
    {
        try
        {
            var point = PointData.Measurement("messages")
                .SetTag("type", type.ToString())
                .SetTag("channel", acars.Channel)
                .SetField("value", 1);

            await _client.WritePointAsync(point, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write metric. {Type} {Freq}", type.ToString(), acars.Channel);
        }
    }
}