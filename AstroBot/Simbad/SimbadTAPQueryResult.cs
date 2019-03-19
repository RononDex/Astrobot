using System;
using System.Linq;
using System.Collections.Generic;
using AstroBot.Objects;
using AstroBot.Objects.AstronomicalObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AstroBot.Simbad
{
    /// <summary>
    /// An object representing the result of a SIMBAD query
    /// </summary>
    public class SimbadTAPQueryResult
    {

        public IReadOnlyList<Dictionary<string, string>> ResultDataSet { get; private set; }

        /// <summary>
        /// Initializes the result from the result of a TAP query
        /// http://simbad.u-strasbg.fr/simbad/sim-tap
        /// </summary>
        /// <param name="tapResultText"></param>
        public SimbadTAPQueryResult(string tapResultText)
        {
            ParseResultIntoDataset(tapResultText);
        }

        /// <summary>
        /// Parses the results into the internal dataset storage
        /// </summary>
        /// <param name="tapResultText"></param>
        private void ParseResultIntoDataset(string tapResultText)
        {
            var parsed = JObject.Parse(tapResultText);
            var header = parsed["metadata"];

            var resultDataSet = new List<Dictionary<string, string>>();
            var rows = tapResultText.Replace("\r", "").Split("\n");

            // If less than 3 rows --> no rows in the result (since header has 2 rows)
            if (rows.Length < 3)
                return;

            var headerRow = rows.First().ToUpper();
            var columnNames = headerRow.Split("|", StringSplitOptions.None).Select(x => x.Trim()).ToArray();


            foreach (var row in rows.Skip(2))
            {
                if (string.IsNullOrWhiteSpace(row))
                    continue;

                var curRowColumns = row.Split('"')
                    .Select((element, index) => index % 2 == 0  // If even index
                                           ? element.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)  // Split the item
                                           : new string[] { element.Trim() })  // Keep the entire item
                    .SelectMany(element => element)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Select(s => s.Trim())
                    .ToArray();


                // var curRowColumns = row.Split("|", StringSplitOptions.None).Select(x => x.Trim()).ToArray();
                var dictionaryRow = new Dictionary<string, string>();
                for (var i = 0; i < curRowColumns.Length; i++)
                {
                    dictionaryRow.Add(columnNames[i], curRowColumns[i]);
                }

                ResultDataSet = resultDataSet;
            }
        }

        /// <summary>
        /// Parse the result text and create the astronomical objects
        /// </summary>
        /// <param name="tapResultText"></param>
        public IReadOnlyList<AstronomicalObject> ToAstronomicalObjects()
        {
            var astronomicalObjects = new List<AstronomicalObject>();

            foreach (var entity in ResultDataSet)
            {
                var properties = ParseProperties(entity.Select(x => x.Key).ToArray(), entity.Select(x => x.Value).ToArray());
                var shortType = properties.ContainsKey("TYPESHORT") ? Convert.ToString(properties["TYPESHORT"]) : string.Empty;

                // Depending on the type, create objects of different types
                if (shortType.Contains('*'))
                {
                    astronomicalObjects.Add(new Star(properties));
                }
                else
                {
                    astronomicalObjects.Add(new AstronomicalObject(properties));
                }
            }

            return astronomicalObjects;
        }

        /// <summary>
        /// Parses the row into properties for the astronomical object
        /// </summary>
        /// <param name="columnNames"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private Dictionary<string, object> ParseProperties(string[] columnNames, string[] values)
        {
            var properties = new Dictionary<string, object>();

            for (var i = 0; i < columnNames.Length; i++)
            {
                var column = columnNames[i];
                var value = values[i];

                // strings are contained within quotes ""
                if (value.StartsWith("\"") && value.EndsWith("\""))
                {
                    // If the field name is "OtherTypes", then we have to translate the short codes into human readable values
                    if (column == "OtherTypes")
                    {
                        var types = value.Split("|", StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                            .Select(x => SimbadClient.ShortTypeNameCache.ContainsKey(x) ? SimbadClient.ShortTypeNameCache[x] : x);

                        properties.Add(
                            key: column,
                            value: string.Join('|', types));
                    }
                    else
                    {
                        properties.Add(
                            key: column,
                            value: value.Substring(1, value.Length - 2));
                    }
                }
                // Else its a number (double)
                else if (double.TryParse(value, out double parsedDouble))
                {
                    properties.Add(
                        key: column,
                        value: parsedDouble);
                }
            }

            return properties;
        }
    }
}