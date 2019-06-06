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
            new ConfigValue { Key = "GreetingChannel", Value = null }
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
                if (!existingEntries.Any(x => x.Key == defaultConfigValue.Key))
                    continue;

                Globals.BotFramework.ConfigStore.SetConfigValue(defaultConfigValue.Key, defaultConfigValue.Value);
            }
        }
    }
}