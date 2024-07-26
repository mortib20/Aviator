namespace Aviator.Library.Database.Config;

public class CouchDbConfig
{
    public const string SectionName = "CouchDB";
    public string? Endpoint { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}