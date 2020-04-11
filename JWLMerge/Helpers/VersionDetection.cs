namespace JWLMerge.Helpers
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using Serilog;

    internal static class VersionDetection
    {
        public static Version GetLatestReleaseVersion(string latestReleaseUrl)
        {
            string version = null;

            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(new Uri(latestReleaseUrl)).Result;
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

            return !string.IsNullOrWhiteSpace(version) ? new Version(version) : null;
        }

        public static Version GetCurrentVersion()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            return new Version($"{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}");
        }
    }
}
