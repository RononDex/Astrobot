using System;
using System.Threading.Tasks;
using AstroBot.Objects;
using AwesomeChatBot.ApiWrapper;

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
        public static async Task<Attachment> GetWeatherForecastAsync(LatLngLocation location)
        {
            #region PRECONDITION

            if (location == null)
                throw new ArgumentNullException("location can not be null!");

            #endregion

            var attachment = new SendAttachment();
            attachment.Name = "forecast.png";
            attachment.Content = await Globals.HttpClient.GetByteArrayAsync(string.Format(FORECAST_LARGE_REQUESTURI, location.Lat, location.Lng)).ConfigureAwait(false);

            return attachment;
        }
    }
}
