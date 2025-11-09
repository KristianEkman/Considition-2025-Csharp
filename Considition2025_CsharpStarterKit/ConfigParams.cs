using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Considition2025_CsharpStarterKit;

internal static class ConfigParams
{
    public static float SkipChargeLimit { get; set; } = 0.43f;

    static void WriteLine()
    {
        Console.WriteLine($"SkipChargeLimit: {SkipChargeLimit}");
    }
}
