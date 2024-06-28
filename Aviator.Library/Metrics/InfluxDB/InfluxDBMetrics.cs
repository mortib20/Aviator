using System.Globalization;
using Aviator.Library.Acars;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;

namespace Aviator.Library.Metrics.InfluxDB;

public class InfluxDbMetrics : IAviatorMetrics
{

    public InfluxDbMetrics(InfluxDbMetricsConfig config)
    {
        _org = config.Org;
        _bucket = config.Bucket;
        _client = new InfluxDBClient(config.Url, config.ApiToken);
    }

    private readonly string _org;
    private readonly string _bucket;
    private readonly InfluxDBClient _client;
    
    public void IncreaseMessagesTotal(BasicAcars basicAcars)
    {
        using var api = _client.GetWriteApi();
        api.WriteRecord($"received_messages_total,type={basicAcars.Type},channel={basicAcars.Freq} value=1", WritePrecision.S, _bucket, _org);
    }

    public void AddSignalLevel(BasicAcars basicAcars, double? level)
    {
        if (level is null) return;
        using var api = _client.GetWriteApi();
        api.WriteRecord($"sig_level,type={basicAcars.Type},channel={basicAcars.Freq} value={((double)level).ToString(CultureInfo.InvariantCulture)}", WritePrecision.S, _bucket, _org);
    }

    public void AddNoiseLevel(BasicAcars basicAcars, double? level)
    {
        if (level is null) return;
        using var api = _client.GetWriteApi();
        api.WriteRecord($"noise_level,type={basicAcars.Type},channel={basicAcars.Freq} value={((double)level).ToString(CultureInfo.InvariantCulture)}", WritePrecision.S, _bucket, _org);
    }

    public void SetOutputStatus(string endpoint, bool connected)
    {
        using var api = _client.GetWriteApi();
        api.WriteRecord($"connected_outputs,endpoint={endpoint} value={(connected ? "1" : "0")}", WritePrecision.S, _bucket, _org);
    }
}