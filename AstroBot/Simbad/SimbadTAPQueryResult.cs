using System;
using System.Linq;
using System.Collections.Generic;
using AstroBot.Objects.AstronomicalObjects;
using Newtonsoft.Json.Linq;

namespace AstroBot.Simbad
{
    /// <summary>
    /// An object representing the result of a SIMBAD query
    /// </summary>
    public class SimbadTAPQueryResult
    {
        public IReadOnlyList<Dictionary<string, object>> ResultDataSet { get; private set; } = null!;

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
            var columnNames = header.Select(x => (string)x["name"]!).ToArray();

            var resultDataSet = new List<Dictionary<string, object>>();

            foreach (var row in parsed["data"]!)
            {
                var dictionaryRow = new Dictionary<string, object>();
                for (var i = 0; i < row.Count(); i++)
                {
                    dictionaryRow.Add(columnNames[i], (row[i] as JValue).Value!);
                }

                resultDataSet.Add(dictionaryRow);
            }
            ResultDataSet = resultDataSet;
        }

        /// <summary>
        /// Parse the result text and create the astronomical objects
        /// </summary>
        public IReadOnlyList<AstronomicalObject> ToAstronomicalObjects()
        {
            var astronomicalObjects = new List<AstronomicalObject>();

            foreach (var entity in ResultDataSet)
            {
                var properties = ParseProperties(entity.Select(x => x.Key).ToArray(), entity.Select(x => x.Value).ToArray());
                var shortType = properties.ContainsKey("TYPESHORT") ? Convert.ToString(properties["TYPESHORT"]) : string.Empty;

                var astronomicalObject = new AstronomicalObject(properties);
                if (!astronomicalObjects.Any(x => x.Name == astronomicalObject.Name))
                {
                    astronomicalObjects.Add(astronomicalObject);
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
        private static Dictionary<string, object?> ParseProperties(string[] columnNames, object[] values)
        {
            var properties = new Dictionary<string, object?>();

            for (var i = 0; i < columnNames.Length; i++)
            {
                var column = columnNames[i];
                var value = values[i];

                // strings are contained within quotes ""
                if (value is string @string)
                {
                    // If the field name is "OtherTypes", then we have to translate the short codes into human readable values
                    if (column == "OtherTypes")
                    {
                        var types = @string.Split("|", StringSplitOptions.RemoveEmptyEntries)
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
                            value: (value as string)?.Trim());
                    }
                }
                // Else its a number (double)
                else if (value is int)
                {
                    properties.Add(
                        key: column,
                        value: (value as int?));
                }
                else if (value is long)
                {
                    properties.Add(
                        key: column,
                        value: (value as long?));
                }
                else if (value is decimal)
                {
                    properties.Add(
                        key: column,
                        value: (value as decimal?));
                }
                else if (value is double)
                {
                    properties.Add(
                        key: column,
                        value: (value as double?));
                }
                else if (value is float)
                {
                    properties.Add(
                        key: column,
                        value: (value as float?));
                }
            }

            return properties;
        }
    }
}
