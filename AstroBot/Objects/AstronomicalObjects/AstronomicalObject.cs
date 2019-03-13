using System.Linq;
using System.Collections.Generic;

namespace AstroBot.Objects.AstronomicalObjects
{
    /// <summary>
    /// Represents an astronomical object (galaxy, star, ...)
    /// </summary>
    public class AstronomicalObject
    {
        /// <summary>
        /// Internal storage of properties
        /// </summary>
        /// <value></value>
        private Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// public accessor for the properties
        /// </summary>
        public object this[string key] => Properties[key];

        /// <summary>
        ///
        /// </summary>
        /// <param name="properties">Properties of the object</param>
        public AstronomicalObject(Dictionary<string, object> properties)
        {
            Properties = properties;
        }

        /// <summary>
        /// Gets a property casted as the given type
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public T Get<T>(string property)
        {
            return (T)this[property];
        }

        /// <summary>
        /// The main name of the object
        /// </summary>
        /// <returns></returns>
        public string Name => Properties.ContainsKey("NAME") ? (string)this["NAME"] : null;

        /// <summary>
        /// The type of the object
        /// </summary>
        /// <returns></returns>
        public string Type => Properties.ContainsKey("TYPE") ? (string)this["TYPE"] : null;

        /// <summary>
        /// A short code describing the type
        /// </summary>
        /// <returns></returns>
        public string ShortType => Properties.ContainsKey("TYPESHORT") ? (string)this["TYPESHORT"] : null;

        /// <summary>
        /// A list of alternative names for this object
        /// </summary>
        /// <returns></returns>
        public List<string> OtherNames => Properties.ContainsKey("OTHERNAMES") ? ((string)this["OTHERNAMES"]).Split('|').ToList() : null;

        /// <summary>
        /// Relative velocity measurement of the object
        /// </summary>
        /// <returns></returns>
        public MeasurementWithError RelativeVelocity => Properties.ContainsKey("RELATIVEVELOCITY") ?
            new MeasurementWithError
            {
                Value = (double)this["RELATIVEVELOCITY"],
                Error = Properties.ContainsKey("RELATIVEVELOCITYERROR") ?
                    Get<double>("RELATIVEVELOCITYERROR") :
                    new double?()
            }
            : null;

        /// <summary>
        /// Angular dimensions of the object
        /// </summary>
        /// <returns></returns>
        public DimensionsWithAngle AngularDimensions => Properties.ContainsKey("MAJORAXISDIMENSION") ?
            new DimensionsWithAngle()
            {
                MajorAxis = Get<double>("MAJORAXISDIMENSION"),
                MinorAxis = Get<double>("MINORAXISDIMENSION"),
                MajorAxisAngle = Get<double>("MAJORAXISANGLE")
            }
            : null;

        /// <summary>
        /// Every astronomical object should have RA and DEC coordinates
        /// </summary>
        /// <returns></returns>
        public RaDecCoordinate RaDecCoordinate => Properties.ContainsKey("RA") && Properties.ContainsKey("DEC") ?
            new RaDecCoordinate(rightAscension: (double)this["RA"], declination: (double)this["DEC"]) :
            null;

        /// <summary>
        /// Override ToString to return objects name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}