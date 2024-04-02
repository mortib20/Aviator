namespace Aviator.Library.Metrics.InfluxDB;

public class InfluxDbMetricsConfig
{
    public const string SectionName = "InfluxDB";
    public bool Enabled { get; set; } = false;
    public string Url { get; set; }
    public string Org { get; set; }
    public string Bucket { get; set; }
    public string ApiToken { get; set; }
}