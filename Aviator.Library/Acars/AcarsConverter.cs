using System.Globalization;
using Aviator.Library.Acars.Types.Acars;
using Aviator.Library.Acars.Types.Hfdl;
using Aviator.Library.Acars.Types.Jaero;
using Aviator.Library.Acars.Types.VDL2;

namespace Aviator.Library.Acars;

public abstract class AcarsConverter
{
    public static BasicAcars ConvertAero(Jaero aero)
    {
        return new BasicAcars
        {
            Type = AcarsType.Aero.ToString(),
            Freq = aero.app.ver,
            Station = aero.station, 
            Timestamp = aero.t.sec,
            Addr = aero.isu.dst.addr,
            Flight = string.Empty,
            Label = aero.isu.acars.label,
            Reg = aero.isu.acars.reg,
            Text = aero.isu.acars.msg_text
        };
    }

    public static BasicAcars ConvertDumpVdl2(DumpVdl2 vdl2)
    {
        return new BasicAcars
        {
            Type = AcarsType.Vdl2.ToString(),
            Freq = vdl2.vdl2.freq.ToString(),
            Addr = vdl2.vdl2.avlc.src.addr,
            Flight = vdl2.vdl2.avlc.acars.flight,
            Label = vdl2.vdl2.avlc.acars.label,
            Reg = vdl2.vdl2.avlc.acars.reg,
            Station = vdl2.vdl2.station,
            Timestamp = vdl2.vdl2.t.sec,
            Text = vdl2.vdl2.avlc.acars.msg_text 
        };
    }

    public static BasicAcars ConvertDumpHfdl(DumpHfdl hfdl)
    {
        return new BasicAcars
        {
            Type = AcarsType.Hfdl.ToString(),
            Station = hfdl.hfdl.station,
            Freq = hfdl.hfdl.freq.ToString(),
            Label = hfdl.hfdl.lpdu.hfnpdu.acars.label,
            Text = hfdl.hfdl.lpdu.hfnpdu.acars.msg_text,
            Reg = hfdl.hfdl.lpdu.hfnpdu.acars.reg,
            Flight = hfdl.hfdl.lpdu.hfnpdu.acars.flight,
            Addr = hfdl.hfdl.lpdu.dst.ac_info?.icao ?? string.Empty,
            Timestamp = hfdl.hfdl.t.sec
        };
    }

    public static BasicAcars ConvertAcarsdec(Acarsdec acars)
    {
        return new BasicAcars
        {
            Type = AcarsType.Acars.ToString(),
            Station = acars.station_id,
            Freq = $"{acars.freq.ToString(CultureInfo.InvariantCulture).Replace(".", "")}000",
            Label = acars.label,
            Text = acars.text,
            Reg = acars.tail,
            Flight = acars.flight,
            Addr = string.Empty,
            Timestamp = (int)acars.timestamp
        };
    }
}