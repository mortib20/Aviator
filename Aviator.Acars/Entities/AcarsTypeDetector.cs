using System.Text.Json.Nodes;

namespace Aviator.Acars.Entities;

public abstract class AcarsTypeFinder
{
    public static AcarsType? Detect(JsonNode json)
    {
        try
        {
            if (IsAero(json)) return AcarsType.Aero;

            if (IsAcars(json)) return AcarsType.Acars;

            if (IsHfdl(json)) return AcarsType.Hfdl;

            if (IsVdl2(json)) return AcarsType.Vdl2;

            if (IsIridium(json)) return AcarsType.Iridium;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return null;
    }

    private static bool IsAero(JsonNode json)
    {
        return json?["app"]?["name"]?.ToString() == "JAERO";
    }

    private static bool IsAcars(JsonNode json)
    {
        return json?["app"]?["name"]?.ToString() == "acarsdec";
    }

    private static bool IsHfdl(JsonNode json)
    {
        return json?["hfdl"]?["app"]?["name"]?.ToString() == "dumphfdl";
    }

    private static bool IsVdl2(JsonNode json)
    {
        return json?["vdl2"]?["app"]?["name"]?.ToString() == "dumpvdl2";
    }
    
    private static bool IsIridium(JsonNode json)
    {
        return json?["app"]?["name"]?.ToString() == "iridium-toolkit";
    }

    public static bool HasAcars(JsonNode json)
    {
        var vdl2 = json["vdl2"]?["avlc"]?["acars"] is not null;
        var hfdl = json["hfdl"]?["lpdu"]?["hfnpdu"]?["acars"] is not null;
        var jaero = json["isu"]?["acars"] is not null;
        var acars = json["text"] is not null;
        var iridium = json["acars"]?["text"] is not null;
        return vdl2 || hfdl || jaero || acars || iridium;
    }
}