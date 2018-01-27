namespace JWLMerge.Helpers
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using Serilog;

    internal static class VersionDetection
    {
        public static string GetLatestReleaseVersion(string latestReleaseUrl)
        {
            string version = null;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync(latestReleaseUrl).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var latestVersionUri = response.RequestMessage.RequestUri;
                        if (latestVersionUri != null)
                        {
                            var segments = latestVersionUri.Segments;
                            if (segments.Any())
                            {
                                version = segments[segments.Length - 1];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Getting latest release version");
            }

            return version;
        }

        public static string GetCurrentVersion()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";
        }
    }
}
