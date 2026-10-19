using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AstroBot.LaunchLibrary;
using AwesomeChatBot.ApiWrapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Extensions.Logging;

namespace AstroBot
{
    /// <summary>
    /// Global variables that are used from the whole application
    /// </summary>
    public static class Globals
    {
        // The settings, logger factory and bot framework are set once at startup (InitGlobals / AstroBotController),
        // before anything else runs, hence null! instead of nullable types
        /// <summary>
        /// Logger factory used to create logger instances
        /// </summary>
        public static ILoggerFactory LoggerFactory { get; private set; } = null!;

        /// <summary>
        /// The settings belonging to the AwesomeChatBot framework
        /// </summary>
        public static AwesomeChatBot.AwesomeChatBotSettings AwesomeChatBotSettings { get; private set; } = null!;

        /// <summary>
        /// The bot wide settings
        /// </summary>
        public static Objects.Config.BotSettings BotSettings { get; private set; } = null!;

        /// <summary>
        /// Globally stored BotFramework instance for easy access
        /// </summary>
        public static AwesomeChatBot.AwesomeChatBot BotFramework { get; internal set; } = null!;

        /// <summary>
        /// The (only) chat api wrapper, slash command invocations don't carry their own
        /// </summary>
        public static ApiWrapper ApiWrapper => BotFramework.ApiWrappers[0];

        /// <summary>
        /// Shared HttpClient, one instance for the whole app to avoid socket exhaustion
        /// </summary>
        public static System.Net.Http.HttpClient HttpClient { get; } = new System.Net.Http.HttpClient();

        public static IReadOnlyList<Launch> UpcomingRocketLaunchesCache { get; internal set; } = [];

        public static IReadOnlyList<Event> UpcomingEventsCache { get; internal set; } = [];

        public static List<string> GloballyBannedUsers { get; set; } = new List<string>();

        /// <summary>
        /// Initialize the global variables
        /// </summary>
        public static void InitGlobals()
        {
            // Set up configuration laoder
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            NLog.LogManager.GetLogger("AstroBot.Globals").Info("Reading settings...");

            var botSettings = new Objects.Config.BotSettings();
            configuration.GetSection("BotSettings").Bind(botSettings);
            BotSettings = botSettings;

            var awesomeChatbotSettings = new AwesomeChatBot.AwesomeChatBotSettings();
            configuration.GetSection("AwesomeChatBotSettings").Bind(awesomeChatbotSettings);
            AwesomeChatBotSettings = awesomeChatbotSettings;

            // Create logger factory
            // Trace, so nlog.config stays the only place that filters log levels
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Trace).AddNLog());
        }

        public static void UpdateBanCache(ApiWrapper wrapper)
        {
            var bannedList = wrapper.ConfigStore.GetConfigValue<string>("GlobalUserBanList")?.Split(";");
            if (bannedList == null)
                GloballyBannedUsers = new List<string>();
            else
                GloballyBannedUsers = bannedList.ToList();
        }
    }
}
