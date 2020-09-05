using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;

namespace AstroBot.LaunchLibrary
{
    public static class LaunchLibraryClient
    {
        public const string ApiUrl = "https://launchlibrary.net/1.4/";

        public static IReadOnlyList<Launch> GetUpcomingLaunches(int days)
        {
            var logger = Globals.LoggerFactory.CreateLogger(nameof(LaunchLibraryClient));
            logger.Log(LogLevel.Information, $"Requesting upcoming launches from LaunchLibrary for the next {days} days");
            var requestUrl = $"{ApiUrl}launch?mode=verbose&startdate={GetDateString(DateTime.UtcNow.Date)}&enddate={GetDateString(DateTime.UtcNow.Date.AddDays(days))}";
            var webRequest = WebRequest.CreateHttp(requestUrl);
            webRequest.Accept = "application/json";
            webRequest.Headers["X-Requested-With"] = nameof(AstroBotController);
            webRequest.UserAgent = nameof(AstroBotController);

            var response = (HttpWebResponse)webRequest.GetResponse();
            string text;
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();
            }

            var result = Result.FromJson(text);
            logger.Log(LogLevel.Information, $"Found {result.Launches.Length} upcoming launches");
            return result.Launches;
        }

        private static string GetDateString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }
    }
}
