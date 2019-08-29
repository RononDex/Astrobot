namespace AstroBot.Objects
{
    public class MeasurementWithError
    {
        public double? Error { get; set; }

        public double Value { get; set; }

        public string Unit { get; set; }

        public override string ToString()
        {
            return $"{Value} {Unit} {(Error != null ? "± " + Error + " " + Unit : string.Empty)}";
        }
    }
}