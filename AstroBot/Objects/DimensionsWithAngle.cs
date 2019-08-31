namespace AstroBot.Objects
{
    public class DimensionsWithAngle
    {
        /// <summary>
        /// Angular size major axis in ' (arcminuts)
        /// </summary>
        /// <value></value>
        public double MajorAxis { get; set; }

        /// <summary>
        /// Angular size in minor axis in ' (arcminutes)
        /// </summary>
        /// <value></value>
        public double MinorAxis { get; set; }

        /// <summary>
        /// The angle at which the major axis is rotated at
        /// </summary>
        /// <value></value>
        public double MajorAxisAngle { get; set; }

        public override string ToString()
        {
            return $"{MajorAxis}' x {MinorAxis}' at an angle of {MajorAxisAngle}°";
        }
    }
}