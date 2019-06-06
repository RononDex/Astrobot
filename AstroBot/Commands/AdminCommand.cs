using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    public class AdminCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {
        public override string Name => "Admin";

        public List<string> Regex => new List<string>
        {
            @"(?'ListServerSettings'show server config)",
        };

        public string Description => "Administrative commands";

        public string[] ExampleCalls => new[] { "show server config" };

        public Task<bool> ExecuteRegexCommand(ReceivedMessage receivedMessage, Match regexMatch)
        {

            return Task.Factory.StartNew(() =>
            {
                if (receivedMessage.Channel.ParentServer == null)
                {
                    _ = receivedMessage.Channel.SendMessageAsync("This command can only be used on a server!");
                    return true;
                }

                if (regexMatch.Groups["ListServerSettings"].Success)
                {
                    if (receivedMessage.Author.Roles != null
                        && !receivedMessage.Author.Roles.Any(x => x.IsAdmin))
                    {
                        _ = receivedMessage.Channel.SendMessageAsync("Access denied! You don't have the permission to use this command!");
                        return true;
                    }

                    var configStore = receivedMessage.ApiWrapper.ConfigStore;
                    var configEntries = configStore.GetAllConfigValues(receivedMessage.Channel.ParentServer);
                    var configTable = "";

                    foreach (var configEntry in configEntries)
                    {
                        configTable += $"{(configEntry.Key + ":").PadRight(20)}{configEntry.Value}\r\n";
                    }

                    _ = receivedMessage.Channel.SendMessageAsync($"Current config for this server:"
                        + $"\r\n{receivedMessage.ApiWrapper.MessageFormatter.CodeBlock(configTable, "json")}");

                }
                return true;
            });
        }
    }
}
