using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.readmeta.Helpers;

namespace starskytest.starsky.foundation.readmeta.Helpers
{
    [TestClass]
    public class GeoDistanceToTest
    {
        [TestMethod]
        public void GetDistanceTest()
        {
            var distance = GeoDistanceTo.GetDistance(51.6824134, 5.2984455, 51.6852471, 5.2943688);
            //  	0.4222 km
            // https://www.movable-type.co.uk/scripts/latlong.html
            // https://stackoverflow.com/questions/27928/calculate-distance-between-two-latitude-longitude-points-haversine-formula
            Assert.AreEqual(0.4223,distance,0.001);
        }
        
        [TestMethod]
        public void GetDistanceTestLargeNumber()
        {
            var distance = GeoDistanceTo.GetDistance(-51.6824134, -5.2984455, 51.6852471, 5.2943688);
            Assert.AreEqual(11549.857139273592,distance,0.001);
        }
    }
}
