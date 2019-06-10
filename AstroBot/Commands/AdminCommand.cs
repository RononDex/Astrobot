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
            "(?'ListServerSettings'show server config)",
            "set server config (?'SetServerConfigKey'.*\\w) (?'SetServerConfigValue'.*\\w)",
        };

        public string Description => "Administrative commands";

        public string[] ExampleCalls => new[]
        {
            "show server config",
            "set server config <key> <value>"
        };

        public Task<bool> ExecuteRegexCommand(ReceivedMessage receivedMessage, Match regexMatch)
        {
            return Task.Factory.StartNew(() =>
            {
                if (receivedMessage.Channel.ParentServer == null)
                {
                    _ = receivedMessage.Channel.SendMessageAsync("This command can only be used on a server!");
                    return true;
                }
                if (receivedMessage.Author.Roles?.Any(x => x.IsAdmin) == false)
                {
                    _ = receivedMessage.Channel.SendMessageAsync("Access denied! You don't have the permission to use this command!");
                    return true;
                }

                if (regexMatch.Groups["ListServerSettings"].Success)
                {
                    PrintServerConfig(receivedMessage);
                }
                else if (regexMatch.Groups["SetServerConfigKey"].Success)
                {
                    var key = regexMatch.Groups["SetServerConfigKey"].Value;
                    var value = regexMatch.Groups["SetServerConfigValue"].Value;

                    if (!Globals.BotFramework.ConfigStore.DoesConfigEntryWithKeyExist(key, receivedMessage.Channel.ParentServer))
                    {
                        _ = receivedMessage.Channel.SendMessageAsync($"Unknown configuration key \"{key}\"");
                        return true;
                    }

                    receivedMessage.ApiWrapper.ConfigStore.SetConfigValue(key, value, receivedMessage.Channel.ParentServer);
                    _ = receivedMessage.Channel.SendMessageAsync($"{key} set to {value}!");
                }

                return true;
            });
        }

        private static void PrintServerConfig(ReceivedMessage receivedMessage)
        {
            var configStore = receivedMessage.ApiWrapper.ConfigStore;
            var configEntries = configStore.GetAllConfigValues(receivedMessage.Channel.ParentServer).OrderBy(x => x.Key);
            var configTable = "";

            foreach (var configEntry in configEntries)
            {
                configTable += $"{(configEntry.Key + ":").PadRight(30)}{configEntry.Value}\r\n";
            }

            _ = receivedMessage.Channel.SendMessageAsync($"Current config for this server:"
                + $"\r\n{receivedMessage.ApiWrapper.MessageFormatter.CodeBlock(configTable, "json")}");
        }
    }
}
