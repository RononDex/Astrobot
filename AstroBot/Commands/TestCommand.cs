using AwesomeChatBot.Commands.Handlers;
using AwesomeChatBot.ApiWrapper;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AstroBot.Commands
{
    /// <summary>
    /// A commend for testing
    /// </summary>
    public class TestCommand : AwesomeChatBot.Commands.Command, IRegexCommand
    {
        /// <summary>
        /// A list of regex patterns that trigger the command
        /// </summary>
        public List<string> Regex => new List<string>() { "test (?'TestParam'.*\\w)" };

        /// <summary>
        ///  Unique name of the command
        /// </summary>
        public override string Name => "Test";

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="receivedMessage"></param>
        /// <param name="regexMatch"></param>
        /// <returns></returns>
        public Task<bool> ExecuteRegexCommand(ReceivedMessage receivedMessage, Match regexMatch)
        {
            return Task<bool>.Factory.StartNew(() =>
            {

                var testParam = regexMatch.Groups["TestParam"].Value;

                receivedMessage.Channel.SendMessageAsync(new SendMessage($"IT'S WORKING!!! You entered {testParam}")).Wait();

                return true;
            });

        }
    }
}