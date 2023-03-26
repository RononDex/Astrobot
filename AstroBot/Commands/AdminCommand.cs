using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;
using Newtonsoft.Json;

namespace AstroBot.Commands
{
    public class AdminCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {
        public override string Name => "Admin";

        public List<string> Regex => new List<string>
        {
            "(?'ListServerSettings'show server config)",
            "set server config (?'SetServerConfigKey'\\w*) (?'SetServerConfigValue'.*\\w)",
            "unban (?'UnbanBotUsageUserId'\\d*) from bot usage",
            "ban (?'BanBotUsageUserId'\\d*) from bot usage",
        };

        public string Description => "Administrative commands";

        public string[] ExampleCalls => new[]
        {
            "show server config",
            "set server config <key> <value>"
        };

        public async Task<bool> ExecuteRegexCommandAsync(ReceivedMessage receivedMessage, Match regexMatch)
        {
            if (receivedMessage.Channel.ParentServer == null)
            {
                await receivedMessage.Channel.SendMessageAsync("This command can only be used on a server!").ConfigureAwait(false);
                return true;
            }
            if (receivedMessage.Author.Roles?.Any(x => x.IsAdmin) == false)
            {
                await receivedMessage.Channel.SendMessageAsync("Access denied! You don't have the permission to use this command!").ConfigureAwait(false);
                return true;
            }

            if (regexMatch.Groups["ListServerSettings"].Success)
            {
                await PrintServerConfigAsync(receivedMessage).ConfigureAwait(false);
            }
            else if (regexMatch.Groups["SetServerConfigKey"].Success)
            {
                var key = regexMatch.Groups["SetServerConfigKey"].Value;
                var value = regexMatch.Groups["SetServerConfigValue"].Value;

                if (!Globals.BotFramework!.ConfigStore.DoesConfigEntryWithKeyExist(key, receivedMessage.Channel.ParentServer))
                {
                    await receivedMessage.Channel.SendMessageAsync($"Unknown configuration key").ConfigureAwait(false);
                    return true;
                }

                if (!ValidateNewConfigValue(key, value))
                {
                    await receivedMessage.Channel.SendMessageAsync($"{value} is not valid setting for {key}!").ConfigureAwait(false);
					return true;
                }
                receivedMessage.ApiWrapper.ConfigStore.SetConfigValue(key, value, receivedMessage.Channel.ParentServer);
                await receivedMessage.Channel.SendMessageAsync($"Done").ConfigureAwait(false);
            }
            else if (regexMatch.Groups["BanBotUsageUserId"].Success)
            {
                var userId = regexMatch.Groups["BanBotUsageUserId"].Value;
                if (IsServerAllowedForBanning(receivedMessage))
                {
                    await receivedMessage.Channel.SendMessageAsync("Your server is not whitelisted for this command");
                    return true;
                }

                var currentBannedUsersList = Globals.GloballyBannedUsers;
                if (!currentBannedUsersList.Contains(userId))
                {
                    currentBannedUsersList.Add(userId);
                }

                receivedMessage.ApiWrapper.ConfigStore.SetConfigValue("GlobalUserBanList", string.Join(';', currentBannedUsersList));
                Globals.UpdateBanCache(receivedMessage.ApiWrapper);

                await receivedMessage.Channel.SendMessageAsync($"User {userId} has been banned from using this bot globally (on all servers)");
            }
            else if (regexMatch.Groups["UnbanBotUsageUserId"].Success)
            {
                var userId = regexMatch.Groups["UnbanBotUsageUserId"].Value;
                if (IsServerAllowedForBanning(receivedMessage))
                {
                    await receivedMessage.Channel.SendMessageAsync("Your server is not whitelisted for this command");
                    return true;
                }

                var currentBannedUsersList = Globals.GloballyBannedUsers;
                if (currentBannedUsersList.Contains(userId))
                {
                    currentBannedUsersList.Remove(userId);
                }

                receivedMessage.ApiWrapper.ConfigStore.SetConfigValue("GlobalUserBanList", string.Join(';', currentBannedUsersList));
                Globals.UpdateBanCache(receivedMessage.ApiWrapper);

                await receivedMessage.Channel.SendMessageAsync($"User {userId} has been unbanned from using this bot globally (on all servers)");
            }

            return true;
        }

        private bool ValidateNewConfigValue(string key, string value)
        {
            switch (key.ToLower())
            {
                case "greetnewusers":
                case "rocketlaunchesnewsenabled":
                    return bool.TryParse(value, out _);
                default:
                    return true;
            }

        }

        private static bool IsServerAllowedForBanning(ReceivedMessage receivedMessage)
        {
            return (!Globals.BotSettings!.GlobalUserBanServerWhiteList.Any(x => x == receivedMessage.Channel.ParentServer.ServerID));
        }

        private static Task PrintServerConfigAsync(ReceivedMessage receivedMessage)
        {
            var configStore = receivedMessage.ApiWrapper.ConfigStore;
            var configEntries = configStore.GetAllConfigValues(receivedMessage.Channel.ParentServer).OrderBy(x => x.Key);
            var configTable = "";

            foreach (var configEntry in configEntries)
            {
                configTable += $"{(configEntry.Key + ":").PadRight(30)}{configEntry.Value}\r\n";
            }

            return receivedMessage.Channel.SendMessageAsync($"Current config for this server:"
                + $"\r\n{receivedMessage.ApiWrapper.MessageFormatter.CodeBlock(configTable, "json")}");
        }
    }
}
