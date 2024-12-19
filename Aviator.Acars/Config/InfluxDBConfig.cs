namespace Aviator.Acars.Config;

public class InfluxDbConfig
{
    public required string Url { get; set; }
    public required string Bucket { get; set; }
    public required string Organization { get; set; }
    public required string Token { get; set; }
}