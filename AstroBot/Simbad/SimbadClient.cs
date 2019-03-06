using System.IO;
using System.Net;
using System.Web;

namespace AstroBot.Simbad
{
    /// <summary>
    /// A client for the simbad website API
    /// </summary>
    public class SimbadClient
    {
        const string TAP_QUERY_ENDPOINT = "http://simbad.u-strasbg.fr/simbad/sim-tap/sync?request=doQuery&lang=en&format=text&query={{QUERY}};";

        /// <summary>
        /// Query SIMBAD with the given TAP query
        /// </summary>
        /// <param name="simbadTAPQuery"></param>
        /// <returns></returns>
        public SimbadTAPQueryResult QuerySimbad(SimbadTAPQuery simbadTAPQuery)
        {
            var requestUrl = TAP_QUERY_ENDPOINT.Replace("{{QUERY}}", HttpUtility.UrlEncode(simbadTAPQuery.Query));

            var request = WebRequest.Create(requestUrl);
            var response = request.GetResponse();

            var responseStream = response.GetResponseStream();
            var streamReader = new StreamReader(responseStream);

            var textContent = streamReader.ReadToEnd();

            return new SimbadTAPQueryResult(textContent);
        }
    }
}