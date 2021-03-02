using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    /// <summary>
    /// Command that can get the lat / lng coordinates for any place on earth
    /// </summary>
    public class LocationCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {
        /// <summary>
        /// The name of the command
        /// </summary>
        public override string Name => "GeoLocation";

        /// <summary>
        /// The regex, when matched executes the command
        /// </summary>
        public List<string> Regex => new List<string> { @"Where is (?'SearchLocation'.*\w)(\?)?" };

        public string Description => "Gets the Lat / Lng coordinates of a certain location";
        public string[] ExampleCalls => new[] { "Where is Zurich" };

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="receivedMessage"></param>
        /// <param name="regexMatch"></param>
        public async Task<bool> ExecuteRegexCommandAsync(ReceivedMessage receivedMessage, Match regexMatch)
        {
            var location = regexMatch.Groups["SearchLocation"].Value;
            var geoLocation = GeoLocation.GeoLocation.FindLocation(location);

            if (geoLocation == null)
            {
                await receivedMessage.Channel.SendMessageAsync(new SendMessage($"I don't know any place on earth with the name {location}")).ConfigureAwait(false);
                return true;
            }

            await receivedMessage.Channel.SendMessageAsync(new SendMessage($"I found the following location for \"{location}\":\r\n" +
                                                                        receivedMessage.ApiWrapper.MessageFormatter.Quote($"Name:   {geoLocation.Name}\r\n" +
                                                                        $"Lat:    {geoLocation.Lat}\r\n" +
                                                                        $"Lng:    {geoLocation.Lng}"))).ConfigureAwait(false);

            return true;
        }
    }
}