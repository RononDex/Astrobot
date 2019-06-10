using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AstroBot.Simbad;
using AstroBot.Objects.AstronomicalObjects;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    public class SimbadCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {
        public string Description => "Access data from the SIMBAD database";

        public string[] ExampleCalls => new string[]
        {
            "what is M31?",
            "what do you know about M31?",
            // "how far away is M13?",
            // "fluxes of NGC6888?",
            // "how bright is M78?",
            // "how big is M31?"
        };

        public List<string> Regex => new List<string>()
        {
            @"what do you know about (?'AstroObject'.*\w)(\?)?",
            @"what is (?'AstroObject'.*\w)(\?)?"
        };

        public override string Name => "Simbad";

        public Task<bool> ExecuteRegexCommand(ReceivedMessage receivedMessage, Match regexMatch)
        {
            return Task.Factory.StartNew(() =>
            {
                var simbadClient = new SimbadClient();

                if (regexMatch.Groups["AstroObject"].Success)
                {
                    string objectName = regexMatch.Groups["AstroObject"].Value;
                    var foundObject = simbadClient.FindObjectByName(objectName);
                    if (foundObject == null)
                    {
                        WriteObjectNotFoundAsync(receivedMessage, objectName).Wait();
                        return true;
                    }

                    WriteObjectDetailsAsync(receivedMessage, foundObject);
                }
                return true;
            });
        }

        private static Task WriteObjectNotFoundAsync(ReceivedMessage receivedMessage, string objectName)
        {
            return receivedMessage.Channel.SendMessageAsync($"No astronomical object found for \"{objectName}\"");
        }

        private static Task WriteObjectDetailsAsync(ReceivedMessage receivedMessage, AstronomicalObject astronomicalObject)
        {
            var columnSize = 24;

            return receivedMessage.Channel.SendMessageAsync(
                receivedMessage.ApiWrapper.MessageFormatter.CodeBlock(
                    $"{"Name:".PadRight(columnSize)} {astronomicalObject.Name}\r\n" +
                    $"{"Type:".PadRight(columnSize)} {astronomicalObject.Type}\r\n" +
                    $"{"Relative velocity:".PadRight(columnSize)} {astronomicalObject.RelativeVelocity}\r\n" +
                    $"\r\n" +
                    $"{"Coordinates:".PadRight(columnSize)} RA: {astronomicalObject.RaDecCoordinate.RightAscension}\r\n" +
                    $"{"            ".PadRight(columnSize)} DEC: {astronomicalObject.RaDecCoordinate.Declination}\r\n" +
                    $"\r\n" +
                    $"Secondary types:\r\n{string.Join(',', astronomicalObject.OtherTypes)}\r\n" +
                    $"\r\n" +
                    $"OtherNames:\r\n{string.Join(',', astronomicalObject.OtherNames)}\r\n"
                ));
        }
    }
}