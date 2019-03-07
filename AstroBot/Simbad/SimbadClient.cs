using System.Linq;
using System.IO;
using AstroBot.Objects;
using System.Collections.Generic;
using System.Net.Http;

namespace AstroBot.Simbad
{
    /// <summary>
    /// A client for the simbad website API
    /// </summary>
    public class SimbadClient
    {
        const string TAP_QUERY_ENDPOINT = "http://simbad.u-strasbg.fr/simbad/sim-tap/sync";

        /// <summary>
        /// Query SIMBAD with the given TAP query
        /// </summary>
        /// <param name="simbadTAPQuery"></param>
        /// <returns></returns>
        public SimbadTAPQueryResult QuerySimbad(SimbadTAPQuery simbadTAPQuery)
        {
            var client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "request", "doQuery" },
                { "lang", "adql" },
                { "format", "text" },
                { "query", simbadTAPQuery.Query }
            };

            var content = new FormUrlEncodedContent(values);
            var response = client.PostAsync(TAP_QUERY_ENDPOINT, content).Result;
            var text = response.Content.ReadAsStringAsync().Result;

            return new SimbadTAPQueryResult(text);
        }

        /// <summary>
        /// Tries to find an object of the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AstronomicalObject FindObjectByName(string name)
        {
            var query = File.ReadAllText("Simbad/Queries/FindByName.adql");
            query = query.Replace("{{name}}", name);
            var result = QuerySimbad(new SimbadTAPQuery(query));
            return result.AstronomicalObjects.FirstOrDefault();
        }
    }
}