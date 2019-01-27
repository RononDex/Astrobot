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

        public Task<bool> ExecuteRegexCommand(ReceivedMessage recievedMessage, Match regexMatch)
        {
            return Task<bool>.Factory.StartNew(() => {
                if (regexMatch.Groups["SearchLocationCL"].Success){

                    var location    = regexMatch.Groups["SearchLocationCL"].Value;
                    var geoLocation = GeoLocation.GeoLocation.FindLocation(location);

                    // If no place with that name can be found
                    if (geoLocation == null)
                    {
                        recievedMessage.Channel.SendMessageAsync(new SendMessage($"I could not find any place on earth with the name \"{location}\"")).Wait();
                        return true;
                    }

                    // Get the weather forecast
                    recievedMessage.Channel.SendMessageAsync($"Searching weather forecast for:    Name: {geoLocation.Name}, Lat: {geoLocation.Lat}, Lng: {geoLocation.Lng}").Wait();
                    var forecast = Weather.Clearoutside.GetWeatherForecast(geoLocation);
                    recievedMessage.Channel.SendMessageAsync(new SendMessage("I found the following weather forecast:", new List<Attachment>() { forecast })).Wait();
                    
                    return true;
                }

                return false;
            });
        }
    }
}