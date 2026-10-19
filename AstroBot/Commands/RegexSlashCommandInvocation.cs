using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    /// <summary>
    /// Runs a slash command from a regex triggered chat message, so both share the same code.
    /// The regex groups become the slash options and the replies are sent to the channel of the message
    /// </summary>
    public class RegexSlashCommandInvocation : SlashCommandInvocation
    {
        private readonly ReceivedMessage _receivedMessage;

        public override string CommandName { get; }

        public override User Author => _receivedMessage.Author;

        public override Channel Channel => _receivedMessage.Channel;

        public override IReadOnlyDictionary<string, object> Options { get; }

        private RegexSlashCommandInvocation(ReceivedMessage receivedMessage, string commandName, IReadOnlyDictionary<string, object> options)
        {
            _receivedMessage = receivedMessage;
            CommandName = commandName;
            Options = options;
        }

        public override Task RespondAsync(SendMessage message)
        {
            return _receivedMessage.Channel.SendMessageAsync(message);
        }

        /// <summary>
        /// Executes the slash command with the given options, returns true so no other regex command handles the message
        /// </summary>
        public static async Task<bool> ExecuteAsync(ISlashCommand command, ReceivedMessage receivedMessage, Dictionary<string, object>? options = null)
        {
            await command.ExecuteSlashCommandAsync(new RegexSlashCommandInvocation(receivedMessage, command.SlashName, options ?? new Dictionary<string, object>())).ConfigureAwait(false);
            return true;
        }
    }
}
