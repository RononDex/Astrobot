using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using AstroBot.Objects;
using AstroBot.Objects.AstronomicalObjects;

namespace AstroBot.Simbad
{
    /// <summary>
    /// A client for the simbad website API
    /// </summary>
    public class SimbadClient
    {
        /// <summary>
        /// Internal cache for lookup of long type names from short type names
        /// </summary>
        /// <value></value>
        internal static IReadOnlyDictionary<string, string> ShortTypeNameCache { get; private set; }

        const string TAP_QUERY_ENDPOINT = "http://simbad.u-strasbg.fr/simbad/sim-tap/sync";

        public SimbadClient()
        {
            // Make sure that the type cache is initialized
            if (ShortTypeNameCache == null)
            {
                InitShortTypeCache();
            }
        }

        /// <summary>
        /// Setup the internal short name to long name of object types
        /// </summary>
        private void InitShortTypeCache()
        {
            var query = File.ReadAllText("Simbad/Queries/LoadAllTypes.adql");

            var result = QuerySimbad(new SimbadTAPQuery(query));

            var cache = new Dictionary<string, string>();

            foreach (var type in result.ResultDataSet)
            {
                if (!cache.ContainsKey((string)type["ShortName"]))
                {
                    cache.Add((string)type["ShortName"], (string)type["LongName"]);
                }
            }

            ShortTypeNameCache = cache;
        }

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
                { "format", "json" },
                { "query", simbadTAPQuery.Query }
            };

            var content = new FormUrlEncodedContent(values);
            var response = client.PostAsync(TAP_QUERY_ENDPOINT
                                            + "?"
                                            + string.Join('&', values.Select(k => HttpUtility.UrlEncode(k.Key) + "=" + HttpUtility.UrlEncode(k.Value))), content).Result;
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
            return result.ToAstronomicalObjects().FirstOrDefault();
        }

        /// <summary>
        /// Tries to find an object of the given name
        /// </summary>
        /// <param name="raDecCoordinate"></param>
        /// <param name="radiusInDegrees"></param>
        /// <returns></returns>
        public IReadOnlyList<AstronomicalObject> QueryAround(RaDecCoordinate raDecCoordinate, float radiusInDegrees)
        {
            var query = File.ReadAllText("Simbad/Queries/QueryAroundRaDecCoordinates.adql");
            query = query
                .Replace("{{RACenter}}", Convert.ToString(raDecCoordinate.RightAscension, CultureInfo.InvariantCulture))
                .Replace("{{DECCenter}}", Convert.ToString(raDecCoordinate.Declination, CultureInfo.InvariantCulture))
                .Replace("{{RadiusInDegrees}}", Convert.ToString(radiusInDegrees, CultureInfo.InvariantCulture));

            var result = QuerySimbad(new SimbadTAPQuery(query));
            return result.ToAstronomicalObjects();
        }
    }
}
