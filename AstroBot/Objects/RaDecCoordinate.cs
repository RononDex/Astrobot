namespace AstroBot.Objects
{
    public class RaDecCoordinate
    {
        public RaDecCoordinate(float rightAscension, float declination)
        {
            this.RightAscension = rightAscension;
            this.Declination = declination;

        }

        /// <summary>
        /// Right ascension component of the coordinate
        /// </summary>
        /// <value></value>
        public float RightAscension { get; set; }

        /// <summary>
        /// The declination component of the coordinate
        /// </summary>
        /// <value></value>
        public float Declination { get; set; }
    }
}