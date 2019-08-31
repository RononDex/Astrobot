using System;
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
            return Properties.ContainsKey(property) ? (T)Convert.ChangeType(this[property], typeof(T)) : default;
        }

        /// <summary>
        /// Id used to identify the object on simbad
        /// </summary>
        public int SimbadId => Get<int>("SimbadId");

        /// <summary>
        /// The main name of the object
        /// </summary>
        /// <returns></returns>
        public string Name => Get<string>("Name");

        /// <summary>
        /// The type of the object
        /// </summary>
        /// <returns></returns>
        public string Type => Get<string>("Type");

        /// <summary>
        /// A short code describing the type
        /// </summary>
        /// <returns></returns>
        public string ShortType => Get<string>("TypeShort");

        /// <summary>
        /// Other types of the object
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        public List<string> OtherTypes => Properties.ContainsKey("OtherTypes")
            ? Get<string>("OtherTypes")?.Split('|').ToList()
            : null;

        /// <summary>
        /// A list of alternative names for this object
        /// </summary>
        /// <returns></returns>
        public List<string> OtherNames => Properties.ContainsKey("OtherNames")
            ? Get<string>("OtherNames")?.Split('|').ToList()
            : null;

        /// <summary>
        /// Relative velocity measurement of the object
        /// </summary>
        /// <returns></returns>
        public MeasurementWithError RelativeVelocity => Properties.ContainsKey("RelativeVelocity") ?
            new MeasurementWithError
            {
                Value = Get<double>("RelativeVelocity"),
                Unit = Get<string>("RelativeVelocityType") == "v" ? "km/s" : "",
                Error = Properties.ContainsKey("RelativeVelocityError") ?
                    Get<double>("RelativeVelocityError") :
                    new double?()
            }
            : null;

        public MeasurementWithError MeasuredDistance => Properties.ContainsKey("Distance")
            ? new MeasurementWithError
            {
                Value = Get<double>("Distance"),
                Unit = Get<string>("DistanceUnit"),
                Error = Get<double>("DistanceMinusErr")
            }
            : null;

        /// <summary>
        /// Angular dimensions of the object
        /// </summary>
        /// <returns></returns>
        public DimensionsWithAngle AngularDimensions => Properties.ContainsKey("MajorAxisDimension") ?
            new DimensionsWithAngle
            {
                MajorAxis = Get<double>("MajorAxisDimension"),
                MinorAxis = Get<double>("MinorAxisDimension"),
                MajorAxisAngle = Get<double>("MajorAxisAngle")
            }
            : null;

        /// <summary>
        /// Every astronomical object should have RA and DEC coordinates
        /// </summary>
        /// <returns></returns>
        public RaDecCoordinate RaDecCoordinate => Properties.ContainsKey("Ra") && Properties.ContainsKey("Dec") ?
            new RaDecCoordinate(rightAscension: Get<double>("Ra"), declination: Get<double>("Dec")) :
            null;

        /// <summary>
        /// The morphological type of a galaxy (null if not a galaxy)
        /// </summary>
        public string MorphologicalType => Properties.ContainsKey("MorphologicalType")
            ? Get<string>("MorphologicalType")
            : null;

        /// <summary>
        /// A list of known fluxes of the object
        /// </summary>
        public IList<Flux> Fluxes => Flux.FluxRangesLookup
            .Where(range => Properties.ContainsKey($"Flux{range.Key.ToString()}"))
            .Select(range => new Flux { FluxType = range.Key, Value = Get<float>($"Flux{range.Key.ToString()}") })
            .ToList();

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