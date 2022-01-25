using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AstroBot.Simbad;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    public class DssCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {
        public string Description => "Queries image data from DSS (Deep sky survey)";

        public string[] ExampleCalls => new string[]
        {
            "how does M31 look like?"
        };

        public List<string> Regex => new List<string>
        {
            @"(how|what) does (?'ObjectName'.+?(?= look)) look like(\?)?"
        };

        public override string Name => "DeepSkySurvey";

        private const float DEFAULT_ANGULARSIZE_ARCMINUTES = 60;
        private const float FACTOR_ANGULARSIZE_ARCMINUTES = 1.7f;
        private const float MAX_ANGULARSIZE_ARCMINUTES = 120f;
        private const float MIN_ANGULARSIZE_ARCMINUTES = 30f;

        public async Task<bool> ExecuteRegexCommandAsync(ReceivedMessage receivedMessage, Match regexMatch)
        {
            if (regexMatch.Groups["ObjectName"].Success)
            {
                var astronomicalObjectName = regexMatch.Groups["ObjectName"].Value;
                var simbadClient = new SimbadClient();
                var objectResolvedBySimbad = simbadClient.FindObjectByName(astronomicalObjectName);

                if (objectResolvedBySimbad == null)
                {
                    await receivedMessage.Channel.SendMessageAsync($"No object with name {astronomicalObjectName} could be found on simbad!").ConfigureAwait(false);
                    return true;
                }

                if (objectResolvedBySimbad.RaDecCoordinate == null)
                {
                    await receivedMessage.Channel.SendMessageAsync($"The object was found, but no Ra / Dec coordinates are known for the object!").ConfigureAwait(false);
                    return true;
                }

                await receivedMessage.Channel.SendMessageAsync("Getting the image from the deep sky survey (DSS) server, one moment...\r\n(The larger the FOV, the longer it takes)").ConfigureAwait(false);
                var arcminutesSize =
                    Math.Max(objectResolvedBySimbad.AngularDimensions != null
                        ? Math.Min(objectResolvedBySimbad.AngularDimensions.MajorAxis * FACTOR_ANGULARSIZE_ARCMINUTES, MAX_ANGULARSIZE_ARCMINUTES)
                        : DEFAULT_ANGULARSIZE_ARCMINUTES,
                    MIN_ANGULARSIZE_ARCMINUTES);

                var image = DeepSkySurvey.DeepSkySurveyClient.GetColorImage(
                    Convert.ToSingle(objectResolvedBySimbad.RaDecCoordinate.RightAscension),
                    Convert.ToSingle(objectResolvedBySimbad.RaDecCoordinate.Declination),
                    arcminutesSize.ToString(CultureInfo.InvariantCulture));

                await receivedMessage.Channel.SendMessageAsync(new SendMessage(
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

            return true;
        }
    }
}