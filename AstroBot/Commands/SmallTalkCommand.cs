using System;
using System.Collections.Generic;
using System.Linq;
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
            foreach (var smallTalkResponse in Globals.SmallTalkReponsesConfig.SmallTalkResponses.Where(response => response.IsMentioned == receivedMessage.IsBotMentioned))
            {
                  if (smallTalkResponse.MatchRegexList.Any(r => r.IsMatch(receivedMessage.Content)))
                  {
                      var random = new Random();
                      var randomResponse = smallTalkResponse.Responses[random.Next(smallTalkResponse.Responses.Count)];
                      await receivedMessage.Channel.SendMessageAsync(randomResponse);
                      return true;
                  }
            }

            return false;
        }
    }
}
