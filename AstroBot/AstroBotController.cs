using AwesomeChatBot.Discord;
using AstroBot.Objects.Config;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using AwesomeChatBot.ApiWrapper;
using System.Collections.Generic;
using AstroBot.CronTasks;
using System.Linq;
using Discord;

namespace AstroBot
{
    /// <summary>
    /// The main class controlling / starting the bot
    /// </summary>
    public class AstroBotController
    {
        public void Initialize()
        {
            // Initialize the DI provider, configs and bot framework
            var botFramework = InitializeFramework();

            // Register commands
            botFramework.RegisterCommand(new Commands.ValidateUserBanCommand());
            botFramework.RegisterCommand(new Commands.TestCommand());
            botFramework.RegisterCommand(new Commands.LocationCommand());
            botFramework.RegisterCommand(new Commands.HelpCommand());
            botFramework.RegisterCommand(new Commands.WeatherCommand());
            botFramework.RegisterCommand(new Commands.AstrometryCommand());
            botFramework.RegisterCommand(new Commands.SimbadCommand());
            botFramework.RegisterCommand(new Commands.AdminCommand());
            botFramework.RegisterCommand(new Commands.SkySurveyCommand());
            botFramework.RegisterCommandHandler(new AwesomeChatBot.Commands.Handlers.RegexCommandHandler());

            // Slash commands don't go through the regex handlers, so ValidateUserBanCommand doesn't cover them
            botFramework.SlashCommandFilter = async invocation =>
            {
                if (!Globals.GloballyBannedUsers.Contains(invocation.Author.UserID))
                    return true;

                await invocation.RespondAsync("You have been banned from using this bot").ConfigureAwait(false);
                return false;
            };

            // Register events
            botFramework.ServerAvailable += Events.ServerEvents.ServerAvailableAsync;

            // Register CronJobs
            CronTaskManager.Register(new UpdateLaunchLibraryCache());
            CronTaskManager.Register(new IntermediateRocketLaunchNotify());
            CronTaskManager.Register(new UpcomingLaunches());
            CronTaskManager.Register(new WriteStatisticsToLog());

            // Update Cache at app startup
            new UpdateLaunchLibraryCache().ExecuteAsync().GetAwaiter().GetResult();

            Globals.UpdateBanCache(botFramework.ApiWrappers.First());
        }

        /// <summary>
        /// Initializes the bot framework, loads configs etc.
        /// </summary>
        private AwesomeChatBot.AwesomeChatBot InitializeFramework()
        {
            Log<AstroBotController>.Info("Initializing bot...");

            // Set up configuration laoder
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            // Load settings
            NLog.LogManager.GetCurrentClassLogger().Info("Reading settings...");
            var discordSettings = new DiscordSettings();
            configuration.GetSection("DiscordSettings").Bind(discordSettings);

            // if discord token file does not exist, throw exception
            if (!File.Exists(discordSettings.DiscordTokenPath))
            {
                throw new ArgumentException($"The discord token file {discordSettings.DiscordTokenPath} does not exist!");
            }

            NLog.LogManager.GetCurrentClassLogger().Info($" - DiscordSettings.TokenPath:                         {discordSettings.DiscordTokenPath}");
            NLog.LogManager.GetCurrentClassLogger().Info($" - AwesomeChatBotSettings.ConfigFolderPath:           {Globals.AwesomeChatBotSettings.ConfigFolderPath}");
            NLog.LogManager.GetCurrentClassLogger().Info($" - AwesomeChatBotSettings.CommandsEnabledByDefault:   {Globals.AwesomeChatBotSettings.CommandsEnabledByDefault}");
            NLog.LogManager.GetCurrentClassLogger().Info($" - BotSettings.GoogleGeoLocationTokenPath:            {Globals.BotSettings.GoogleGeoLocationTokenPath}");
            NLog.LogManager.GetCurrentClassLogger().Info($" - BotSettings.NovaAstrometryApiKeyPath:              {Globals.BotSettings.NovaAstrometryApiKeyPath}");

            var discordToken = File.ReadAllText(discordSettings.DiscordTokenPath).Replace("\r", "").Replace("\n", "");

            // Setup bot framework
            // No privileged intents, so nothing has to be enabled in the discord developer portal
            var discordWrapper = new DiscordWrapper(discordToken, Globals.LoggerFactory, GatewayIntents.AllUnprivileged);
            var wrappers = new List<ApiWrapper> { discordWrapper };
            var botFramework = new AwesomeChatBot.AwesomeChatBot(wrappers, Globals.LoggerFactory, Globals.AwesomeChatBotSettings);

            Globals.BotFramework = botFramework;
            return botFramework;
        }
    }
}
