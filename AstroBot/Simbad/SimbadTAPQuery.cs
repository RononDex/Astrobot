namespace AstroBot.Simbad
{
    /// <summary>
    /// A query for the Simbad API
    /// </summary>
    public class SimbadTAPQuery
    {
        /// <summary>
        /// The ADQL query
        /// </summary>
        /// <value></value>
        public string Query { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="query">The ADQL query</param>
        public SimbadTAPQuery(string query)
        {
            Query = query;
        }
    }
}