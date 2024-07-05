using System.Globalization;
using System.Timers;
using Aviator.Library.Acars;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Aviator.Library.Metrics.InfluxDB;

public class InfluxDbMetrics : IAviatorMetrics
{
    private readonly InfluxDbMetricsConfig _config;
    private readonly ILogger<InfluxDbMetrics> _logger;
    private readonly InfluxDBClient _client;
    private readonly Timer _timer = new(TimeSpan.FromMinutes(1));
    private bool _pingable = true;

    public InfluxDbMetrics(InfluxDbMetricsConfig config, ILogger<InfluxDbMetrics> logger)
    {
        _config = config;
        _logger = logger;
        _client = new InfluxDBClient(config.Url, config.ApiToken);
        CheckConnectionAsync().Wait();
        _timer.Elapsed += async (_, _) => await CheckConnectionAsync().ConfigureAwait(false);
        _timer.Start();
        _client.EnableGzip();
    }

    public void IncreaseMessagesTotal(BasicAcars basicAcars)
    {
        WriteRecord($"received_messages_total,type={basicAcars.Type},channel={basicAcars.Freq} value=1");
    }

    public void AddSignalLevel(BasicAcars basicAcars, double? level)
    {
        if (level is null) return;
        WriteRecord($"sig_level,type={basicAcars.Type},channel={basicAcars.Freq} value={((double)level).ToString(CultureInfo.InvariantCulture)}");
    }

    public void AddNoiseLevel(BasicAcars basicAcars, double? level)
    {
        if (level is null) return;
        WriteRecord($"noise_level,type={basicAcars.Type},channel={basicAcars.Freq} value={((double)level).ToString(CultureInfo.InvariantCulture)}");
    }

    public void SetOutputStatus(string endpoint, bool connected)
    {
        WriteRecord($"connected_outputs,endpoint={endpoint} value={(connected ? "1" : "0")}");
    }

    private void WriteRecord(string record)
    {
        if (!_pingable) return;
        using var api = _client.GetWriteApi();
        api.WriteRecord(record, WritePrecision.S, _config.Bucket, _config.Org);
    }

    private async Task CheckConnectionAsync()
    {
        var before = _pingable;
        var pingable = await _client.PingAsync();
        _pingable = pingable;
        
        if (!before && pingable)
        {
            _logger.LogInformation("{Url} Can be pinged again!", _config.Url);
        }
        else if (!pingable)
        {
            _logger.LogWarning("{Url} Cannot be pinged! While this situation is not resolved, we will not send metrics!", _config.Url);
        }
    }
}