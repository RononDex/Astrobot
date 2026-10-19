using System;
using System.Threading.Tasks;
using AstroBot.Objects;

namespace AstroBot.SkySurveys
{
    public record SkySurveyImage(byte[] Image, SkySurvey Survey);

    /// <summary>
    /// Decides which sky survey to query for a band and gets the image from it
    /// </summary>
    public static class SkySurveyService
    {
        private const double NSNS_MIN_DECLINATION = -15;
        private const int NSNS_IMAGE_WIDTH = 1000;

        // ponytail: ~1" per pixel matches the DSS2 plates, capped since CDS takes about a minute for 4000px
        private const int DSS_MAX_IMAGE_WIDTH = 3000;

        public static bool IsNarrowband(SkySurveyBand band) =>
            band is SkySurveyBand.Halpha or SkySurveyBand.Oiii or SkySurveyBand.Sii or SkySurveyBand.Narrowband;

        /// <summary>
        /// NSNS is only used for the narrowband images, DSS2 has the better resolution for everything else
        /// </summary>
        public static SkySurvey GetSkySurvey(SkySurveyBand band, double declination) =>
            IsNarrowband(band) && declination >= NSNS_MIN_DECLINATION ? SkySurvey.Nsns : SkySurvey.Dss;

        /// <summary>
        /// Gets the image, narrowband bands fall back to the DSS2 red plates if NSNS has no image.
        /// Returns null if no survey has an image of the region
        /// </summary>
        public static async Task<SkySurveyImage?> GetImageAsync(SkySurveyBand band, RaDecCoordinate raDec, double arcminutesSize)
        {
            if (GetSkySurvey(band, raDec.Declination) == SkySurvey.Nsns)
            {
                var nsnsImage = await Hips2FitsClient.GetImageAsync($"simg.de/P/NSNS/DR0_2/{GetNsnsHips(band)}", raDec, arcminutesSize, NSNS_IMAGE_WIDTH).ConfigureAwait(false);
                if (nsnsImage != null)
                    return new SkySurveyImage(nsnsImage, SkySurvey.Nsns);
            }

            var dssImage = band switch
            {
                // CDS reprojects the red and blue plates onto the same grid, so the colors line up
                SkySurveyBand.Color => await Hips2FitsClient.GetImageAsync("CDS/P/DSS2/color", raDec, arcminutesSize, (int)Math.Min(arcminutesSize * 60, DSS_MAX_IMAGE_WIDTH)).ConfigureAwait(false),
                SkySurveyBand.Infrared => await DeepSkySurveyClient.GetImageAsync(raDec, arcminutesSize, "DSS2-infrared").ConfigureAwait(false),
                _ => await DeepSkySurveyClient.GetImageAsync(raDec, arcminutesSize, "DSS2-red").ConfigureAwait(false),
            };
            return dssImage != null ? new SkySurveyImage(dssImage, SkySurvey.Dss) : null;
        }

        private static string GetNsnsHips(SkySurveyBand band) =>
            band switch
            {
                SkySurveyBand.Halpha => "halpha8",
                SkySurveyBand.Oiii => "oiii8",
                SkySurveyBand.Sii => "sii8",
                _ => "ohs8",
            };
    }
}
