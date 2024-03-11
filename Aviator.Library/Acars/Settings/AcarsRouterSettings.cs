using Aviator.Library.IO;
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
    public List<EndPoint> Aero { get; set; } = [];
    public List<EndPoint> Vdl2 { get; set; } = [];
    public List<EndPoint> Hfdl { get; set; } = [];
    public List<EndPoint> Acars { get; set; } = [];
    public List<EndPoint> Iridium { get; set; } = [];
}