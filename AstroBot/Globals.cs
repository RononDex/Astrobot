using System.IO;
using Cron;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        /// <value></value>
        public static ILoggerFactory LoggerFactory { get; private set; }

        /// <summary>
        /// The settings belonging to the AwesomeChatBot framework
        /// </summary>
        /// <value></value>
        public static AwesomeChatBot.AwesomeChatBotSettings AwesomeChatBotSettings { get; private set; }

        /// <summary>
        /// The bot wide settings
        /// </summary>
        /// <value></value>
        public static Objects.Config.BotSettings BotSettings { get; private set; }

        /// <summary>
        /// Globally stored BotFramework instance for easy access
        /// </summary>
        /// <value></value>
        public static AwesomeChatBot.AwesomeChatBot BotFramework { get; set; }

        public static CronDaemon CronDaemon { get; set; }

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
            LoggerFactory = new LoggerFactory();
            LoggerFactory.AddNLog();

            // Setup CronDaemon
            CronDaemon = new CronDaemon();
            CronDaemon.Start();
        }
    }
}