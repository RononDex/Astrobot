using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AstroBot.Simbad;
using AstroBot.Objects.AstronomicalObjects;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;
using System;
using AstroBot.Utilities.UnitConverters;
using AstroBot.Objects;

namespace AstroBot.Commands
{
    public class SimbadCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {
        public string Description => "Access data from the SIMBAD database";

        public string[] ExampleCalls => new string[]
        {
            "what is M31?",
            "what do you know about M31?",
            // "how far away is M13?",
            // "fluxes of NGC6888?",
            // "how bright is M78?",
            // "how big is M31?"
        };

        public List<string> Regex => new List<string>()
        {
            @"what do you know about (?'AstroObject'.*\w)(\?)?",
            @"what is (?'AstroObject'.*\w)(\?)?"
        };

        public override string Name => "Simbad";

        public async Task<bool> ExecuteRegexCommand(ReceivedMessage receivedMessage, Match regexMatch)
        {
            var simbadClient = new SimbadClient();

            if (regexMatch.Groups["AstroObject"].Success)
            {
                var objectName = regexMatch.Groups["AstroObject"].Value;
                var foundObject = simbadClient.FindObjectByName(objectName);
                if (foundObject == null)
                {
                    await WriteObjectNotFoundAsync(receivedMessage, objectName);
                    return true;
                }

                await WriteObjectDetailsAsyncEmbeddedAsync(receivedMessage, foundObject);
            }
            return true;
        }

        private static Task WriteObjectDetailsAsyncEmbeddedAsync(ReceivedMessage receivedMessage, AstronomicalObject foundObject)
        {
            var embeddedMessage = new EmbeddedMessage
            {
                Title = foundObject.Name
            };

            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Type:", foundObject.Type, inline: true);
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Morphological Type:", foundObject.MorphologicalType, inline: true);

            var coordinates = foundObject.RaDecCoordinate != null
                ? $"RA: {Math.Round(foundObject.RaDecCoordinate.RightAscension, 5)}\r\nDEC: {Math.Round(foundObject.RaDecCoordinate.Declination, 5)}"
                : null;
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Coordinates:", coordinates, inline: true);
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Relative Velocity:", foundObject.RelativeVelocity?.ToString(), inline: true);

            var estimatedDistance = foundObject.MeasuredDistance != null
                ? $"{foundObject.MeasuredDistance}"
                : null;
            if (estimatedDistance != null)
            {
                var convertedDistance = AstronomicalDistanceUnitConverter.ConvertMeasurementWithErrorTo(
                    foundObject.MeasuredDistance,
                    AstronomicalDistanceUnitType.SI);
                if (convertedDistance.Value != foundObject.MeasuredDistance.Value)
                {
                    estimatedDistance += $"\r\n{convertedDistance}";
                }
            }

            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Estimated Distance:", estimatedDistance, inline: true);
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Angular size:", foundObject.AngularDimensions?.ToString(), inline: true);
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Fluxes:", FormatFluxes(foundObject), inline: false);
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Secondary types:", string.Join(", ", foundObject.OtherTypes), inline: false);
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Also known as:", string.Join(", ", foundObject.OtherNames), inline: false);

            return receivedMessage.Channel.SendMessageAsync(new SendMessage(embeddedMessage));
        }

        private static string FormatFluxes(AstronomicalObject foundObject)
        {
            return string.Join("\r\n", foundObject.Fluxes.Select(flux => $"{Enum.GetName(typeof(FluxType), flux.FluxType)}: {flux.Value} (mag)"));
        }

        private static void AddFieldToEmbeddedMessageIfNotEmpty(
            EmbeddedMessage embeddedMessage,
            string title,
            string fieldValue,
            bool inline)
        {
            if (!string.IsNullOrEmpty(fieldValue))
            {
                embeddedMessage.Fields.Add(new EmbeddedMessageField
                {
                    Name = title,
                    Content = fieldValue,
                    Inline = inline
                });
            }
        }

        private static Task WriteObjectNotFoundAsync(ReceivedMessage receivedMessage, string objectName)
        {
            return receivedMessage.Channel.SendMessageAsync($"No astronomical object found for \"{objectName}\"");
        }
    }
}