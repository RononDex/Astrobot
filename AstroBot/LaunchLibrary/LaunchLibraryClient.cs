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

            try
            {
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

                var result = JsonConvert.DeserializeObject<RootLaunches>(text);
                logger.Log(LogLevel.Information, $"Found {result.Results.Count} upcoming launches");
                return result.Results.OrderBy(x => x.Net).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while trying to get the next {limit} launches: {ex}", ex);
                return new List<Launch>();
            }
        }

        public static IReadOnlyList<Event> GetUpcomingEvents(int limit)
        {
            var logger = Globals.LoggerFactory.CreateLogger(nameof(LaunchLibraryClient));

            try
            {
                logger.Log(LogLevel.Information, $"Requesting the next {limit} upcoming events from LaunchLibrary");
                var requestUrl = $"{ApiUrl}event/upcoming/?mode=detailed&format=json&limit={limit}";
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

                var result = JsonConvert.DeserializeObject<RootEvents>(text);
                logger.Log(LogLevel.Information, $"Found {result.Results.Count} upcoming events");
                return result.Results.OrderBy(x => x.EventTime).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while trying to get the next {limit} events: {ex}", ex);
                return new List<Event>();
            }
        }
    }
}
