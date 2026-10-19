using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AstroBot.Objects;

namespace AstroBot.SkySurveys
{
    /// <summary>
    /// Cuts images out of HiPS surveys (e.g. NSNS, DSS2 color) using the CDS hips2fits service
    /// </summary>
    public static class Hips2FitsClient
    {
        private const string ServiceURL = "https://alasky.cds.unistra.fr/hips-image-services/hips2fits";

        /// <summary>
        /// Gets a square jpg image, returns null if it could not be retrieved
        /// </summary>
        /// <param name="hips">ID of the HiPS survey, e.g. CDS/P/DSS2/color</param>
        /// <param name="raDec">Coordinates of the image center</param>
        /// <param name="arcminutesSize">Width and height of the image in arcminutes</param>
        /// <param name="width">Width and height of the image in pixels</param>
        public static async Task<byte[]?> GetImageAsync(string hips, RaDecCoordinate raDec, double arcminutesSize, int width)
        {
            var fovDegrees = (arcminutesSize / 60).ToString(CultureInfo.InvariantCulture);
            try
            {
                return await Globals.HttpClient.GetByteArrayAsync($"{ServiceURL}?hips={HttpUtility.UrlEncode(hips)}&ra={raDec.RightAscension.ToString(CultureInfo.InvariantCulture)}&dec={raDec.Declination.ToString(CultureInfo.InvariantCulture)}&fov={fovDegrees}&width={width}&height={width}&projection=TAN&format=jpg").ConfigureAwait(false);
            }
            // TaskCanceledException is thrown when the request times out
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return null;
            }
        }
    }
}
