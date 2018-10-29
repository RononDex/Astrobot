namespace AstroBot.Objects.Config
{
    /// <summary>
    /// Bot wide settings (like API tokens, ...)
    /// </summary>
    class BotSettings
    {
        /// <summary>
        /// Path to the file containing the token to access googles GeoLocation API
        /// </summary>
        /// <value></value>
        public string GoogleGeoLocationTokenPath { get; set; }
    }
}