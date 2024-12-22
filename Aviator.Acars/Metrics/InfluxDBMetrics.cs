using System.Net;
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

    private bool _disabled;

    public async Task Increase(AcarsType type, BasicAcars acars, CancellationToken cancellationToken = default)
    {
        if (_disabled)
        {
            return;
        }
        
        try
        {
            var point = PointData.Measurement("messages")
                .SetTag("type", type.ToString())
                .SetTag("channel", acars.Channel)
                .SetField("value", 1);

            await _client.WritePointAsync(point, cancellationToken: cancellationToken);
        }
        catch (InfluxDBApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(ex, "Not authorized! We disable the Metrics for now, please adjust the config and restart the service. {Type} {Freq}", type.ToString(), acars.Channel);
            _disabled = true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write metric. {Type} {Freq}", type.ToString(), acars.Channel);
        }
    }
}