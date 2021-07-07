using AwesomeChatBot.Discord;
using AstroBot.Objects.Config;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using AwesomeChatBot.ApiWrapper;
using System.Collections.Generic;
using AstroBot.CronTasks;

namespace AstroBot
{
    /// <summary>
    /// The main class controlling / starting the bot
    /// </summary>
    public class AstroBotController
    {
        private AwesomeChatBot.AwesomeChatBot BotFramework { get; set; }

        public void Initialize()
        {
            // Initialize the DI provider, configs and bot framework
            InitializeFramework();

            var botFramework = BotFramework;

            // Register commands
            botFramework.RegisterCommand(new Commands.TestCommand());
            botFramework.RegisterCommand(new Commands.LocationCommand());
            botFramework.RegisterCommand(new Commands.HelpCommand());
            botFramework.RegisterCommand(new Commands.WeatherCommand());
            botFramework.RegisterCommand(new Commands.AstrometryCommand());
            botFramework.RegisterCommand(new Commands.SimbadCommand());
            botFramework.RegisterCommand(new Commands.AdminCommand());
            botFramework.RegisterCommand(new Commands.DssCommand());
            botFramework.RegisterCommandHandler(new AwesomeChatBot.Commands.Handlers.RegexCommandHandler());

            // Register events
            botFramework.NewUserJoinedServer += Events.UserServerEvents.UserJoinedServerAsync;
            botFramework.ServerAvailable += Events.ServerEvents.ServerAvailableAsync;
            botFramework.MessageDeleted += Events.ServerEvents.MessageDeletedAsync;

            // Register CronJobs
            CronTaskManager.Register(new UpdateLaunchLibraryCache());
            CronTaskManager.Register(new IntermediateRocketLaunchNotify());
            CronTaskManager.Register(new UpcomingLaunches());

            // Update Cache at app startup
            new UpdateLaunchLibraryCache().ExecuteAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Initializes the bot framework, loads configs etc.
        /// </summary>
        private void InitializeFramework()
        {
            Log<AstroBotController>.Info("Initializing bot...");

            // Set up configuration laoder
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            // Load settings
            NLog.LogManager.GetLogger(GetType().FullName).Info("Reading settings...");
            var discordSettings = new DiscordSettings();
            configuration.GetSection("DiscordSettings").Bind(discordSettings);

            // if discord token file does not exist, throw exception
            if (!File.Exists(discordSettings.DiscordTokenPath))
            {
                throw new ArgumentException($"The discord token file {discordSettings.DiscordTokenPath} does not exist!");
            }

            NLog.LogManager.GetLogger(GetType().FullName).Info($" - DiscordSettings.TokenPath:                         {discordSettings.DiscordTokenPath}");
            NLog.LogManager.GetLogger(GetType().FullName).Info($" - AwesomeChatBotSettings.ConfigFolderPath:           {Globals.AwesomeChatBotSettings.ConfigFolderPath}");
            NLog.LogManager.GetLogger(GetType().FullName).Info($" - AwesomeChatBotSettings.CommandsEnabledByDefault:   {Globals.AwesomeChatBotSettings.CommandsEnabledByDefault}");
            NLog.LogManager.GetLogger(GetType().FullName).Info($" - BotSettings.GoogleGeoLocationTokenPath:            {Globals.BotSettings.GoogleGeoLocationTokenPath}");
            NLog.LogManager.GetLogger(GetType().FullName).Info($" - BotSettings.NovaAstrometryApiKeyPath:              {Globals.BotSettings.NovaAstrometryApiKeyPath}");

            var discordToken = File.ReadAllText(discordSettings.DiscordTokenPath).Replace("\r", "").Replace("\n", "");

            // Setup bot framework
            var discordWrapper = new DiscordWrapper(discordToken, Globals.LoggerFactory);
            var wrappers = new List<ApiWrapper>() { discordWrapper };
            BotFramework = new AwesomeChatBot.AwesomeChatBot(wrappers, Globals.LoggerFactory, Globals.AwesomeChatBotSettings);

            Globals.BotFramework = BotFramework;
        }
    }
}
