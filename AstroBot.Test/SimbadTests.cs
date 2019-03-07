using System.IO;
using AstroBot.Simbad;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstroBot.Test
{
    [TestClass]
    public class SimbadTests
    {
        [TestInitialize]
        public void Setup()
        {
            Directory.SetCurrentDirectory("../../../../AstroBot/Bin/Debug/netcoreapp2.2/");
        }

        [TestMethod]
        public void TestBasicTapQuery()
        {
            var simbadClient = new SimbadClient();
            var testQuery = @"
                -- Basic data from an object given one of its identifiers.
                SELECT basic.OID,
                    RA,
                    DEC,
                    main_id AS ""Name"",
                    coo_bibcode AS ""BiblioReference"",
                    nbref AS ""NbReferences"",
                    plx_value as ""Parallax"",
                    rvz_radvel as ""Radial velocity"",
                    galdim_majaxis,
                    galdim_minaxis,
                    galdim_angle AS ""Galaxy ellipse angle""
                FROM basic JOIN flux ON oidref = oid
                WHERE filter = 'B'
                    AND CONTAINS(POINT('ICRS', RA, DEC), CIRCLE('ICRS', 10, 5, 5)) = 1
                ORDER BY ""Name""; ";

            var result = simbadClient.QuerySimbad(new SimbadTAPQuery(testQuery));
        }

        [TestMethod]
        public void TestFindObjectByName()
        {
            var objName = "M78";
            var simbadClient = new SimbadClient();
            var foundObject = simbadClient.FindObjectByName(objName);
        }

        [TestMethod]
        public void TestQueryAround()
        {
            var simbadClient = new SimbadClient();
            var objects = simbadClient.QueryAround(
                new Objects.RaDecCoordinate(rightAscension: 20, declination: 10),
                radiusInDegrees: 1.5f);
        }
    }
}
