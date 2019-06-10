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
            new ConfigValue { Key = "GreetNewUsersMessage", Value = "Welcome @UserMention" }
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
                if (existingEntries.Any(x => x.Key == defaultConfigValue.Key))
                {
                    continue;
                }

                Globals.BotFramework.ConfigStore.SetConfigValue(defaultConfigValue.Key, defaultConfigValue.Value, server);
            }

            // Delete existing entries that are not listed in "DefaultServerConfig"
            var entriesToDelete = existingEntries.Where(x => !DefaultServerConfig.Any(y => x.Key == y.Key)).ToList();

            for (int i = 0; i < entriesToDelete.Count; i++)
            {
                var entryToDelete = entriesToDelete[i];
                Globals.BotFramework.ConfigStore.DeleteConfigEntry(entryToDelete.Key, server);
            }
        }
    }
}