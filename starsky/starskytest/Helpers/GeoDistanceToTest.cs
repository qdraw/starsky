using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;

namespace starskytest.Helpers
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
        
	    [TestMethod]
	    public void GeoDistanceTo_ExifRead_ParseGpsTest()
	    {

		    var latitude = GeoDistanceTo.ConvertDegreeMinutesSecondsToDouble("52° 18' 29.54\"", "N");
		    Assert.AreEqual(latitude,  52.308205555500003, 0.000001);
             
		    var longitude = GeoDistanceTo.ConvertDegreeMinutesSecondsToDouble("6° 11' 36.8\"", "E");
		    Assert.AreEqual(longitude,  6.1935555554999997, 0.000001);
	    }
	    
	             
	    [TestMethod]
	    public void GeoDistanceTo_ExifRead_ConvertDegreeMinutesToDouble_ConvertLongLat()
	    {

		    var input = "52,20.708N";
		    string refGps = input.Substring(input.Length-1, 1);
		    var data = GeoDistanceTo.ConvertDegreeMinutesToDouble(input, refGps);
		    Assert.AreEqual(52.3451333333,data,0.001);

            
		    var input1 = "5,55.840E";
		    string refGps1 = input1.Substring(input1.Length-1, 1);
		    var data1 = GeoDistanceTo.ConvertDegreeMinutesToDouble(input1, refGps1);
		    Assert.AreEqual(5.930,data1,0.001);

	    }
            
    }
}