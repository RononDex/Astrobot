namespace AstroBot.Simbad
{
    /// <summary>
    /// A client for the simbad website API
    /// </summary>
    public class SimbadClient
    {
        const string TAP_QUERY_ENDPOINT = "http://simbad.u-strasbg.fr/simbad/sim-tap/sync?request=doQuery&lang=en&format=text&query={{QUERY}};";
    }
}