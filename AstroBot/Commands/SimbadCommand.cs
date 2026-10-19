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
    public class SimbadCommand : AwesomeChatBot.Commands.Command, IRegexCommand, ISlashCommand, ICommandDescription
    {
        public string Description => "Access data from the SIMBAD database";

        public override string Name => "Simbad";

        public List<string> Regex => new List<string>()
        {
            @"what is around (?'CenterOfSearchRA'\d*\.\d*)\s(?'CenterOfSearchDEC'[+-]\d*\.\d*)",
            @"what is around (?'CenterOfSearchName'.*\w)",
            @"what do you know about (?'AstroObject'.*\w)(\?)?",
            @"what is (?'AstroObject'.*\w)(\?)?",
        };

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

        public string SlashName => "simbad";

        public string SlashDescription => Description;

        public IReadOnlyList<SlashCommandOption> Options => new[]
        {
            new SlashCommandOption { Name = "object", Description = "Name of the object (e.g. M31), or \"RA DEC\" in degrees when searching around", Type = SlashOptionType.String, Required = true },
            new SlashCommandOption { Name = "around", Description = "List all objects around the object / coordinates instead", Type = SlashOptionType.Boolean, Required = false },
        };

        private static readonly Regex RaDecRegex = new Regex(@"^(?'RA'\d*\.?\d+)\s+(?'DEC'[+-]?\d*\.?\d+)$");

        public Task<bool> ExecuteRegexCommandAsync(ReceivedMessage receivedMessage, Match regexMatch)
        {
            var options = new Dictionary<string, object>();
            if (regexMatch.Groups["CenterOfSearchRA"].Success)
            {
                options["object"] = $"{regexMatch.Groups["CenterOfSearchRA"].Value} {regexMatch.Groups["CenterOfSearchDEC"].Value}";
                options["around"] = true;
            }
            else if (regexMatch.Groups["CenterOfSearchName"].Success)
            {
                options["object"] = regexMatch.Groups["CenterOfSearchName"].Value;
                options["around"] = true;
            }
            else
            {
                options["object"] = regexMatch.Groups["AstroObject"].Value;
            }

            return RegexSlashCommandInvocation.ExecuteAsync(this, receivedMessage, options);
        }

        public Task ExecuteSlashCommandAsync(SlashCommandInvocation invocation)
        {
            var simbadClient = new SimbadClient();
            var objectName = invocation.GetOption<string>("object").Trim();

            return invocation.GetOption<bool>("around")
                ? SearchForObjectsAroundAsync(invocation, objectName, simbadClient)
                : SearchForObjectByNameAsync(invocation, objectName, simbadClient);
        }

        private static async Task SearchForObjectsAroundAsync(SlashCommandInvocation invocation, string objectName, SimbadClient simbadClient)
        {
            const float radiusInDegrees = 0.5f;
            RaDecCoordinate? centerCoordinates;

            var raDecMatch = RaDecRegex.Match(objectName);
            if (raDecMatch.Success)
            {
                centerCoordinates = new RaDecCoordinate(
                    double.Parse(raDecMatch.Groups["RA"].Value, CultureInfo.InvariantCulture),
                    double.Parse(raDecMatch.Groups["DEC"].Value, CultureInfo.InvariantCulture));
            }
            else
            {
                var queryAroundObject = await simbadClient.FindObjectByNameAsync(objectName).ConfigureAwait(false);
                if (queryAroundObject == null)
                {
                    await invocation.RespondAsync($"No object with that name found in the SIMBAD databse!").ConfigureAwait(false);
                    return;
                }

                centerCoordinates = queryAroundObject.RaDecCoordinate;
            }

            if (centerCoordinates == null)
            {
                await invocation.RespondAsync("Could not parse RA/DEC coordinates").ConfigureAwait(false);
                return;
            }

            var objectsAroundTarget = await simbadClient.QueryAroundAsync(centerCoordinates, radiusInDegrees).ConfigureAwait(false);
            var csvString = CsvExporter.AstronomicalObjectsToCsv(objectsAroundTarget);
            await invocation.RespondAsync($"Found {objectsAroundTarget.Count} objects around {centerCoordinates} within a radius of {radiusInDegrees}°:").ConfigureAwait(false);
            await invocation.RespondAsync(
                    new SendMessage(
                        content: null,
                        new List<Attachment>
                        {
                            new SendAttachment
                            {
                                Name = "queryResult.csv",
                                Content = Encoding.ASCII.GetBytes(csvString)
                            }
                        })).ConfigureAwait(false);
        }

        private static async Task SearchForObjectByNameAsync(SlashCommandInvocation invocation, string objectName, SimbadClient simbadClient)
        {
            var foundObject = await simbadClient.FindObjectByNameAsync(objectName).ConfigureAwait(false);
            if (foundObject == null)
            {
                await invocation.RespondAsync($"No astronomical object found for \"{objectName}\"").ConfigureAwait(false);
                return;
            }

            await WriteObjectDetailsEmbeddedAsync(invocation, foundObject).ConfigureAwait(false);
        }

        private static Task WriteObjectDetailsEmbeddedAsync(SlashCommandInvocation invocation, AstronomicalObject foundObject)
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

            var measuredDistance = foundObject.MeasuredDistance;
            var estimatedDistance = measuredDistance != null
                ? $"{measuredDistance}"
                : null;
            if (measuredDistance != null)
            {
                var convertedDistance = AstronomicalDistanceUnitConverter.ConvertMeasurementWithErrorTo(
                    measuredDistance,
                    AstronomicalDistanceUnitType.SI);
                if (convertedDistance.Value != measuredDistance.Value)
                {
                    estimatedDistance += $"\r\n{convertedDistance}";
                }
            }

            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Estimated Distance:", estimatedDistance, inline: true);
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Angular size:", foundObject.AngularDimensions?.ToString(), inline: true);
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Fluxes [mag]:", FormatFluxes(foundObject), inline: false);
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Secondary types:", string.Join(", ", foundObject.OtherTypes), inline: false);
            AddFieldToEmbeddedMessageIfNotEmpty(embeddedMessage, "Also known as:", string.Join(", ", foundObject.OtherNames).WithMaxLength(1024), inline: false);

            return invocation.RespondAsync(new SendMessage(embeddedMessage));
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
            string? fieldValue,
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
    }
}
