using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AstroBot.Simbad;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    public class DssCommand : AwesomeChatBot.Commands.Command, ISlashCommand, ICommandDescription
    {
        public string Description => "Queries image data from DSS (Deep sky survey)";

        public override string Name => "DeepSkySurvey";

        public string SlashName => "dss";

        public string SlashDescription => Description;

        public IReadOnlyList<SlashCommandOption> Options => new[]
        {
            new SlashCommandOption { Name = "object", Description = "Name of the astronomical object, e.g. M31", Type = SlashOptionType.String, Required = true },
            new SlashCommandOption
            {
                Name = "band", Description = "Which DSS image to get (default: red)", Type = SlashOptionType.String, Required = false,
                Choices = new[]
                {
                    new SlashCommandChoice<string> { Name = "Red", Value = "red" },
                    new SlashCommandChoice<string> { Name = "Color", Value = "color" },
                    new SlashCommandChoice<string> { Name = "Infrared", Value = "infrared" },
                }
            },
        };

        private const float DEFAULT_ANGULARSIZE_ARCMINUTES = 60;
        private const float FACTOR_ANGULARSIZE_ARCMINUTES = 1.7f;
        private const float MAX_ANGULARSIZE_ARCMINUTES = 120f;
        private const float MIN_ANGULARSIZE_ARCMINUTES = 30f;

        public async Task ExecuteSlashCommandAsync(SlashCommandInvocation invocation)
        {
            var band = invocation.GetOption<string>("band") ?? "red";
            var simbadClient = new SimbadClient();
            var objectResolvedBySimbad = await simbadClient.FindObjectByNameAsync(invocation.GetOption<string>("object")).ConfigureAwait(false);

            if (objectResolvedBySimbad == null)
            {
                await invocation.RespondAsync($"No object with that name could be found on simbad!").ConfigureAwait(false);
                return;
            }

            if (objectResolvedBySimbad.RaDecCoordinate == null)
            {
                await invocation.RespondAsync($"The object was found, but no Ra / Dec coordinates are known for the object!").ConfigureAwait(false);
                return;
            }

            await invocation.RespondAsync("Getting the image from the deep sky survey (DSS) server, one moment...\r\n(The larger the FOV, the longer it takes)").ConfigureAwait(false);
            var arcminutesSize =
                Math.Max(objectResolvedBySimbad.AngularDimensions != null
                    ? Math.Min(objectResolvedBySimbad.AngularDimensions.MajorAxis * FACTOR_ANGULARSIZE_ARCMINUTES, MAX_ANGULARSIZE_ARCMINUTES)
                    : DEFAULT_ANGULARSIZE_ARCMINUTES,
                MIN_ANGULARSIZE_ARCMINUTES);

            var ra = Convert.ToSingle(objectResolvedBySimbad.RaDecCoordinate.RightAscension);
            var dec = Convert.ToSingle(objectResolvedBySimbad.RaDecCoordinate.Declination);
            var size = arcminutesSize.ToString(CultureInfo.InvariantCulture);
            var image = band switch
            {
                "color" => await DeepSkySurvey.DeepSkySurveyClient.GetColorImageAsync(ra, dec, size).ConfigureAwait(false),
                "infrared" => await DeepSkySurvey.DeepSkySurveyClient.GetImageAsync(ra, dec, size, "download-gif", "DSS2-infrared").ConfigureAwait(false),
                _ => await DeepSkySurvey.DeepSkySurveyClient.GetImageAsync(ra, dec, size, "download-gif", "DSS2-red").ConfigureAwait(false),
            };

            if (image != null)
            {
                await invocation.RespondAsync(new SendMessage(
                    content: $"Image size: {Math.Round(arcminutesSize, 3)}' x {Math.Round(arcminutesSize, 3)}'",
                    new List<Attachment>
                    {
                        new SendAttachment
                        {
                            Name = $"{objectResolvedBySimbad.Name}.jpg",
                            Content = image
                        }
                    })).ConfigureAwait(false);
            }
            else
            {
                await invocation.RespondAsync("I am sorry, it seems the requested object has not yet been imaged by dss-2").ConfigureAwait(false);
            }
        }
    }
}
