using System;
using System.Globalization;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace JWLMerge;

internal static class AppCenterInit
{
    // Please omit this token (or use your own) if you are building a fork
    private static readonly string? TheToken = "2a948a7a-4933-4e7d-9ba0-90e071b71e39";

    public static void Execute()
    {
        if (OperatingSystem.IsWindows())
        {
#pragma warning disable CA1416
            AppCenter.Start(TheToken, typeof(Analytics), typeof(Crashes));
            AppCenter.SetCountryCode(RegionInfo.CurrentRegion.TwoLetterISORegionName);
#pragma warning restore CA1416
        }
    }
}