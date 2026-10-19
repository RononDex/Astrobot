using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AstroBot.Objects;
using AstroBot.Objects.AstronomicalObjects;
using AstroBot.Simbad;
using AstroBot.SkySurveys;
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
                @"(how|what) does (?'ObjectName'.+?(?= look)) look (like )?in (?'Band'colou?r|ir|infrared|narrowband|h-?alpha|oiii|sii)(\?)?",
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

        public Task<bool> ExecuteRegexCommandAsync(
            ReceivedMessage receivedMessage,
            Match regexMatch
        )
        {
            return RegexSlashCommandInvocation.ExecuteAsync(
                this,
                receivedMessage,
                new Dictionary<string, object>
                {
                    ["object"] = regexMatch.Groups["ObjectName"].Value,
                    ["band"] = regexMatch.Groups["Band"].Value,
                }
            );
        }

        public async Task ExecuteSlashCommandAsync(SlashCommandInvocation invocation)
        {
            var band = ParseBand(invocation.GetOption<string>("band"));
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
            var image = await SkySurveyService
                .GetImageAsync(band, objectResolvedBySimbad.RaDecCoordinate, arcminutesSize)
                .ConfigureAwait(false);

            if (image == null)
            {
                await invocation
                    .RespondAsync(
                        "I am sorry, it seems the requested object has not yet been imaged by dss-2"
                    )
                    .ConfigureAwait(false);
                return;
            }

            var source = image.Survey switch
            {
                SkySurvey.Nsns => "NSNS (CC BY-NC-SA)",
                _ when SkySurveyService.IsNarrowband(band) =>
                    "DSS2 red, no NSNS image is available for this object",
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
                                Content = image.Image,
                            },
                        }
                    )
                )
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Parses the band of the slash option or the regex, e.g. "h-alpha" or "colour"
        /// </summary>
        public static SkySurveyBand ParseBand(string? band) =>
            band?.ToLowerInvariant().Replace("-", "") switch
            {
                null or "" or "colour" => SkySurveyBand.Color,
                "ir" => SkySurveyBand.Infrared,
                var name => Enum.Parse<SkySurveyBand>(name, ignoreCase: true),
            };

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
    }
}
