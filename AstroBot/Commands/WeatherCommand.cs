using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    /// <summary>
    /// The weather commands used to get weather forecasts
    /// </summary>
    public class WeatherCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ISlashCommand, ICommandDescription
    {
        public string Description => "Gives weather forcasts for specific locations";

        public override string Name => "Weather";

        public List<string> Regex => new List<string>() {
            @"(whats|what's|show\sme|how is|hows|how's|what is) the (weather|forecast) (like )?(in|for) (?'SearchLocationCL'.*\w)(\?)?",
            @"(weather|forecast) (in|for) (?'SearchLocationCL'.*\w)(\?)?",
            @"(weather|forecast) (?'SearchLocationCL'.*\w)(\?)?"
        };

        public string[] ExampleCalls => new string[] { "Weather in Zurich" };

        public string SlashName => "weather";

        public string SlashDescription => Description;

        public IReadOnlyList<SlashCommandOption> Options => new[]
        {
            new SlashCommandOption { Name = "location", Description = "Name of the place, e.g. Zurich", Type = SlashOptionType.String, Required = true }
        };

        public Task<bool> ExecuteRegexCommandAsync(ReceivedMessage receivedMessage, Match regexMatch)
        {
            return RegexSlashCommandInvocation.ExecuteAsync(this, receivedMessage, new Dictionary<string, object>
            {
                ["location"] = regexMatch.Groups["SearchLocationCL"].Value
            });
        }

        public async Task ExecuteSlashCommandAsync(SlashCommandInvocation invocation)
        {
            var geoLocation = await GeoLocation.GeoLocation.FindLocationAsync(invocation.GetOption<string>("location")).ConfigureAwait(false);

            // If no place with that name can be found
            if (geoLocation == null)
            {
                await invocation.RespondAsync("I could not find any place on earth with that name").ConfigureAwait(false);
                return;
            }

            // Get the weather forecast
            await invocation.RespondAsync($"Searching weather forecast for:    Name: {geoLocation.Name}, Lat: {geoLocation.Lat}, Lng: {geoLocation.Lng}")
                .ConfigureAwait(false);
            var forecast = await Weather.Clearoutside.GetWeatherForecastAsync(geoLocation).ConfigureAwait(false);
            await invocation.RespondAsync(new SendMessage("I found the following weather forecast:", new List<Attachment> { forecast }))
                .ConfigureAwait(false);
        }
    }
}
