using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;

namespace JWLMerge.EventTracking
{
#pragma warning disable CA1416 // Validate platform compatibility
    
    internal static class EventTracker
    {
        public static void Track(EventName eventName, Dictionary<string, string>? properties = null)
        {
            Analytics.TrackEvent(eventName.ToString(), properties);
        }

        public static void TrackWrongVer(int verFound, int verExpected)
        {
            var properties = new Dictionary<string, string> 
            { 
                { "found", verFound.ToString() },
                { "expected", verExpected.ToString() },
            };

            Track(EventName.WrongVer, properties);
        }

        public static void TrackWrongManifestVer(int verFound, int verExpected)
        {
            var properties = new Dictionary<string, string>
            {
                { "found", verFound.ToString() },
                { "expected", verExpected.ToString() },
            };

            Track(EventName.WrongManifestVer, properties);
        }

        public static void Error(Exception ex, string? context = null)
        {            
            if (string.IsNullOrEmpty(context))
            {
                Crashes.TrackError(ex);
            }
            else
            {
                var properties = new Dictionary<string, string> { { "context", context } };
                Crashes.TrackError(ex, properties);
            }            
        }

        public static void TrackMerge(int numSourceFiles)
        {
            Track(EventName.Merge, new Dictionary<string, string>
            {
                { "count", numSourceFiles.ToString() },
            });
        }
    }

#pragma warning restore CA1416 // Validate platform compatibility
}
