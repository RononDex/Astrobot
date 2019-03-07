namespace AstroBot.Objects
{
    public class RaDecCoordinate
    {
        public RaDecCoordinate(double rightAscension, double declination)
        {
            RightAscension = rightAscension;
            Declination = declination;
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

        /// <summary>
        /// Return a more meaningful string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"RA: {RightAscension}, DEC: {Declination}";
        }
    }
}