using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using AstroBot.Objects.Config;

namespace AstroBot.GeoLocation
{
    /// <summary>
    /// Used to determine Lat/Lng coordinates for places on earth
    /// </summary>
    static class GeoLocation
    {
        /// <summary>
        /// Gets the lat/lng coordinates for a given place from google api
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static Objects.LatLngLocation FindLocation(string address)
        {
            Log<DiscordAstroBot>.Info($"Requesting GeoLocation for {address}");

            var settings       = Globals.ServiceProvider.GetService(typeof(BotSettings)) as BotSettings;
            var geoLocationKey = File.ReadAllText(settings.GoogleGeoLocationTokenPath);
            var webRequest     = WebRequest.Create(string.Format("https://maps.googleapis.com/maps/api/geocode/json?address={0}&key={1}", WebUtility.UrlEncode(address), geoLocationKey));
            var response       = (HttpWebResponse)webRequest.GetResponse();

            string text;
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();
            }

            dynamic result = JsonConvert.DeserializeObject(text);
            if (result.results.Count == 0)
                return null;

            var hit = result.results[0];
            var location =  new Objects.LatLngLocation()
            {
                Name = hit.formatted_address,
                Lat  = Convert.ToSingle(hit.geometry.location.lat),
                Lng  = Convert.ToSingle(hit.geometry.location.lng)
            };
            
            return location;
        }
    }
}