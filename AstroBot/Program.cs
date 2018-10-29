using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.DiscordWrapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NLog.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AstroBot
{
    class Program
    {
        static void Main(string[] args)
        {
            // Not every host console supports setting window width
            try {
                // Set console width
                Console.WindowWidth = 190;
            }
            catch {}

            // Register a global exception handler
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            // Async wrapper of Main method
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Async main function serves as entry point of the app
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync()
        {
            NLog.LogManager.LoadConfiguration("nlog.config");

            NLog.LogManager.GetLogger(this.GetType().FullName).Info("---------------------------------------------------------");
            NLog.LogManager.GetLogger(this.GetType().FullName).Info("----------------- Launching Astro bot -------------------");
            NLog.LogManager.GetLogger(this.GetType().FullName).Info("---------------------------------------------------------");
            NLog.LogManager.GetLogger(this.GetType().FullName).Info("");
            NLog.LogManager.GetLogger(this.GetType().FullName).Info("NLog logging system loaded");

            new DiscordAstroBot().Initialize();

            await Task.Delay(-1);
        }

        /// <summary>
        /// Global exception handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            NLog.LogManager.GetLogger("AstroBot").Fatal($"Unhandled exception catched by global handler: {e.ExceptionObject.ToString()}");
        }
    }
}
