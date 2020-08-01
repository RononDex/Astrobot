using System.Collections.Generic;
using System.Text;
using AstroBot.Objects.AstronomicalObjects;

namespace AstroBot.Utilities.Exporters
{
    public static class CsvExporter
    {
        public  static string AstronomicalObjectsToCsv(IReadOnlyList<AstronomicalObject> astronomicalObjects)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Name;Type;RA;DEC;Measured Distance;Measured Distance Error;Angular size major axis (');Angular size minor axis (');Angular size major axis angle (°);Relative velocity;Morphological type;Secondary names;Secondary types;");

            foreach(var astronomicalObject in astronomicalObjects)
            {
                csv
                    .Append(astronomicalObject.Name)
                    .Append(';')
                    .Append(astronomicalObject.Type)
                    .Append(';')
                    .Append(astronomicalObject.RaDecCoordinate.RightAscension)
                    .Append(';')
                    .Append(astronomicalObject.RaDecCoordinate.Declination)
                    .Append(';')
                    .Append(astronomicalObject.MeasuredDistance?.Value)
                    .Append(' ')
                    .Append(astronomicalObject.MeasuredDistance?.Unit)
                    .Append(';')
                    .Append(astronomicalObject.MeasuredDistance?.Error)
                    .Append(';')
                    .Append(astronomicalObject.AngularDimensions?.MajorAxis)
                    .Append(';')
                    .Append(astronomicalObject.AngularDimensions?.MinorAxis)
                    .Append(';')
                    .Append(astronomicalObject.AngularDimensions?.MajorAxisAngle)
                    .Append(';')
                    .Append(astronomicalObject.RelativeVelocity?.Value)
                    .Append(' ')
                    .Append(astronomicalObject.RelativeVelocity?.Unit)
                    .Append(';')
                    .Append(astronomicalObject.MorphologicalType)
                    .Append(';')
                    .Append(string.Join(", ", astronomicalObject.OtherNames))
                    .Append(';')
                    .Append(string.Join(", ", astronomicalObject.OtherTypes))
                    .Append(';')
                    .AppendLine();
            }

            return csv.ToString();
        }
    }
}
