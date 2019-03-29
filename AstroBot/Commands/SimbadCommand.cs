using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AstroBot.Simbad;
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
                        WriteObjectNotFound(receivedMessage, objectName).Wait();
                        return true;
                    }

                }
                return true;
            });
        }

        private Task WriteObjectNotFound(ReceivedMessage receivedMessage, string objectName)
        {
            return receivedMessage.Channel.SendMessageAsync($"No astronomical object found for \"{objectName}\"");
        }
    }
}