using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        public static ILoggerFactory LoggerFactory { get; set; }

        /// <summary>
        /// The DI service provider
        /// </summary>
        public static ServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Initialize the global variables
        /// </summary>
        public static void InitGlobals(){

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

            NLog.LogManager.GetLogger("AstroBot.Globals").Info("Reading settings...");
            var botSettings = new Objects.Config.BotSettings();
            configuration.GetSection("BotSettings").Bind(botSettings);
            services.AddSingleton(botSettings);

            // Create service provider
            ServiceProvider = services.BuildServiceProvider();
            LoggerFactory = ServiceProvider.GetService<ILoggerFactory>();
            LoggerFactory.AddNLog();
        }
    }
}