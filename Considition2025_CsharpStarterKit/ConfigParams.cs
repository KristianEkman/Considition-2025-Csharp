using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Considition2025_CsharpStarterKit;

internal static class ConfigParams
{
    public static float SkipChargeLimit { get; set; } = 0.95f;

    public static bool SaveToServer { get; set; } = true;

    public static string MapName { get; set; } = "";

    internal static void ReadInput(string[]? args)
    {
        if (args == null || args.Length == 0)
            return;

        SaveToServer = args.Contains("save");

        Console.WriteLine($"Save {SaveToServer}");
        for (int i = 0; i < args.Length; i++)
        {
            switch (i)
            {
                case 0:
                    MapName = args[i];
                    break;
                case 1:
                    var f = 0f;
                    if (float.TryParse(args[i], out f))
                        SkipChargeLimit = f;
                    break;

                default:
                    break;
            }
        }
    }

    internal static string WriteLine()
    {
        return $"Map: {MapName} SkipChargeLimit: {SkipChargeLimit} Save: {SaveToServer}";
    }
}
