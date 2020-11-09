using System;
using System.Threading.Tasks;
using NLog;

namespace AstroBot
{
    class Program
    {
        static void Main(string[] args)
        {
            // Not every host console supports setting window width
            try
            {
                // Set console width
                Console.WindowWidth = 190;
            }
            catch (Exception)
            {
                Console.WriteLine("Your terminal / console does not support setting window width!");
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
            LogManager.LoadConfiguration("nlog.config");

            LogManager.GetLogger(GetType().FullName).Info("---------------------------------------------------------");
            LogManager.GetLogger(GetType().FullName).Info("----------------- Launching Astro bot -------------------");
            LogManager.GetLogger(GetType().FullName).Info("---------------------------------------------------------");
            LogManager.GetLogger(GetType().FullName).Info("");
            LogManager.GetLogger(GetType().FullName).Info("NLog logging system loaded");

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
