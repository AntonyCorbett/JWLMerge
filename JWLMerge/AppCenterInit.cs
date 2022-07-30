using System.Globalization;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace JWLMerge
{
    internal static class AppCenterInit
    {
#pragma warning disable CA1805 // Do not initialize unnecessarily
        private static readonly string? TheToken = null;
#pragma warning restore CA1805 // Do not initialize unnecessarily

#pragma warning disable U2U1112 // Do not call string.IsNullOrEmpty() on a constant string
#pragma warning disable CA1416 // Validate platform compatibility
        public static void Execute()
        {
            if (!string.IsNullOrEmpty(TheToken))
            {
                AppCenter.Start(TheToken, typeof(Analytics), typeof(Crashes));
                AppCenter.SetCountryCode(RegionInfo.CurrentRegion.TwoLetterISORegionName);
            }
        }
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore U2U1112 // Do not call string.IsNullOrEmpty() on a constant string}
    }
}
