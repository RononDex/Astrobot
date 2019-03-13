namespace AstroBot.Objects
{
    public class MeasurementWithError
    {
        public double? Error { get; set; }

        public double Value { get; set; }

        public string Unit { get; set; }
    }
}