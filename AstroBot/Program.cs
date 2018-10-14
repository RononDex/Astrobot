using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.DiscordWrapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NLog.Extensions.Logging;
using System;
using System.IO;

namespace AstroBot
{
    class Program
    {
        static void Main(string[] args)
        {
            NLog.LogManager.LoadConfiguration("nlog.config");

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();
            
            // Setup the bot framework
            var discordSettings = new DiscordSettings();
            configuration.GetSection("DiscordSettings").Bind(discordSettings);

            var discordWrapper = new DiscordWrapper(File.ReadAllText(discordSettings.DiscordTokenPath));

            var chatbotSettings = new AwesomeChatBot.AwesomeChatBotSettings();
            configuration.GetSection("ChatBotSettings").Bind(chatbotSettings);

            services.AddSingleton<ApiWrapper>(discordWrapper);

            // Setup the logging framework
            var provider = services.BuildServiceProvider();

            var serviceProvider = services.BuildServiceProvider();
            var factory = provider.GetService<ILoggerFactory>();
            factory.AddNLog();

            var chatbotFramework = new AwesomeChatBot.AwesomeChatBot(discordWrapper, provider.GetService<ILoggerFactory>(), chatbotSettings);
        }
    }
}
