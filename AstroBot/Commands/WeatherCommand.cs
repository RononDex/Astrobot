using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    /// <summary>
    /// The weather commands used to get weather forecasts
    /// </summary>
    public class WeatherCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {
        public string Description => "Gives weather forcasts for specific locations";

        public string[] ExampleCalls => new string[] { "Weather in Zurich" };

        public List<string> Regex => new List<string>() {
            @"(whats|what's|show\sme|how is|hows|how's|what is) the (weather|forecast) (like )?(in|for) (?'SearchLocationCL'.*\w)(\?)?",
            @"(weather|forecast) (in|for) (?'SearchLocationCL'.*\w)(\?)?",
            @"(weather|forecast) (?'SearchLocationCL'.*\w)(\?)?"
        };

        public override string Name => "Weather";

        public async Task<bool> ExecuteRegexCommandAsync(ReceivedMessage receivedMessage, Match regexMatch)
        {
            if (regexMatch.Groups["SearchLocationCL"].Success)
            {

                var location = regexMatch.Groups["SearchLocationCL"].Value;
                var geoLocation = GeoLocation.GeoLocation.FindLocation(location);

                // If no place with that name can be found
                if (geoLocation == null)
                {
                    await receivedMessage.Channel.SendMessageAsync(new SendMessage($"I could not find any place on earth with the name \"{location}\""))
                        .ConfigureAwait(false);
                    return true;
                }

                // Get the weather forecast
                await receivedMessage.Channel.SendMessageAsync($"Searching weather forecast for:    Name: {geoLocation.Name}, Lat: {geoLocation.Lat}, Lng: {geoLocation.Lng}")
                    .ConfigureAwait(false);
                var forecast = Weather.Clearoutside.GetWeatherForecast(geoLocation);
                await receivedMessage.Channel.SendMessageAsync(new SendMessage("I found the following weather forecast:", new List<Attachment> { forecast }))
                    .ConfigureAwait(false);

                return true;
            }

            return false;
        }
    }
}