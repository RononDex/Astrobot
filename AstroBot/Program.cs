using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace AstroBot
{
    class Program
    {
        static void Main(string[] args)
        {
            // Only windows supports setting the console width, and not every windows console either
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    Console.WindowWidth = 190;
                }
                catch (Exception)
                {
                    Console.WriteLine("Your terminal / console does not support setting window width!");
                }
            }

            // Register a global exception handler
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            // Async wrapper of Main method
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Async main function serves as entry point of the app
        /// </summary>
        /// <returns></returns>
        public Task MainAsync()
        {
            LogManager.Setup().LoadConfigurationFromFile("nlog.config");

            LogManager.GetCurrentClassLogger().Info("---------------------------------------------------------");
            LogManager.GetCurrentClassLogger().Info("----------------- Launching Astro bot -------------------");
            LogManager.GetCurrentClassLogger().Info("---------------------------------------------------------");
            LogManager.GetCurrentClassLogger().Info("");
            LogManager.GetCurrentClassLogger().Info("NLog logging system loaded");

            // Initialize globals
            Globals.InitGlobals();

            new AstroBotController().Initialize();

            // Wait indefinitely so the bot can run in the background
            return Task.Delay(-1, CancellationToken.None);
        }

        /// <summary>
        /// Global exception handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            NLog.LogManager.GetLogger(nameof(AstroBotController)).Fatal($"Unhandled exception caught by global handler: {e.ExceptionObject}");
        }
    }
}
