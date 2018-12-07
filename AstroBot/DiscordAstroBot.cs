using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.DiscordWrapper;
using AstroBot.Objects.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AstroBot
{
    /// <summary>
    /// The main class controlling / starting the bot
    /// </summary>
    public class DiscordAstroBot
    {
        /// <summary>
        /// The DI service provider
        /// </summary>
        public static ServiceProvider ServiceProvider { get; private set; }
        public void Initialize()
        {
            // Initialize the DI provider, configs and bot framework
            InitializeFramework();

            // Register commands
            var botFramework = ServiceProvider.GetRequiredService<AwesomeChatBot.AwesomeChatBot>();            

            botFramework.RegisterCommand(new Commands.TestCommand());
            botFramework.RegisterCommand(new Commands.LocationCommand());
            botFramework.RegisterCommand(new Commands.HelpCommand());
            botFramework.RegisterCommand(new Commands.WeatherCommand());
            botFramework.RegisterCommandHandler(new AwesomeChatBot.Commands.Handlers.RegexCommandHandler());
        }

        /// <summary>
        /// Initializes the bot framework, loads configs etc.
        /// </summary>
        private void InitializeFramework()
        {
            Log<DiscordAstroBot>.Info("Initialising bot...");

            // Set up DI (dependency injection)
            var services = new ServiceCollection()
                .AddLogging()
                .AddOptions();

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

            var awesomeChatbotSettings = new AwesomeChatBot.AwesomeChatBotSettings();
            configuration.GetSection("AwesomeChatBotSettings").Bind(awesomeChatbotSettings);

            var botSettings = new Objects.Config.BotSettings();
            configuration.GetSection("BotSettings").Bind(botSettings);

            NLog.LogManager.GetLogger(this.GetType().FullName).Info($" - DiscordSettings.TokenPath:                 {discordSettings.DiscordTokenPath}");
            NLog.LogManager.GetLogger(this.GetType().FullName).Info($" - AwesomeChatBotSettings.ConfigFolderPath:   {awesomeChatbotSettings.ConfigFolderPath}");
            NLog.LogManager.GetLogger(this.GetType().FullName).Info($" - BotSettings.GoogleGeoLocationTokenPath:    {botSettings.GoogleGeoLocationTokenPath}");

            // Create and store a service provider
            ServiceProvider = services.BuildServiceProvider();
            var loggerFactoryTmp = ServiceProvider.GetService<ILoggerFactory>();
            loggerFactoryTmp.AddNLog();

            // Setup bot framework
            var discordWrapper = new DiscordWrapper(File.ReadAllText(discordSettings.DiscordTokenPath), loggerFactoryTmp);
            var chatbotFramework = new AwesomeChatBot.AwesomeChatBot(discordWrapper, loggerFactoryTmp, awesomeChatbotSettings);
            
            services.AddSingleton<ApiWrapper>(discordWrapper);
            services.AddSingleton<AwesomeChatBot.AwesomeChatBot>(chatbotFramework);

            // Create and store a service provider
            ServiceProvider = services.BuildServiceProvider();            
        }
    }
}
