using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AstroBot.LaunchLibrary
{
    public static class LaunchLibraryClient
    {
        public const string ApiUrl = "https://ll.thespacedevs.com/2.0.0/";

        public static IReadOnlyList<Launch> GetUpcomingLaunches(int limit)
        {
            var logger = Globals.LoggerFactory.CreateLogger(nameof(LaunchLibraryClient));
            logger.Log(LogLevel.Information, $"Requesting the next {limit} upcoming launches from LaunchLibrary");
            var requestUrl = $"{ApiUrl}launch/upcoming/?mode=detailed&format=json&limit={limit}";
            var webRequest = WebRequest.CreateHttp(requestUrl);
            webRequest.Accept = "application/json";
            webRequest.Headers["X-Requested-With"] = "AstroBot";
            webRequest.UserAgent = "AstroBot";

            var response = (HttpWebResponse)webRequest.GetResponse();
            string text;
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();
            }

            var result = JsonConvert.DeserializeObject<Root>(text);
            logger.Log(LogLevel.Information, $"Found {result.Results.Count} upcoming launches");
            return result.Results.OrderByDescending(x => x.Net).ToList();
        }
    }
}
