using AwesomeChatBot.ApiWrapper;
using System.Text.RegularExpressions;
using AwesomeChatBot.Commands.Handlers;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AstroBot.Commands
{
    /// <summary>
    /// A commend for testing
    /// </summary>
    public class TestCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ISlashCommand
    {
        /// <summary>
        ///  Unique name of the command
        /// </summary>
        public override string Name => "Test";

        /// <summary>
        /// A list of regex patterns that trigger the command
        /// </summary>
        public List<string> Regex => new List<string> { "test (?'TestParam'.*\\w)" };

        public string SlashName => "test";

        public string SlashDescription => "Checks if the bot is working";

        public IReadOnlyList<SlashCommandOption> Options => Array.Empty<SlashCommandOption>();

        public Task<bool> ExecuteRegexCommandAsync(ReceivedMessage receivedMessage, Match regexMatch)
        {
            return RegexSlashCommandInvocation.ExecuteAsync(this, receivedMessage);
        }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="invocation"></param>
        public Task ExecuteSlashCommandAsync(SlashCommandInvocation invocation)
        {
            return invocation.RespondAsync("IT'S WORKING!!!");
        }
    }
}
