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
        public float RA { get; set; }
        public float DEC { get; set; }
    }
}