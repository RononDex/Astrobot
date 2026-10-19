using System.Linq;
using System.Text.RegularExpressions;
using AstroBot.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstroBot.Test
{
    [TestClass]
    public class SkySurveyCommandTests
    {
        [TestMethod]
        [DataRow("how does M31 look like?", "ObjectName", "M31", "")]
        [DataRow("how does M31 look like in color?", "ObjectNameColor", "M31", "")]
        [DataRow("how does M31 look like in infrared?", "ObjectNameIr", "M31", "")]
        [DataRow("how does M42 look in h-alpha?", "ObjectNameNarrowband", "M42", "h-alpha")]
        [DataRow("what does NGC 7000 look like in narrowband", "ObjectNameNarrowband", "NGC 7000", "narrowband")]
        public void TestRegexPicksTheRightBand(string message, string group, string objectName, string band)
        {
            // Same as the regex handler: the first matching regex wins
            var match = new Commands.SkySurveyCommand().Regex
                .Select(r => new Regex(r, RegexOptions.IgnoreCase).Match(message))
                .First(m => m.Success);

            Assert.AreEqual(objectName, match.Groups[group].Value);
            Assert.AreEqual(band, match.Groups["Band"].Value);
        }
    
        [TestMethod]
        [DataRow(null, 41.27, SkySurvey.Dss)]
        [DataRow("color", 41.27, SkySurvey.Dss)]
        [DataRow("halpha", -15, SkySurvey.Nsns)]
        [DataRow("narrowband", 41.27, SkySurvey.Nsns)]
        [DataRow("red", 41.27, SkySurvey.Dss)]
        [DataRow("infrared", 41.27, SkySurvey.Dss)]
        [DataRow(null, -59.87, SkySurvey.Dss)]
        [DataRow("oiii", -59.87, SkySurvey.Dss)]
        [DataRow("narrowband", -59.87, SkySurvey.Dss)]
        public void TestGetSkySurvey(string band, double declination, SkySurvey expected)
        {
            Assert.AreEqual(expected, Commands.SkySurveyCommand.GetSkySurvey(band, declination));
        }
    }
}
