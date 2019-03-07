namespace AstroBot.Objects
{
    public class RaDecCoordinate
    {
        public RaDecCoordinate(double rightAscension, double declination)
        {
            this.RightAscension = rightAscension;
            this.Declination = declination;

        }

        /// <summary>
        /// Right ascension component of the coordinate
        /// </summary>
        /// <value></value>
        public double RightAscension { get; set; }

        /// <summary>
        /// The declination component of the coordinate
        /// </summary>
        /// <value></value>
        public double Declination { get; set; }
    }
}