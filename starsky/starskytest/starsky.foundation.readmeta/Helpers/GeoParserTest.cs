using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.readmeta.Helpers;
using starsky.foundation.readmeta.Models;

namespace starskytest.starsky.foundation.readmeta.Helpers
{
	[TestClass]
	public sealed class GeoParserTest // GeoParserTests
	{
		
		[TestMethod]
		public void GeoParser_ExifRead_ParseGpsTest()
		{
			var latitude = GeoParser.ConvertDegreeMinutesSecondsToDouble("52° 18' 29.54\"", "N");
			Assert.AreEqual(52.308205555500003, latitude, 0.000001);
             
			var longitude = GeoParser.ConvertDegreeMinutesSecondsToDouble("6° 11' 36.8\"", "E");
			Assert.AreEqual(6.1935555554999997, longitude,0.000001);
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
		public void ConvertDegreeMinutesToDouble_WithNorthOrEast_ReturnsPositiveValue()
		{
			// Arrange
			string point = "5,55.840";
			string refGps = "N";

			// Act
			double result = GeoParser.ConvertDegreeMinutesToDouble(point, refGps);

			// Assert
			Assert.IsTrue(result > 0);
		}

		[TestMethod]
		public void ConvertDegreeMinutesToDouble_WithSouthOrWest_ReturnsNegativeValue()
		{
			// Arrange
			string point = "5,55.840";
			string refGps = "W";

			// Act
			double result = GeoParser.ConvertDegreeMinutesToDouble(point, refGps);

			// Assert
			Assert.IsTrue(result < 0);
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
		public void ParseIsoString_InvalidLength_ReturnsEmptyGeoListItem()
		{
			// Arrange
			// Doesn't has seperators so that's why its invalid
			const string isoStr = "InvalidString123456/"; 

			// Act
			var result = GeoParser.ParseIsoString(isoStr);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Latitude);
			Assert.AreEqual(0, result.Longitude);
		}

		[TestMethod]
		public void ParseIsoString_InvalidLength2_ReturnsEmptyGeoListItem()
		{
			// Arrange
			// First part is not dash or plus
			const string isoStr = "Invalid-test-String123456/"; // Replace with your invalid string

			// Act
			var result = GeoParser.ParseIsoString(isoStr);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Latitude);
			Assert.AreEqual(0, result.Longitude);
		}
		
		[TestMethod]
		public void ParseIsoString_InvalidLength1_ReturnsEmptyGeoListItem()
		{
			// Arrange
			const string isoStr = "-Invalid-test-String123456/"; 

			// Act
			var result = GeoParser.ParseIsoString(isoStr);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Latitude);
			Assert.AreEqual(0, result.Longitude);
		}
		
		[TestMethod]
		public void ParseIsoString_InvalidLength3_ReturnsEmptyGeoListItem()
		{
			// Arrange
			const string isoStr = "-In.valid-test-String123456/"; 

			// Act
			var result = GeoParser.ParseIsoString(isoStr);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Latitude);
			Assert.AreEqual(0, result.Longitude);
		}
		
		[TestMethod]
		public void GeoParser_ParseIsoString_Italy()
		{
			var result = GeoParser.ParseIsoString("+40.8516+014.2480+083.241/");
			Assert.AreEqual(40.8516015625,result.Latitude,0.001);
			Assert.AreEqual(14.248000217013889,result.Longitude,0.001);
			Assert.AreEqual(0,result.Altitude,0.001);
		}
	
		[TestMethod]
		public void ParseIsoString_ValidInput_ReturnsCorrectCoordinates()
		{
			// Arrange
			const string input = "+1234.56-09854.321/";

			// Act
			var result = GeoParser.ParseIsoString(input);

			// Assert
			Assert.AreEqual(12.576, result.Latitude, 0.001);
			Assert.AreEqual(-98.90535, result.Longitude, 0.00001);
		}
		
		[TestMethod]
		public void ParseIsoString_InvalidInput_ReturnsEmptyGeoListItem()
		{
			// Arrange
			const string input = "+12.34/";
			var expected = new GeoListItem();

			// Act
			var actual = GeoParser.ParseIsoString(input);

			// Assert
			Assert.AreEqual(expected.Latitude, actual.Latitude, 0.001f);
			Assert.AreEqual(expected.Longitude, actual.Longitude, 0.001f);
			Assert.AreEqual(expected.Altitude, actual.Altitude, 0.001f);
		}
		
		[TestMethod]
		public void ParseIsoString_WhenPointIs4_ReturnsCorrectGeoListItem()
		{
			// Arrange
			const string isoStr = "+1234.56-09854.321/";
			var expected = new GeoListItem
			{
				Latitude = 12.576f,
				Longitude = -98.90535f
			};
        
			// Act
			var actual = GeoParser.ParseIsoString(isoStr);
        
			// Assert
			Assert.AreEqual(expected.Latitude, actual.Latitude, 0.001f);
			Assert.AreEqual(expected.Longitude, actual.Longitude, 0.001f);
		}
		
		[TestMethod]
		public void TestParseIsoString_PointIs6()
		{
			// Arrange
			const string input = "+123456.7-0985432.1+15.9/";
			var expectedOutput = new GeoListItem { Latitude = 12.5824167f, Longitude = -98.9089167f };

			// Act
			var output = GeoParser.ParseIsoString(input);

			// Assert
			Assert.AreEqual(expectedOutput.Latitude, output.Latitude, 0.0001f);
			Assert.AreEqual(expectedOutput.Longitude, output.Longitude, 0.0001f);
		}
		
		[TestMethod]
		public void TestParseIsoStringInvalidAltitude()
		{
			// Arrange
			const string isoStr = "+1234.56-09854.321+abc/";
			var expectedOutput = new GeoListItem { Latitude = 45273.6015625, Longitude = 356059.25 };

			// Act
			var output = GeoParser.ParseIsoString(isoStr);

			// Assert
			Assert.AreEqual(expectedOutput.Latitude, output.Latitude, 0.000001f);
			Assert.AreEqual(expectedOutput.Longitude, output.Longitude, 0.000001f);
			Assert.AreEqual(expectedOutput.Altitude, output.Altitude, 0.000001f);
		}

	}
}
