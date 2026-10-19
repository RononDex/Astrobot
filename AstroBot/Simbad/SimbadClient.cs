using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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

        const string TAP_QUERY_ENDPOINT = "https://simbad.u-strasbg.fr/simbad/sim-tap/sync";

        private static Task? _shortTypeCacheTask;

        /// <summary>
        /// Loads the type cache on first use. A failed load is retried on the next call.
        /// Concurrent first calls may load it twice, which is harmless.
        /// </summary>
        private static Task EnsureShortTypeCacheAsync()
        {
            var task = _shortTypeCacheTask;
            if (task == null || task.IsFaulted || task.IsCanceled)
            {
                task = InitShortTypeCacheAsync();
                _shortTypeCacheTask = task;
            }
            return task;
        }

        /// <summary>
        /// Setup the internal short name to long name of object types
        /// </summary>
        private static async Task InitShortTypeCacheAsync()
        {
            var query = File.ReadAllText("Simbad/Queries/LoadAllTypes.adql");

            var result = await SendQueryAsync(new SimbadTAPQuery(query)).ConfigureAwait(false);

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
        public async Task<SimbadTAPQueryResult> QuerySimbadAsync(SimbadTAPQuery simbadTAPQuery)
        {
            await EnsureShortTypeCacheAsync().ConfigureAwait(false);
            return await SendQueryAsync(simbadTAPQuery).ConfigureAwait(false);
        }

        private static async Task<SimbadTAPQueryResult> SendQueryAsync(SimbadTAPQuery simbadTAPQuery)
        {
            var values = new Dictionary<string, string>
            {
                { "request", "doQuery" },
                { "lang", "adql" },
                { "format", "json" },
                { "query", simbadTAPQuery.Query }
            };

            var content = new FormUrlEncodedContent(values);
            using var response = await Globals.HttpClient.PostAsync(TAP_QUERY_ENDPOINT
                                            + "?"
                                            + string.Join('&', values.Select(k => HttpUtility.UrlEncode(k.Key) + "=" + HttpUtility.UrlEncode(k.Value))), content).ConfigureAwait(false);
            var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return new SimbadTAPQueryResult(text);
        }

        /// <summary>
        /// Tries to find an object of the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<AstronomicalObject> FindObjectByNameAsync(string name)
        {
            var query = File.ReadAllText("Simbad/Queries/FindByName.adql");
            query = query.Replace("{{name}}", name);
            var result = await QuerySimbadAsync(new SimbadTAPQuery(query)).ConfigureAwait(false);
            return result.ToAstronomicalObjects().FirstOrDefault();
        }

        /// <summary>
        /// Tries to find an object of the given name
        /// </summary>
        /// <param name="raDecCoordinate"></param>
        /// <param name="radiusInDegrees"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<AstronomicalObject>> QueryAroundAsync(RaDecCoordinate raDecCoordinate, float radiusInDegrees)
        {
            var query = File.ReadAllText("Simbad/Queries/QueryAroundRaDecCoordinates.adql");
            query = query
                .Replace("{{RACenter}}", Convert.ToString(raDecCoordinate.RightAscension, CultureInfo.InvariantCulture))
                .Replace("{{DECCenter}}", Convert.ToString(raDecCoordinate.Declination, CultureInfo.InvariantCulture))
                .Replace("{{RadiusInDegrees}}", Convert.ToString(radiusInDegrees, CultureInfo.InvariantCulture));

            var result = await QuerySimbadAsync(new SimbadTAPQuery(query)).ConfigureAwait(false);
            return result.ToAstronomicalObjects();
        }
    }
}
