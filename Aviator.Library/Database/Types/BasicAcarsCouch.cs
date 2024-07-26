using CouchDB.Driver.Types;

namespace Aviator.Library.Database.Types;

public class BasicAcarsCouch : CouchDocument
{
    public string Type { get; set; } = string.Empty;
    public string Station { get; set; } = string.Empty;
    public string Freq { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Reg { get; set; } = string.Empty;
    public string Flight { get; set; } = string.Empty;
    public string Addr { get; set; } = string.Empty;
    public long Timestamp { get; set; }
}