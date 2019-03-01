using AstroBot.Objects;

namespace AstroBot.Astrometry
{
    public class AstrometrySubmissionCalibrationData
    {
        public float Parity { get; set; }
        public float Orientation { get; set; }

        /// <summary>
        /// The scale of the image in arcseconds / pixel
        /// </summary>
        public float PixScale { get; set; }

        /// <summary>
        /// Radius of the image in degrees
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// The RA/DEC coordinates
        /// </summary>
        /// <value></value>
        public RaDecCoordinate Coordinates { get; set; }

        /// <summary>
        /// The url to the wcs file
        /// </summary>
        /// <value></value>
        public string WCSFileUrl { get; set; }
    }
}