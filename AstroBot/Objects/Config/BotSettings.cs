namespace AstroBot.Objects.Config
{
    /// <summary>
    /// Bot wide settings (like API tokens, ...)
    /// </summary>
    public class BotSettings
    {
        /// <summary>
        /// Path to the file containing the token to access googles GeoLocation API
        /// </summary>
        /// <value></value>
        public string GoogleGeoLocationTokenPath { get; set; }

        /// <summary>
        /// The api key for nova.astrometry
        /// </summary>
        /// <value></value>
        public string NovaAstrometryApiKeyPath { get; set; }

        /// <summary>
        /// A list of all servers that are whitelisted to globally ban a user from bot usage
        /// </summary>
        /// <value></value>
        public string[] GlobalUserBanServerWhiteList { get; set; }
    }
}