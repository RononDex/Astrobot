namespace AstroBot.Objects
{
    /// <summary>
    /// Represents a Lat / Lng location
    /// </summary>
    public class LatLngLocation
    {
        /// <summary>
        /// Name of the location
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// The latitude component of the coordinate
        /// </summary>
        /// <value></value>
        public float Lat { get; set; }

        /// <summary>
        /// The longitude component of the coordinate
        /// </summary>
        /// <value></value>
        public float Lng { get; set; }

        /// <summary>
        /// Override default ToString()
        /// </summary>
        public override string ToString()
        {
            return $"Lat: {this.Lat}  Lng: {this.Lng}";
        }
    }
}