using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.readmeta.Helpers;

namespace starskytest.starsky.foundation.readmeta.Helpers
{
	[TestClass]
	public class GeoParserTest
	{
		
		[TestMethod]
		public void GeoParser_ExifRead_ParseGpsTest()
		{
			var latitude = GeoParser.ConvertDegreeMinutesSecondsToDouble("52° 18' 29.54\"", "N");
			Assert.AreEqual(latitude,  52.308205555500003, 0.000001);
             
			var longitude = GeoParser.ConvertDegreeMinutesSecondsToDouble("6° 11' 36.8\"", "E");
			Assert.AreEqual(longitude,  6.1935555554999997, 0.000001);
		}
	    
	             
		[TestMethod]
		public void GeoParser_ExifRead_ConvertDegreeMinutesToDouble_ConvertLongLat()
		{
			var input = "52,20.708N";
			string refGps = input.Substring(input.Length-1, 1);
			var data = GeoParser.ConvertDegreeMinutesToDouble(input, refGps);
			Assert.AreEqual(52.3451333333,data,0.001);

			var input1 = "5,55.840E";
			string refGps1 = input1.Substring(input1.Length-1, 1);
			var data1 = GeoParser.ConvertDegreeMinutesToDouble(input1, refGps1);
			Assert.AreEqual(5.930,data1,0.001);
		}

		[TestMethod]
		public void GeoParser_ParseIsoString_DoesNotEndOnSlash()
		{
			var result = GeoParser.ParseIsoString("-05.2169-080.6303"); // no slash here
			Assert.AreEqual(0,result.Latitude,0.001);
		}
		
		[TestMethod]
		public void GeoParser_ParseIsoString_WrongPlusFormat()
		{
			var result = GeoParser.ParseIsoString("0-05"); 
			Assert.AreEqual(0,result.Latitude,0.001);
		}
		
		[TestMethod]
		public void GeoParser_ParseIsoString_LotsOfMinus()
		{
			var result = GeoParser.ParseIsoString("0-05-0-0-0-0-0-0-0-0"); 
			Assert.AreEqual(0,result.Latitude,0.001);
		}
		
		[TestMethod]
		public void GeoParser_ParseIsoString_Peru()
		{
			var result = GeoParser.ParseIsoString("-05.2169-080.6303/");
			Assert.AreEqual(-5.2168999565972225,result.Latitude,0.001);
			Assert.AreEqual(-80.63030381944445,result.Longitude,0.001);
			Assert.AreEqual(0,result.Altitude,0.001);
		}

		[TestMethod]
		public void GeoParser_ParseIsoString_NotSupported()
		{
			var result = GeoParser.ParseIsoString("+40.20361-75.00417CRSxxxx/");
			Assert.AreEqual(0,result.Latitude,0.001);
			Assert.AreEqual(0,result.Longitude,0.001);
			Assert.AreEqual(0,result.Altitude,0.001);
		}
		
		[TestMethod]
		public void GeoParser_ParseIsoString_Italy()
		{
			var result = GeoParser.ParseIsoString("+40.8516+014.2480+083.241/");
			Assert.AreEqual(40.8516015625,result.Latitude,0.001);
			Assert.AreEqual(14.248000217013889,result.Longitude,0.001);
			Assert.AreEqual(0,result.Altitude,0.001);
		}
	
	}
}
