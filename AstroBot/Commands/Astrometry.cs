using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    public class Astrometry : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {
        public List<string> Regex => throw new System.NotImplementedException();

        public string Description => throw new System.NotImplementedException();

        public string[] ExampleCalls => throw new System.NotImplementedException();

        public override string Name => throw new System.NotImplementedException();

        public Task<bool> ExecuteRegexCommand(ReceivedMessage receivedMessage, Match regexMatch)
        {
            throw new System.NotImplementedException();
        }
    }
}