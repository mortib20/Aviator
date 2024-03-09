using Aviator.Library.IO.Input;
using Aviator.Library.IO.Output;

namespace Aviator.Library.Acars.Settings;

public class AcarsRouterSettings
{
    public static readonly string SectionName = "AcarsRouter";
    public int InputPort { get; set; }
    public InputProtocol InputProtocol { get; set; }
    
    public Outputs Outputs { get; set; }
}

public class Outputs
{
    public List<Endpoint> Aero { get; set; } = [];
    public List<Endpoint> Vdl2 { get; set; } = [];
    public List<Endpoint> Hfdl { get; set; } = [];
    public List<Endpoint> Acars { get; set; } = [];
    public List<Endpoint> Iridium { get; set; } = [];
}

public class Endpoint
{
    public OutputProtocol Protocol { get; set; }
    public string? Address { get; set; }
    public int Port { get; set; }
}