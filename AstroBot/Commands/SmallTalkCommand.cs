using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    public class SmallTalkCommand : AwesomeChatBot.Commands.Command, IRegexCommand
    {
        public List<string> Regex => new List<string> { ".*" };

        public override string Name => "SmallTalk";

        public async Task<bool> ExecuteRegexCommandAsync(ReceivedMessage receivedMessage, Match regexMatch)
        {
            if (receivedMessage.IsBotMentioned)
            {
                await receivedMessage.Channel.SendMessageAsync("I am not sure how to respond to that");
                return true;
            }

            return false;
        }
    }
}
