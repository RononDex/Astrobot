using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AstroBot.Objects;

namespace AstroBot.SkySurveys
{
    /// <summary>
    /// Gets single plate images from the ESO DSS server
    /// </summary>
    public static class DeepSkySurveyClient
    {
        private const string ServiceURL = "https://archive.eso.org/dss/dss/image";

        /// <summary>
        /// Gets a grayscale image from the DSS, returns null if the region has not been imaged or the request failed
        /// </summary>
        /// <param name="raDec">Coordinates of the target</param>
        /// <param name="arcminutesSize">Width and height of the image in arcminutes</param>
        /// <param name="catalogue">The plates to query, e.g. DSS2-red</param>
        public static async Task<byte[]?> GetImageAsync(RaDecCoordinate raDec, double arcminutesSize, string catalogue)
        {
            var size = HttpUtility.UrlEncode(arcminutesSize.ToString(CultureInfo.InvariantCulture));
            byte[] data;
            try
            {
                data = await Globals.HttpClient.GetByteArrayAsync($"{ServiceURL}?ra={raDec.RightAscension.ToString(CultureInfo.InvariantCulture)}&dec={raDec.Declination.ToString(CultureInfo.InvariantCulture)}&x={size}&y={size}&mime-type=download-gif&Sky-Survey={HttpUtility.UrlEncode(catalogue)}&equinox=J2000&statsmode=VO").ConfigureAwait(false);
            }
            // TaskCanceledException is thrown when the request times out
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return null;
            }

            using var stream = new MemoryStream(data);
            using var grayscaleImage = Utilities.ImageUtility.MakeGrayscaleFromRGB(stream);
            return grayscaleImage?.ToArray();
        }
    }
}
