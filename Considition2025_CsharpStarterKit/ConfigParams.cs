using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Considition2025_CsharpStarterKit;

internal static class ConfigParams
{
    public static bool SaveToServer { get; set; } = true;
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
        if (args.Contains("schedule")) Schedule = true;
        if (args.Contains("verbose")) VerboseLog = true;
        if (args.Contains("shortest")) Shortest = true;
    }

    internal static string ToText()
    {
        return $"Map: {MapName} Save: {SaveToServer} Schedule: {Schedule} Shortest: {Shortest}";
    }
}
