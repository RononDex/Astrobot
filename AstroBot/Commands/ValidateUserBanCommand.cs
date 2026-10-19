
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    /// <summary>
    /// The weather commands used to get weather forecasts
    /// </summary>
    public class ValidateUserBanCommand : AwesomeChatBot.Commands.Command, IRegexCommand
    {
        public List<string> Regex => new List<string>() {
            @".*",
        };

        public override string Name => "Weather";

        public async Task<bool> ExecuteRegexCommandAsync(ReceivedMessage receivedMessage, Match regexMatch)
        {
            if (receivedMessage.IsBotMentioned && Globals.GloballyBannedUsers.Any(x => x == receivedMessage.Author.UserID))
            {
                await receivedMessage.Channel.SendMessageAsync("You have been banned from using this bot");
                return true;
            }

            return false;
        }
    }
}
