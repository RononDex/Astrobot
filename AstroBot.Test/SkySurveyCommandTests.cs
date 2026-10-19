using System.Linq;
using System.Text.RegularExpressions;
using AstroBot.Objects;
using AstroBot.SkySurveys;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstroBot.Test
{
    [TestClass]
    public class SkySurveyCommandTests
    {
        [TestMethod]
        [DataRow("how does M31 look like?", "M31", SkySurveyBand.Color)]
        [DataRow("how does M31 look like in colour?", "M31", SkySurveyBand.Color)]
        [DataRow("how does M31 look like in ir?", "M31", SkySurveyBand.Infrared)]
        [DataRow("how does M31 look like in infrared?", "M31", SkySurveyBand.Infrared)]
        [DataRow("how does M42 look in h-alpha?", "M42", SkySurveyBand.Halpha)]
        [DataRow("what does NGC 7000 look like in narrowband", "NGC 7000", SkySurveyBand.Narrowband)]
        public void TestRegexPicksTheRightBand(string message, string objectName, SkySurveyBand band)
        {
            // Same as the regex handler: the first matching regex wins
            var match = new Commands.SkySurveyCommand().Regex
                .Select(r => new Regex(r, RegexOptions.IgnoreCase).Match(message))
                .First(m => m.Success);

            Assert.AreEqual(objectName, match.Groups["ObjectName"].Value);
            Assert.AreEqual(band, Commands.SkySurveyCommand.ParseBand(match.Groups["Band"].Value));
        }

        [TestMethod]
        public void TestAllBandChoicesCanBeParsed()
        {
            var choices = new Commands.SkySurveyCommand().Options.Single(o => o.Name == "band").Choices!;

            CollectionAssert.AreEquivalent(
                System.Enum.GetValues<SkySurveyBand>(),
                choices.Select(c => Commands.SkySurveyCommand.ParseBand((string)c.Value)).ToArray());
        }

        [TestMethod]
        [DataRow(SkySurveyBand.Color, 41.27, SkySurvey.Dss)]
        [DataRow(SkySurveyBand.Red, 41.27, SkySurvey.Dss)]
        [DataRow(SkySurveyBand.Infrared, 41.27, SkySurvey.Dss)]
        [DataRow(SkySurveyBand.Halpha, -15, SkySurvey.Nsns)]
        [DataRow(SkySurveyBand.Narrowband, 41.27, SkySurvey.Nsns)]
        [DataRow(SkySurveyBand.Oiii, -59.87, SkySurvey.Dss)]
        [DataRow(SkySurveyBand.Narrowband, -59.87, SkySurvey.Dss)]
        public void TestGetSkySurvey(SkySurveyBand band, double declination, SkySurvey expected)
        {
            Assert.AreEqual(expected, SkySurveyService.GetSkySurvey(band, declination));
        }
    }
}
