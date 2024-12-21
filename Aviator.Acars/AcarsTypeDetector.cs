using System.Text.Json.Nodes;

namespace Aviator.Acars;

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

            // TODO add Iridium
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

    public static bool HasAcars(JsonNode json)
    {
        return json["vdl2"]?["avlc"]?["acars"] is not null || json["hfdl"]?["lpdu"]?["hfnpdu"]?["acars"] is not null ||
               json["isu"]?["acars"] is not null || json["text"] is not null;
    }
}