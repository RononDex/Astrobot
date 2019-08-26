using System;
using AstroBot.Objects;

namespace AstroBot.Utilities.UnitConverters
{
    public static class AstronomicalDistanceUnitConverter
    {
        private const double FACTOR_PC_TO_LY = 3.2614708603742;

        public static AstronomicalDistanceUnitType DetermineUnitType(string unit)
        {
            if (unit.Contains("pc")
                || unit.Contains("AU"))
            {
                return AstronomicalDistanceUnitType.AstronomicalUnits;
            }

            return AstronomicalDistanceUnitType.SI;
        }

        public static MeasurementWithError ConvertMeasurementWithErrorTo(
            MeasurementWithError measurementWithError,
            AstronomicalDistanceUnitType targetUnitType)
        {
            var convertedValue = ConvertUnitTo(measurementWithError.Value, measurementWithError.Unit, targetUnitType);
            if (convertedValue.convertedValue == measurementWithError.Value)
                return measurementWithError;

            var convertedErrorValue = ConvertUnitTo(measurementWithError.Error ?? 0, measurementWithError.Unit, targetUnitType);

            return new MeasurementWithError
            {
                Error = convertedErrorValue.convertedValue,
                Unit = convertedValue.newUnit,
                Value = convertedValue.convertedValue
            };
        }

        public static (double convertedValue, string newUnit) ConvertUnitTo(
            double value,
            string currentUnit,
            AstronomicalDistanceUnitType targetUnitType)
        {
            var convertedValue = value;
            var convertedUnit = currentUnit;
            var currentUnitType = DetermineUnitType(currentUnit);
            if (currentUnitType == targetUnitType)
                return (value, currentUnit);

            if (currentUnitType == AstronomicalDistanceUnitType.AstronomicalUnits
                && targetUnitType == AstronomicalDistanceUnitType.SI)
            {
                if (currentUnit.Contains("pc"))
                {
                    convertedValue = value * FACTOR_PC_TO_LY;
                    convertedUnit = currentUnit.Replace("pc", "Ly", System.StringComparison.Ordinal);
                }
            }

            if (currentUnitType == AstronomicalDistanceUnitType.SI
                && targetUnitType == AstronomicalDistanceUnitType.AstronomicalUnits)
            {
                if (currentUnit.Contains("Ly"))
                {
                    convertedValue = value / FACTOR_PC_TO_LY;
                    convertedUnit = currentUnit.Replace("Ly", "pc", System.StringComparison.Ordinal);
                }
            }

            convertedValue = Math.Round(convertedValue, 6);
            return (convertedValue, convertedUnit);
        }
    }

    public enum AstronomicalDistanceUnitType
    {
        /// <summary>
        /// SI units (light years)
        /// </summary>
        SI,

        /// <summary>
        /// Astronomical unit system (parsecs, AU, ...)
        /// </summary>
        AstronomicalUnits
    }
}