using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AstroBot.Objects;
using AstroBot.Objects.AstronomicalObjects;
using AstroBot.Simbad;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    public class SkySurveyCommand
        : AwesomeChatBot.Commands.Command,
            IRegexCommand,
            ISlashCommand,
            ICommandDescription
    {
        public string Description =>
            "Queries image data from DSS2 (Digitized Sky Survey), narrowband images from NSNS (Northern Sky Narrowband Survey) if available";

        public override string Name => "SkySurvey";

        public List<string> Regex =>
            new List<string>
            {
                @"(how|what) does (?'ObjectNameColor'.+?(?= look)) look like in col(o|ou)r(\?)?",
                @"(how|what) does (?'ObjectNameIr'.+?(?= look)) look like in (ir|infrared)(\?)?",
                @"(how|what) does (?'ObjectNameNarrowband'.+?(?= look)) look (like )?in (?'Band'narrowband|h-?alpha|oiii|sii)(\?)?",
                @"(how|what) does (?'ObjectName'.+?(?= look)) look like(\?)?",
            };

        public string[] ExampleCalls =>
            new string[] { "how does M31 look like?", "how does M42 look in h-alpha?" };

        public string SlashName => "skysurvey";

        public string SlashDescription =>
            "Queries image data from DSS2, narrowband images from NSNS if available";

        public IReadOnlyList<SlashCommandOption> Options =>
            new[]
            {
                new SlashCommandOption
                {
                    Name = "object",
                    Description = "Name of the astronomical object, e.g. M31",
                    Type = SlashOptionType.String,
                    Required = true,
                },
                new SlashCommandOption
                {
                    Name = "band",
                    Description =
                        "Which image to get (default: color)",
                    Type = SlashOptionType.String,
                    Required = false,
                    Choices = new[]
                    {
                        new SlashCommandChoice<string> { Name = "Color (DSS)", Value = "color" },
                        new SlashCommandChoice<string> { Name = "Red (DSS)", Value = "red" },
                        new SlashCommandChoice<string>
                        {
                            Name = "Infrared (DSS)",
                            Value = "infrared",
                        },
                        new SlashCommandChoice<string>
                        {
                            Name = "H-alpha (NSNS)",
                            Value = "halpha",
                        },
                        new SlashCommandChoice<string> { Name = "[OIII] (NSNS)", Value = "oiii" },
                        new SlashCommandChoice<string> { Name = "[SII] (NSNS)", Value = "sii" },
                        new SlashCommandChoice<string>
                        {
                            Name = "Narrowband [OIII], H-alpha, [SII] (NSNS)",
                            Value = "narrowband",
                        },
                    },
                },
            };

        private const float DEFAULT_ANGULARSIZE_ARCMINUTES = 60;
        private const float FACTOR_ANGULARSIZE_ARCMINUTES = 1.7f;
        private const float MAX_ANGULARSIZE_ARCMINUTES = 120f;
        private const float MIN_ANGULARSIZE_ARCMINUTES = 30f;

        // NSNS and the DSS2 color image are published as HiPS tiles, CDS hips2fits cuts images out of them
        private const string HIPS2FITS_SERVICE_URL =
            "https://alasky.cds.unistra.fr/hips-image-services/hips2fits";
        private const int NSNS_IMAGE_WIDTH = 1000;

        // ponytail: ~1" per pixel matches the DSS2 plates, capped since CDS takes about a minute for 4000px
        private const int DSS_MAX_IMAGE_WIDTH = 3000;
        private const double NSNS_MIN_DECLINATION = -15;

        public Task<bool> ExecuteRegexCommandAsync(
            ReceivedMessage receivedMessage,
            Match regexMatch
        )
        {
            var options = new Dictionary<string, object>();
            if (regexMatch.Groups["ObjectNameColor"].Success)
            {
                options["object"] = regexMatch.Groups["ObjectNameColor"].Value;
                options["band"] = "color";
            }
            else if (regexMatch.Groups["ObjectNameIr"].Success)
            {
                options["object"] = regexMatch.Groups["ObjectNameIr"].Value;
                options["band"] = "infrared";
            }
            else if (regexMatch.Groups["ObjectNameNarrowband"].Success)
            {
                var band = regexMatch.Groups["Band"].Value.ToLowerInvariant().Replace("-", "");
                options["object"] = regexMatch.Groups["ObjectNameNarrowband"].Value;
                options["band"] = band;
            }
            else
            {
                options["object"] = regexMatch.Groups["ObjectName"].Value;
            }

            return RegexSlashCommandInvocation.ExecuteAsync(this, receivedMessage, options);
        }

        public async Task ExecuteSlashCommandAsync(SlashCommandInvocation invocation)
        {
            var band = invocation.GetOption<string>("band") ?? "color";
            var simbadClient = new SimbadClient();
            var objectResolvedBySimbad = await simbadClient
                .FindObjectByNameAsync(invocation.GetOption<string>("object"))
                .ConfigureAwait(false);

            if (objectResolvedBySimbad == null)
            {
                await invocation
                    .RespondAsync($"No object with that name could be found on simbad!")
                    .ConfigureAwait(false);
                return;
            }

            if (objectResolvedBySimbad.RaDecCoordinate == null)
            {
                await invocation
                    .RespondAsync(
                        $"The object was found, but no Ra / Dec coordinates are known for the object!"
                    )
                    .ConfigureAwait(false);
                return;
            }

            await invocation
                .RespondAsync(
                    "Getting the image from the sky survey server, one moment...\r\n(The larger the FOV, the longer it takes)"
                )
                .ConfigureAwait(false);
            var arcminutesSize = GetImageSizeArcminutes(objectResolvedBySimbad);

            var survey = GetSkySurvey(band, objectResolvedBySimbad.RaDecCoordinate.Declination);
            var raDec = objectResolvedBySimbad.RaDecCoordinate;
            var image =
                survey == SkySurvey.Nsns
                    ? await GetHips2FitsImageAsync(
                            $"simg.de/P/NSNS/DR0_2/{GetNsnsHips(band)}",
                            raDec,
                            arcminutesSize,
                            NSNS_IMAGE_WIDTH
                        )
                        .ConfigureAwait(false)
                    : null;

            // Also covers NSNS failing to deliver the image
            if (image == null)
            {
                survey = SkySurvey.Dss;
                if (band is "halpha" or "oiii" or "sii" or "narrowband")
                    await invocation
                        .RespondAsync(
                            $"No NSNS image is available for this object, getting the red image from DSS instead"
                        )
                        .ConfigureAwait(false);

                var ra = Convert.ToSingle(raDec.RightAscension);
                var dec = Convert.ToSingle(raDec.Declination);
                var size = arcminutesSize.ToString(CultureInfo.InvariantCulture);
                image = band switch
                {
                    // CDS reprojects the red and blue plates onto the same grid, so the colors line up
                    "color" => await GetHips2FitsImageAsync(
                            "CDS/P/DSS2/color",
                            raDec,
                            arcminutesSize,
                            (int)Math.Min(arcminutesSize * 60, DSS_MAX_IMAGE_WIDTH)
                        )
                        .ConfigureAwait(false),
                    "infrared" => await DeepSkySurvey
                        .DeepSkySurveyClient.GetImageAsync(
                            ra,
                            dec,
                            size,
                            "download-gif",
                            "DSS2-infrared"
                        )
                        .ConfigureAwait(false),
                    _ => await DeepSkySurvey
                        .DeepSkySurveyClient.GetImageAsync(
                            ra,
                            dec,
                            size,
                            "download-gif",
                            "DSS2-red"
                        )
                        .ConfigureAwait(false),
                };
            }

            if (image != null)
            {
                var source = survey switch
                {
                    SkySurvey.Nsns => "NSNS (CC BY-NC-SA)",
                    _ => "DSS2",
                };
                await invocation
                    .RespondAsync(
                        new SendMessage(
                            content: $"Image size: {Math.Round(arcminutesSize, 3)}' x {Math.Round(arcminutesSize, 3)}'\r\nData: {source}",
                            new List<Attachment>
                            {
                                new SendAttachment
                                {
                                    Name = $"{objectResolvedBySimbad.Name}.jpg",
                                    Content = image,
                                },
                            }
                        )
                    )
                    .ConfigureAwait(false);
            }
            else
            {
                await invocation
                    .RespondAsync(
                        "I am sorry, it seems the requested object has not yet been imaged by dss-2"
                    )
                    .ConfigureAwait(false);
            }
        }

        private static double GetImageSizeArcminutes(AstronomicalObject astronomicalObject) =>
            Math.Max(
                astronomicalObject.AngularDimensions != null
                    ? Math.Min(
                        astronomicalObject.AngularDimensions.MajorAxis
                            * FACTOR_ANGULARSIZE_ARCMINUTES,
                        MAX_ANGULARSIZE_ARCMINUTES
                    )
                    : DEFAULT_ANGULARSIZE_ARCMINUTES,
                MIN_ANGULARSIZE_ARCMINUTES
            );

        /// <summary>
        /// NSNS is only used for the narrowband images, DSS2 has the better resolution for everything else
        /// </summary>
        public static SkySurvey GetSkySurvey(string? band, double declination) =>
            band is "halpha" or "oiii" or "sii" or "narrowband"
            && declination >= NSNS_MIN_DECLINATION
                ? SkySurvey.Nsns
                : SkySurvey.Dss;

        private static string GetNsnsHips(string? band) =>
            band switch
            {
                "halpha" => "halpha8",
                "oiii" => "oiii8",
                "sii" => "sii8",
                _ => "ohs8",
            };

        /// <summary>
        /// Cuts a square jpg image out of a HiPS survey, returns null if it could not be retrieved
        /// </summary>
        private static async Task<byte[]?> GetHips2FitsImageAsync(
            string hips,
            RaDecCoordinate raDec,
            double arcminutesSize,
            int width
        )
        {
            try
            {
                return await Globals
                    .HttpClient.GetByteArrayAsync(
                        $"{HIPS2FITS_SERVICE_URL}?hips={HttpUtility.UrlEncode(hips)}&ra={raDec.RightAscension.ToString(CultureInfo.InvariantCulture)}&dec={raDec.Declination.ToString(CultureInfo.InvariantCulture)}&fov={(arcminutesSize / 60).ToString(CultureInfo.InvariantCulture)}&width={width}&height={width}&projection=TAN&format=jpg"
                    )
                    .ConfigureAwait(false);
            }
            // TaskCanceledException is thrown when the request times out
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return null;
            }
        }
    }
}
