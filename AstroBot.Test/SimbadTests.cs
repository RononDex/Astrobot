using AstroBot.Simbad;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AstroBot.Test
{
    [TestClass]
    public class SimbadTests
    {
        [TestMethod]
        public void TestBasicTapQuery()
        {
            var simbadClient = new SimbadClient();
            var testQuery = @"
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
    }
}
