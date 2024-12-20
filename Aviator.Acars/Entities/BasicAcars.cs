namespace Aviator.Acars.Entities;

public class BasicAcars
{
    public AcarsType Type { get; set; }
    public required string Address { get; set; }
    public required string Channel { get; set; }
    public DateTime Time { get; set; }
    public string Message { get; set; } = string.Empty;
}