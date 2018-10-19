using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.DiscordWrapper;
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
        public ServiceProvider ServiceProvider { get; private set; }

        public DiscordAstroBot()
        {
            // Initialize the DI provider, configs and bot framework
            Initialize();

            // Register commands

        }

        /// <summary>
        /// Initializes the bot framework, loads configs etc.
        /// </summary>
        public void Initialize()
        {
            NLog.LogManager.GetLogger(this.GetType().FullName).Info("Initialising bot...");

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

            var chatbotSettings = new AwesomeChatBot.AwesomeChatBotSettings();
            configuration.GetSection("AwesomeChatBotSettings").Bind(chatbotSettings);

            NLog.LogManager.GetLogger(this.GetType().FullName).Info($" - DiscordSettings.TokenPath:                 {discordSettings.DiscordTokenPath}");
            NLog.LogManager.GetLogger(this.GetType().FullName).Info($" - AwesomeChatBotSettings.ConfigFolderPath:   {chatbotSettings.ConfigFolderPath}");

            // Create and store a service provider
            this.ServiceProvider = services.BuildServiceProvider();
            var factory = ServiceProvider.GetService<ILoggerFactory>();
            factory.AddNLog();

            // Setup bot framework
            var discordWrapper = new DiscordWrapper(File.ReadAllText(discordSettings.DiscordTokenPath), factory);
            var chatbotFramework = new AwesomeChatBot.AwesomeChatBot(discordWrapper, factory, chatbotSettings);
            
            services.AddSingleton<ApiWrapper>(discordWrapper);
            services.AddSingleton<AwesomeChatBot.AwesomeChatBot>(chatbotFramework);

            // Create and store a service provider
            this.ServiceProvider = services.BuildServiceProvider();
            factory = ServiceProvider.GetService<ILoggerFactory>();
            factory.AddNLog();
        }
    }
}
