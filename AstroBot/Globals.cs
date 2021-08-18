using System;
using System.Collections.Generic;
using System.IO;
using AstroBot.Config;
using AstroBot.LaunchLibrary;
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
        /// <summary>
        /// Logger factory used to create logger instances
        /// </summary>
        public static ILoggerFactory? LoggerFactory { get; private set; }

        /// <summary>
        /// The settings belonging to the AwesomeChatBot framework
        /// </summary>
        public static AwesomeChatBot.AwesomeChatBotSettings? AwesomeChatBotSettings { get; private set; }

        /// <summary>
        /// The bot wide settings
        /// </summary>
        public static Objects.Config.BotSettings? BotSettings { get; private set; }

        /// <summary>
        /// Globally stored BotFramework instance for easy access
        /// </summary>
        public static AwesomeChatBot.AwesomeChatBot? BotFramework { get; internal set; }

        public static IReadOnlyList<Launch>? UpcomingRocketLaunchesCache { get; internal set; }

        public static IReadOnlyList<Event>? UpcomingEventsCache { get; internal set; }

        public static Root? SmallTalkReponsesConfig { get; internal set; }

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


            SmallTalkReponsesConfig = JsonConvert.DeserializeObject<Root>(File.ReadAllText("smallTalkConfig.json"));
            Console.WriteLine($"Loaded {SmallTalkReponsesConfig.SmallTalkResponses.Count} small talk responses");

            // Create logger factory
            LoggerFactory = new LoggerFactory();
            LoggerFactory.AddNLog();
        }
    }
}
