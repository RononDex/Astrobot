using System.Linq;
using System.Collections.Generic;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Config;

namespace AstroBot.Config
{
    public static class DefaultConfigsHelper
    {
        private static IReadOnlyList<ConfigValue> DefaultServerConfig => new List<ConfigValue>
        {
            new ConfigValue { Key = "GreetNewUsersChannel", Value = null },
            new ConfigValue { Key = "GreetNewUsers", Value = "false" },
            new ConfigValue { Key = "GreetNewUsersMessage", Value = "Welcome @UserMention" },
            new ConfigValue { Key = "UserSelfAssignableRoles", Value = "" },
            new ConfigValue { Key = "RocketLaunchesNewsEnabled", Value = "false" },
            new ConfigValue { Key = "RocketLaunchesNewsChannel", Value = "news" },
            new ConfigValue { Key = "RocketLaunchesIntermediateTagRole", Value = "LaunchNotify" },
        };

        /// <summary>
        /// Sets up the default values for a server
        /// </summary>
        /// <param name="server"></param>
        public static void SetupDefaultServerConfig(Server server)
        {
            var existingEntries = Globals.BotFramework.ConfigStore.GetAllConfigValues(server);

            foreach (var defaultConfigValue in DefaultServerConfig)
            {
                if (existingEntries.Any(x => string.Equals(x.Key, defaultConfigValue.Key, System.StringComparison.Ordinal)))
                {
                    continue;
                }

                Globals.BotFramework.ConfigStore.SetConfigValue(defaultConfigValue.Key, defaultConfigValue.Value, server);
            }

            // Delete existing entries that are not listed in "DefaultServerConfig"
            var entriesToDelete = existingEntries.Where(x => !DefaultServerConfig.Any(y => string.Equals(x.Key, y.Key, System.StringComparison.Ordinal))).ToList();

            foreach (var entryToDelete in entriesToDelete)
            {
                Globals.BotFramework.ConfigStore.DeleteConfigEntry(entryToDelete.Key, server);
            }
        }
    }
}