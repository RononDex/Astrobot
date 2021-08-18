using Microsoft.Extensions.Logging;

namespace AstroBot
{
    /// <summary>
    /// A logger extension class, allowing for easy access of generic loggers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Log<T>
    {
        /// <summary>
        /// Log as info
        /// </summary>
        public static void Info(string message)
        {
            Globals.LoggerFactory!.CreateLogger(typeof(T).FullName).LogInformation(message);
        }

        /// <summary>
        /// Logs and error message
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            Globals.LoggerFactory!.CreateLogger(typeof(T).FullName).LogError(message);
        }
    }
}