using AwesomeChatBot.Commands.Handlers;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AstroBot.Commands
{
    /// <summary>
    /// A commend for testing
    /// </summary>
    public class TestCommand : AwesomeChatBot.Commands.Command, ISlashCommand
    {
        /// <summary>
        ///  Unique name of the command
        /// </summary>
        public override string Name => "Test";

        public string SlashName => "test";

        public string SlashDescription => "Checks if the bot is working";

        public IReadOnlyList<SlashCommandOption> Options => Array.Empty<SlashCommandOption>();

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
