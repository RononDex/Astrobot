using System.Collections.Generic;

namespace AstroBot.Objects
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
    }
}