using System;
using System.IO;
using System.Threading.Tasks;
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
            // The Simbad/Queries files are copied next to the test assembly
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        }

        [TestMethod]
        public async Task TestBasicTapQuery()
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

            var result = await simbadClient.QuerySimbadAsync(new SimbadTAPQuery(testQuery));
            Assert.IsTrue(result.ResultDataSet.Count > 0);
        }

        [TestMethod]
        public async Task TestFindObjectByName()
        {
            var objName = "M31";
            var simbadClient = new SimbadClient();
            var foundObject = await simbadClient.FindObjectByNameAsync(objName);
            Assert.IsNotNull(foundObject);
        }

        [TestMethod]
        public async Task TestConcurrentQueries()
        {
            var simbadClient = new SimbadClient();
            var found = await Task.WhenAll(
                simbadClient.FindObjectByNameAsync("M31"),
                simbadClient.FindObjectByNameAsync("M42"),
                simbadClient.FindObjectByNameAsync("M51"));
            CollectionAssert.AllItemsAreNotNull(found);
        }

        [TestMethod]
        public async Task TestQueryAround()
        {
            var simbadClient = new SimbadClient();
            var objects = await simbadClient.QueryAroundAsync(
                new Objects.RaDecCoordinate(rightAscension: 20, declination: 10),
                radiusInDegrees: 1.5f);
            Assert.IsTrue(objects.Count > 0);
        }
    }
}
