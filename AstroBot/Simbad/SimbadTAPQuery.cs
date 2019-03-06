namespace AstroBot.Simbad
{
    /// <summary>
    /// A query for the Simbad API
    /// </summary>
    public class SimbadTAPQuery
    {
        public string Query { get; private set; }

        public SimbadTAPQuery(string query)
        {
            Query = query;
        }
    }
}