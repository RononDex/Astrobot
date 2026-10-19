using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    public class AdminCommand : AwesomeChatBot.Commands.Command, ISlashCommand, ICommandDescription
    {
        public override string Name => "Admin";

        public string Description => "Administrative commands";

        public string SlashName => "admin";

        public string SlashDescription => Description;

        public IReadOnlyList<SlashCommandOption> Options => new[]
        {
            new SlashCommandOption
            {
                Name = "action", Description = "What to do", Type = SlashOptionType.String, Required = true,
                Choices = new[]
                {
                    new SlashCommandChoice<string> { Name = "Show server config", Value = "show-config" },
                    new SlashCommandChoice<string> { Name = "Set server config (key, value)", Value = "set-config" },
                    new SlashCommandChoice<string> { Name = "Ban user from bot usage (user_id)", Value = "ban" },
                    new SlashCommandChoice<string> { Name = "Unban user from bot usage (user_id)", Value = "unban" },
                }
            },
            new SlashCommandOption
            {
                Name = "key", Description = "Config key (set-config)", Type = SlashOptionType.String, Required = false,
                Choices = Config.DefaultConfigsHelper.DefaultServerConfig
                    .Select(x => new SlashCommandChoice<string> { Name = x.Key, Value = x.Key })
                    .ToList()
            },
            new SlashCommandOption { Name = "value", Description = "Config value (set-config)", Type = SlashOptionType.String, Required = false },
            new SlashCommandOption { Name = "user_id", Description = "Id of the user (ban / unban)", Type = SlashOptionType.String, Required = false },
        };

        public async Task ExecuteSlashCommandAsync(SlashCommandInvocation invocation)
        {
            var server = invocation.Channel?.ParentServer;
            if (server == null)
            {
                await invocation.RespondAsync("This command can only be used on a server!").ConfigureAwait(false);
                return;
            }
            if (invocation.Author.Roles?.Any(x => x.IsAdmin) != true)
            {
                await invocation.RespondAsync("Access denied! You don't have the permission to use this command!").ConfigureAwait(false);
                return;
            }

            var configStore = Globals.BotFramework!.ConfigStore;
            var key = invocation.GetOption<string>("key");
            var value = invocation.GetOption<string>("value");
            var userId = invocation.GetOption<string>("user_id")?.Trim();

            var action = invocation.GetOption<string>("action");
            switch (action)
            {
                case "show-config":
                    await PrintServerConfigAsync(invocation, server).ConfigureAwait(false);
                    break;

                case "set-config":
                    if (string.IsNullOrWhiteSpace(key) || value == null)
                    {
                        await invocation.RespondAsync("set-config needs a key and a value").ConfigureAwait(false);
                        return;
                    }

                    if (!configStore.DoesConfigEntryWithKeyExist(key, server))
                    {
                        await invocation.RespondAsync($"Unknown configuration key").ConfigureAwait(false);
                        return;
                    }

                    if (!ValidateNewConfigValue(key, value))
                    {
                        await invocation.RespondAsync($"{value} is not valid setting for {key}!").ConfigureAwait(false);
                        return;
                    }
                    configStore.SetConfigValue(key, value, server);
                    await invocation.RespondAsync($"Done").ConfigureAwait(false);
                    break;

                case "ban":
                case "unban":
                    if (string.IsNullOrEmpty(userId) || !userId.All(char.IsDigit))
                    {
                        await invocation.RespondAsync("ban / unban needs a numeric user_id").ConfigureAwait(false);
                        return;
                    }
                    if (!IsServerAllowedForBanning(server))
                    {
                        await invocation.RespondAsync("Your server is not whitelisted for this command").ConfigureAwait(false);
                        return;
                    }

                    var ban = action == "ban";
                    var currentBannedUsersList = Globals.GloballyBannedUsers;
                    if (ban && !currentBannedUsersList.Contains(userId))
                    {
                        currentBannedUsersList.Add(userId);
                    }
                    else if (!ban)
                    {
                        currentBannedUsersList.Remove(userId);
                    }

                    configStore.SetConfigValue("GlobalUserBanList", string.Join(';', currentBannedUsersList));
                    Globals.UpdateBanCache(Globals.ApiWrapper);

                    await invocation.RespondAsync(ban
                        ? $"User {userId} has been banned from using this bot globally (on all servers)"
                        : $"User {userId} has been unbanned from using this bot globally (on all servers)").ConfigureAwait(false);
                    break;

                default:
                    await invocation.RespondAsync("Unknown action").ConfigureAwait(false);
                    break;
            }
        }

        private bool ValidateNewConfigValue(string key, string value)
        {
            switch (key.ToLower())
            {
                case "rocketlaunchesnewsenabled":
                    return bool.TryParse(value, out _);
                default:
                    return true;
            }

        }

        private static bool IsServerAllowedForBanning(Server server)
        {
            return Globals.BotSettings!.GlobalUserBanServerWhiteList.Any(x => x == server.ServerID);
        }

        private static Task PrintServerConfigAsync(SlashCommandInvocation invocation, Server server)
        {
            var configEntries = Globals.BotFramework!.ConfigStore.GetAllConfigValues(server).OrderBy(x => x.Key);
            var configTable = "";

            foreach (var configEntry in configEntries)
            {
                configTable += $"{(configEntry.Key + ":").PadRight(30)}{configEntry.Value}\r\n";
            }

            return invocation.RespondAsync($"Current config for this server:"
                + $"\r\n{Globals.ApiWrapper.MessageFormatter.CodeBlock(configTable, "json")}");
        }
    }
}
