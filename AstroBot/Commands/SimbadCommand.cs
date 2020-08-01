using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AstroBot.Objects;
using AstroBot.Objects.AstronomicalObjects;
using AstroBot.Simbad;
using AstroBot.Utilities.Exporters;
using AstroBot.Utilities.Extensions;
using AstroBot.Utilities.UnitConverters;
using AwesomeChatBot.ApiWrapper;
using AwesomeChatBot.Commands.Handlers;

namespace AstroBot.Commands
{
    public class SimbadCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ICommandDescription
    {
        public string Description => "Access data from the SIMBAD database";

        public string[] ExampleCalls => new string[]
        {
            "what is M31?",
            "what do you know about M31?",
            "what is around [RA] [DEC]",
            "what is around M51",
            // "how far away is M13?",
            // "fluxes of NGC6888?",
            // "how bright is M78?",
            // "how big is M31?"
        };

        public List<string> Regex => new List<string>()
        {
            @"what is around (?'CenterOfSearchRA'\d*\.\d*)\s(?'CenterOfSearchDEC'[+-]\d*\.\d*)",
            @"what is around (?'CenterOfSearchName'.*\w)",
            @"what do you know about (?'AstroObject'.*\w)(\?)?",
            @"what is (?'AstroObject'.*\w)(\?)?",
        };

        public override string Name => "Simbad";

        public async Task<bool> ExecuteRegexCommand(ReceivedMessage receivedMessage, Match regexMatch)
        {
            var simbadClient = new SimbadClient();

            if (regexMatch.Groups["AstroObject"].Success)
            {
                SearchForObjectByNameAsync(receivedMessage, regexMatch, simbadClient);
            }
            if (regexMatch.Groups["CenterOfSearchRA"].Success
                || regexMatch.Groups["CenterOfSearchName"].Success)
            {
                SearchForObjectsAroundRaDec(receivedMessage, regexMatch, simbadClient);
            }
            return true;
        }

        private async void SearchForObjectsAroundRaDec(ReceivedMessage receivedMessage, Match regexMatch, SimbadClient simbadClient)
        {
            RaDecCoordinate centerCoordinates = null;
            var radiusInDegrees = 0.5f;
            try
            {
                if (regexMatch.Groups["CenterOfSearchRA"].Success)
                {
                    centerCoordinates = new RaDecCoordinate(
                        double.Parse(regexMatch.Groups["CenterOfSearchRA"].Value, CultureInfo.InvariantCulture),
                        double.Parse(regexMatch.Groups["CenterOfSearchDEC"].Value, CultureInfo.InvariantCulture));
                }
                else if (regexMatch.Groups["CenterOfSearchName"].Success)
                {
                    var name = regexMatch.Groups["CenterOfSearchName"].Value;
                    var queryAroundObject = simbadClient.FindObjectByName(name);
                    if (queryAroundObject == null)
                    {
                        await receivedMessage.Channel.SendMessageAsync($"No object with name {name} found in the SIMBAD databse!");
                        return;
                    }

                    centerCoordinates = queryAroundObject.RaDecCoordinate;
                }
            }
            finally
            {
                if (centerCoordinates == null)
                {
                    await receivedMessage.Channel.SendMessageAsync("Could not parse RA/DEC coordinates");
                }
            }

            var objectsAroundTarget = simbadClient.QueryAround(centerCoordinates, radiusInDegrees);
            var csvString = CsvExporter.AstronomicalObjectsToCsv(objectsAroundTarget);
            await receivedMessage.Channel.SendMessageAsync($"Found {objectsAroundTarget.Count} objects around {centerCoordinates} within a radius of {radiusInDegrees}°:");
            await receivedMessage.Channel.SendMessageAsync(
                    new SendMessage(
                        content: null,
                        new List<Attachment>
                        {
                            new SendAttachment
                            {
                                Name = "queryResult.csv",
                                Content = Encoding.ASCII.GetBytes(csvString)
                            }
                        }));
        }

        private static async Task SearchForObjectByNameAsync(ReceivedMessage receivedMessage, Match regexMatch, SimbadClient simbadClient)
        {
            var objectName = regexMatch.Groups["AstroObject"].Value;
            var foundObject = simbadClient.FindObjectByName(objectName);
            if (foundObject == null)
            {
                WriteObjectNotFound(receivedMessage, objectName);
                return;
            }

            await WriteObjectDetailsEmbedded(receivedMessage, foundObject);
        }

        private static async Task WriteObjectDetailsEmbedded(ReceivedMessage receivedMessage, AstronomicalObject foundObject)
        {
            var embeddedMessage = new EmbeddedMessage
            {
                Title = foundObject.Name,
                ThumbnailUrl = $"http://alasky.u-strasbg.fr/cgi/simbad-thumbnails/get-thumbnail.py?oid={foundObject.SimbadId}&size=200&legend=true"
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
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Fluxes [mag]:", FormatFluxes(foundObject), inline: false);
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Secondary types:", string.Join(", ", foundObject.OtherTypes), inline: false);
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Also known as:", string.Join(", ", foundObject.OtherNames).WithMaxLength(1024), inline: false);

            await receivedMessage.Channel.SendMessageAsync(new SendMessage(embeddedMessage));
        }

        private static string FormatFluxes(AstronomicalObject foundObject)
        {
            return string.Join("\r\n", foundObject.Fluxes.Select(flux =>
            {
                var displayInformation = Flux.FluxRangesLookup.FirstOrDefault(x => x.Key == flux.FluxType);
                return $"{Enum.GetName(typeof(FluxType), flux.FluxType)}: {flux.Value} ({displayInformation.Value.From} - {displayInformation.Value.To})";
            }));
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

        private static void WriteObjectNotFound(ReceivedMessage receivedMessage, string objectName)
        {
            receivedMessage.Channel.SendMessageAsync($"No astronomical object found for \"{objectName}\"");
        }
    }
}
