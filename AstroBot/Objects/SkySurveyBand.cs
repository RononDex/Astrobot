namespace AstroBot.Objects
{
    public enum SkySurveyBand
    {
        Color,
        Red,
        Infrared,
        Halpha,
        Oiii,
        Sii,

        /// <summary>
        /// [OIII], H-alpha and [SII] combined into one color image
        /// </summary>
        Narrowband,
    }
}
