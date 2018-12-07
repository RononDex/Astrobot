using System;
using System.IO;
using System.Net;
using System.Net.Http;
using AstroBot.Objects;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.DiscordWrapper.Objects;

namespace AstroBot.Weather
{
    public static class Clearoutside
    {
        
        public const string FORECAST_LARGE_REQUESTURI = "https://clearoutside.com/forecast_image_large/{0:0.00}/{1:0.00}/forecast.png";

        /// <summary>
        /// Gets a weather forecast as an image
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static Attachement GetWeatherForecast(LatLngLocation location)
        {
            #region PRECONDITION

            if (location == null)
                throw new ArgumentNullException("location can not be null!");

            #endregion

            var attachement = new SendAttachement();
            attachement.Name = "forecast.png";

            var request = WebRequest.Create(string.Format(FORECAST_LARGE_REQUESTURI, location.Lat, location.Lng ));
            var result = request.GetResponseAsync().Result;
            var response = result.GetResponseStream();

            using (var ms = new MemoryStream())
            {
                response.CopyTo(ms);
                var imageData =  ms.ToArray();
                attachement.Content = imageData;
            }

            return attachement;
        }
    }
}