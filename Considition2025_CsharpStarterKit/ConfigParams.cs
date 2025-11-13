using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Considition2025_CsharpStarterKit;

internal static class ConfigParams
{
    public static bool SaveToServer { get; set; } = false;
    public static bool Schedule { get; set; } = false;

    public static string MapName { get; set; } = "";
    public static bool VerboseLog { get; set; } = false;
    public static bool Shortest { get; internal set; } = true;

    internal static void ReadArgs(string[]? args)
    {
        if (args == null || args.Length == 0)
            return;
        MapName = args[0];

        SaveToServer = args.Contains("save");
        Schedule = args.Contains("schedule");
        VerboseLog = args.Contains("verbose");
        Shortest = args.Contains("shortest");
    }

    internal static string ToText()
    {
        return $"Map: {MapName} Save: {SaveToServer} Schedule: {Schedule} Shortest: {Shortest}";
    }
}
