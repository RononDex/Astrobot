using AwesomeChatBot.DiscordWrapper;
using AstroBot.Objects.Config;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace AstroBot
{
    /// <summary>
    /// The main class controlling / starting the bot
    /// </summary>
    public class DiscordAstroBot
    {
        private AwesomeChatBot.AwesomeChatBot BotFramework { get; set; }

        public void Initialize()
        {
            // Initialize the DI provider, configs and bot framework
            InitializeFramework();

            // Register commands
            var botFramework = this.BotFramework;

            botFramework.RegisterCommand(new Commands.TestCommand());
            botFramework.RegisterCommand(new Commands.LocationCommand());
            botFramework.RegisterCommand(new Commands.HelpCommand());
            botFramework.RegisterCommand(new Commands.WeatherCommand());
            botFramework.RegisterCommand(new Commands.Astrometry());
            botFramework.RegisterCommandHandler(new AwesomeChatBot.Commands.Handlers.RegexCommandHandler());
        }

        /// <summary>
        /// Initializes the bot framework, loads configs etc.
        /// </summary>
        private void InitializeFramework()
        {
            Log<DiscordAstroBot>.Info("Initializing bot...");

            // Set up configuration laoder
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            // Load settings
            NLog.LogManager.GetLogger(this.GetType().FullName).Info("Reading settings...");
            var discordSettings = new DiscordSettings();
            configuration.GetSection("DiscordSettings").Bind(discordSettings);

            // if discord token file does not exist, throw exception
            if (!File.Exists(discordSettings.DiscordTokenPath))
                throw new ArgumentException($"The discord token file {discordSettings.DiscordTokenPath} does not exist!");

            NLog.LogManager.GetLogger(this.GetType().FullName).Info($" - DiscordSettings.TokenPath:                 {discordSettings.DiscordTokenPath}");
            NLog.LogManager.GetLogger(this.GetType().FullName).Info($" - AwesomeChatBotSettings.ConfigFolderPath:   {Globals.AwesomeChatBotSettings.ConfigFolderPath}");
            NLog.LogManager.GetLogger(this.GetType().FullName).Info($" - BotSettings.GoogleGeoLocationTokenPath:    {Globals.BotSettings.GoogleGeoLocationTokenPath}");
            NLog.LogManager.GetLogger(this.GetType().FullName).Info($" - BotSettings.NovaAstrometryApiKeyPath:      {Globals.BotSettings.NovaAstrometryApiKeyPath}");

            var discordToken = File.ReadAllText(discordSettings.DiscordTokenPath).Replace("\r", "").Replace("\n", "");

            // Setup bot framework
            var discordWrapper = new DiscordWrapper(discordToken, Globals.LoggerFactory);
            BotFramework = new AwesomeChatBot.AwesomeChatBot(discordWrapper, Globals.LoggerFactory, Globals.AwesomeChatBotSettings);
        }
    }
}
