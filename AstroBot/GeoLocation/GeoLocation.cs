using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace AstroBot.GeoLocation
{
    /// <summary>
    /// Used to determine Lat/Lng coordinates for places on earth
    /// </summary>
    internal static class GeoLocation
    {
        private static string? _googleApiKey;

        /// <summary>
        /// Keep the Api key in memory, to reduce I/O and increase performance
        /// </summary>
        /// <value></value>
        private static string GoogleApiKey
        {
            get
            {
                // If Api Key was not loaded yet, read it from the settings file
                if (string.IsNullOrEmpty(_googleApiKey))
                {
                    _googleApiKey = File.ReadAllText(Globals.BotSettings.GoogleGeoLocationTokenPath);
                }

                return _googleApiKey;
            }
        }


        /// <summary>
        /// Gets the lat/lng coordinates for a given place from google api
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static Objects.LatLngLocation? FindLocation(string address)
        {
            Log<AstroBotController>.Info($"Requesting GeoLocation for {address}");

            var webRequest = WebRequest.Create($"https://maps.googleapis.com/maps/api/geocode/json?address={WebUtility.UrlEncode(address)}&key={GoogleApiKey}");
            var response = (HttpWebResponse)webRequest.GetResponse();

            string text;
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                text = sr.ReadToEnd();
            }

            dynamic? result = JsonConvert.DeserializeObject(text);
            if (result!.results.Count == 0)
                return null;

            var hit = result.results[0];
            var location = new Objects.LatLngLocation()
            {
                Name = hit.formatted_address,
                Lat = Convert.ToSingle(hit.geometry.location.lat),
                Lng = Convert.ToSingle(hit.geometry.location.lng)
            };

            return location;
        }
    }
}
