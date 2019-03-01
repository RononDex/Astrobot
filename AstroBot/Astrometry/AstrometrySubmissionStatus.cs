using System;

namespace AstroBot.Astrometry
{

    public class AstrometrySubmissionStatus
    {
        public int? JobID { get; set; }
        public string SubmissionID { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? FinishTime { get; set; }

        public AstrometrySubmissionState State { get; set; }
    }
}