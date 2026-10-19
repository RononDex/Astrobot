using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AstroBot.LaunchLibrary
{
    public static class LaunchLibraryClient
    {
        public const string ApiUrl = "https://ll.thespacedevs.com/2.0.0/";

        private static async Task<string> GetAsync(string requestUrl)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Accept.ParseAdd("application/json");
            request.Headers.Add("X-Requested-With", "AstroBot");
            request.Headers.UserAgent.ParseAdd("AstroBot");

            using var response = await Globals.HttpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public static async Task<IReadOnlyList<Launch>> GetUpcomingLaunchesAsync(int limit)
        {
            var logger = Globals.LoggerFactory.CreateLogger(nameof(LaunchLibraryClient));

            try
            {
                logger.Log(LogLevel.Information, $"Requesting the next {limit} upcoming launches from LaunchLibrary");
                var requestUrl = $"{ApiUrl}launch/upcoming/?mode=detailed&format=json&limit={limit}";
                var text = await GetAsync(requestUrl).ConfigureAwait(false);

                var result = JsonConvert.DeserializeObject<RootLaunches>(text) ?? throw new Exception("LaunchLibrary returned an empty response");
                logger.Log(LogLevel.Information, $"Found {result.Results.Count} upcoming launches");
                return result.Results.OrderBy(x => x.Net).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error while trying to get the next {limit} launches: {ex}", ex);
                return new List<Launch>();
            }
        }

        public static async Task<IReadOnlyList<Event>> GetUpcomingEventsAsync(int limit)
        {
            var logger = Globals.LoggerFactory.CreateLogger(nameof(LaunchLibraryClient));

            try
            {
                logger.Log(LogLevel.Information, $"Requesting the next {limit} upcoming events from LaunchLibrary");
                var requestUrl = $"{ApiUrl}event/upcoming/?mode=detailed&format=json&limit={limit}";
                var text = await GetAsync(requestUrl).ConfigureAwait(false);

                var result = JsonConvert.DeserializeObject<RootEvents>(text) ?? throw new Exception("LaunchLibrary returned an empty response");
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
