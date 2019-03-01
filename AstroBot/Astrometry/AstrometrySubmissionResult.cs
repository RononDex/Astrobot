using System.Collections.Generic;

namespace AstroBot.Astrometry
{
    public class AstrometrySubmissionResult
    {
        public AstrometrySubmissionCalibrationData CalibrationData { get; set; }

        public List<string> MachineTags { get; set; } = new List<string>();

        public List<string> Tags { get; set; } = new List<string>();

        public string FileName { get; set; }

        public List<string> ObjectsInfField { get; set; } = new List<string>();

        public string JobID { get; set; }

    }
}