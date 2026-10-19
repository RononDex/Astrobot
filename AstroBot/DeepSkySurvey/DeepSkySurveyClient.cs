using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace AstroBot.DeepSkySurvey
{
    public static class DeepSkySurveyClient
    {
        private const string ServiceURL = "http://archive.eso.org/dss/dss/image";

        /// <summary>
        /// Gets an image from the DSS
        /// </summary>
        /// <param name="ra">RA coordinates of target</param>
        /// <param name="dec">DEC coordinates of target</param>
        /// <param name="size">Size of image in arcminutes, default: 85</param>
        /// <param name="mimetype">mime type of image to download, default: download-gif</param>
        /// <param name="catalogue">the catalogue to query, default: DSS2</param>
        /// <returns></returns>
        public static async Task<byte[]?> GetImageAsync(float ra, float dec, string size = "60", string mimetype = "download-gif", string catalogue = "DSS2-red")
        {
            var data = await Globals.HttpClient.GetByteArrayAsync($"{ServiceURL}?ra={HttpUtility.UrlEncode(ra.ToString())}&dec={HttpUtility.UrlEncode(dec.ToString())}&x={HttpUtility.UrlEncode(size)}&y={HttpUtility.UrlEncode(size)}&mime-type={HttpUtility.UrlEncode(mimetype)}&Sky-Survey={HttpUtility.UrlEncode(catalogue)}&equinox=J2000&statsmode=VO").ConfigureAwait(false);

            using (var stream = new MemoryStream(data))
            {
                var newImage = Utilities.ImageUtility.MakeGrayscaleFromRGB(stream);
                if (newImage == null)
                    return null;

                newImage.Position = 0;

                var buffer = new byte[16 * 1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = newImage.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    return ms.ToArray();
                }
            }
        }

        public static async Task<byte[]?> GetColorImageAsync(float ra, float dec, string size = "60", string mimetype = "download-gif")
        {
            // Download both plates concurrently
            var redTask = Globals.HttpClient.GetByteArrayAsync($"{ServiceURL}?ra={HttpUtility.UrlEncode(ra.ToString())}&dec={HttpUtility.UrlEncode(dec.ToString())}&x={HttpUtility.UrlEncode(size)}&y={HttpUtility.UrlEncode(size)}&mime-type={HttpUtility.UrlEncode(mimetype)}&Sky-Survey=DSS2-red&equinox=J2000&statsmode=VO");
            var blueTask = Globals.HttpClient.GetByteArrayAsync($"{ServiceURL}?ra={HttpUtility.UrlEncode(ra.ToString())}&dec={HttpUtility.UrlEncode(dec.ToString())}&x={HttpUtility.UrlEncode(size)}&y={HttpUtility.UrlEncode(size)}&mime-type={HttpUtility.UrlEncode(mimetype)}&Sky-Survey=DSS2-blue&equinox=J2000&statsmode=VO");
            var redData = await redTask.ConfigureAwait(false);
            var blueData = await blueTask.ConfigureAwait(false);

            using (var redStream = new MemoryStream(redData))
            {
                using (var blueStream = new MemoryStream(blueData))
                {
                    var newImage = Utilities.ImageUtility.MergeTogether(redStream, blueStream);
                    if (newImage == null)
                        return null;

                    newImage.Position = 0;

                    var buffer = new byte[16 * 1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int read;
                        while ((read = newImage.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }
                        return ms.ToArray();
                    }
                }
            }
        }
    }
}
