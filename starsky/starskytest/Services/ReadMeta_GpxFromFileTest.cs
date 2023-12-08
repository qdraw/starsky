using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Services
{
	[TestClass]
	public sealed class ReadGpxFromFileTest
	{
		[TestMethod]
		public void ReadGpxFromFileTest_ReturnAfterFirstFieldReadFile_Null()
		{
			var returnItem =
				new ReadMetaGpx(new FakeIWebLogger())
					.ReadGpxFromFileReturnAfterFirstField(null, "/test.gpx");
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,
				returnItem.Status);
			Assert.AreEqual("/test.gpx", returnItem.FilePath);
		}

		[TestMethod]
		public void ReadGpxFromFileTest_ReturnAfterFirstFieldReadFile()
		{
			var gpxBytes = CreateAnGpx.Bytes.ToArray();
			MemoryStream stream = new MemoryStream(gpxBytes);

			var returnItem =
				new ReadMetaGpx(new FakeIWebLogger())
					.ReadGpxFromFileReturnAfterFirstField(stream, "/test.gpx");
			Assert.AreEqual(5.485941, returnItem.Longitude, 0.001);
			Assert.AreEqual(51.809360, returnItem.Latitude, 0.001);
			Assert.AreEqual("_20180905-fietsen-oss", returnItem.Title);
			Assert.AreEqual(7.263, returnItem.LocationAltitude, 0.001);

			DateTime.TryParseExact("2018-09-05T19:31:53Z",
				"yyyy-MM-ddTHH:mm:ssZ",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal,
				out var expectDateTime);

			Assert.AreEqual(expectDateTime, returnItem.DateTime);
			Assert.AreEqual("/test.gpx", returnItem.FilePath);
		}

		[TestMethod]
		public void
			ReadGpxFromFileTest_ReturnAfterFirstFieldReadFile_Utc_UseLocalFalse()
		{
			var gpxBytes = CreateAnGpx.Bytes.ToArray();
			MemoryStream stream = new MemoryStream(gpxBytes);

			var returnItem = new ReadMetaGpx(new FakeIWebLogger())
				.ReadGpxFromFileReturnAfterFirstField(stream, "/test.gpx",
					false);
			Assert.AreEqual(5.485941, returnItem.Longitude, 0.001);
			Assert.AreEqual(51.809360, returnItem.Latitude, 0.001);
			Assert.AreEqual("_20180905-fietsen-oss", returnItem.Title);
			Assert.AreEqual(7.263, returnItem.LocationAltitude, 0.001);

			DateTime.TryParseExact("2018-09-05T17:31:53Z",
				"yyyy-MM-ddTHH:mm:ssZ",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal,
				out var expectDateTime);
			// gpx is always utc
			Assert.AreEqual(expectDateTime, returnItem.DateTime);
			Assert.AreEqual("/test.gpx", returnItem.FilePath);
		}

		[TestMethod]
		public void ReadGpxFromFileTest_NonValidInput()
		{
			var gpxBytes = Array.Empty<byte>();
			MemoryStream stream = new MemoryStream(gpxBytes);

			var returnItem =
				new ReadMetaGpx(new FakeIWebLogger())
					.ReadGpxFromFileReturnAfterFirstField(stream, "/test.gpx");
			Assert.AreEqual(new DateTime(), returnItem.DateTime);
			Assert.AreEqual("/test.gpx", returnItem.FilePath);
		}

		[TestMethod]
		public void ReadGpxFromFileTest_TestFileName()
		{
			var gpxBytes = CreateAnGpx.Bytes.ToArray();
			MemoryStream stream = new MemoryStream(gpxBytes);

			var returnItem =
				new ReadMetaGpx(new FakeIWebLogger())
					.ReadGpxFromFileReturnAfterFirstField(stream, "/test.gpx");
			Assert.AreEqual("test.gpx", returnItem.FileName);
			Assert.AreEqual("/", returnItem.ParentDirectory);
		}

		[TestMethod]
		public void ReadGpxFromFileTest_ReadFile()
		{
			var gpxBytes = CreateAnGpx.Bytes.ToArray();
			MemoryStream stream = new MemoryStream(gpxBytes);
			var returnItem =
				new ReadMetaGpx(new FakeIWebLogger()).ReadGpxFile(stream);
			Assert.AreEqual(5.485941, returnItem.FirstOrDefault()!.Longitude,
				0.001);
			Assert.AreEqual(51.809360, returnItem.FirstOrDefault()!.Latitude,
				0.001);
			DateTime.TryParseExact("2018-09-05T17:31:53Z",
				"yyyy-MM-ddTHH:mm:ssZ",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal,
				out var expectDateTime);

			// gpx is always utc
			Assert.AreEqual(expectDateTime,
				returnItem.FirstOrDefault()!.DateTime);
			Assert.AreEqual("_20180905-fietsen-oss",
				returnItem.FirstOrDefault()!.Title);
			Assert.AreEqual(7.263, returnItem.FirstOrDefault()!.Altitude,
				0.001);
		}

		[TestMethod]
		public void ParseXml_XXETest()
		{
			var xxeExample = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
			                 "<!DOCTYPE foo [ <!ENTITY xxe SYSTEM \"file:///etc/passwd\"> ]>" +
			                 "<stockCheck><productId>&xxe;</productId></stockCheck>";

			var returnItem = ReadMetaGpx.ParseXml(xxeExample);

			Assert.AreEqual(string.Empty, returnItem.ChildNodes[1]!.InnerText);
		}

		[TestMethod]
		public void ConvertDateTime()
		{
			var result = ReadMetaGpx.ConvertDateTime(DateTime.MinValue,
				true, 0, 0);

			Assert.AreEqual(1, result.Year);
			Assert.AreEqual(1, result.Month);
			Assert.AreEqual(1, result.Day);
		}

		[TestMethod]
		public void ConvertDateTime_SummerTime()
		{
			DateTime.TryParseExact("2022-09-05T17:31:53Z",
				"yyyy-MM-ddTHH:mm:ssZ",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal,
				out var expectDateTime);
			
			var result = ReadMetaGpx.ConvertDateTime(expectDateTime,
				true, 52.373882, 4.891711);

			Assert.AreEqual(19, result.Hour);
			Assert.AreEqual(31, result.Minute);
			Assert.AreEqual(53, result.Second);
		}
		
		[TestMethod]
		public void ConvertDateTime_WinterTime()
		{
			DateTime.TryParseExact("2022-11-05T17:31:53Z",
				"yyyy-MM-ddTHH:mm:ssZ",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal,
				out var expectDateTime);
			
			var result = ReadMetaGpx.ConvertDateTime(expectDateTime,
				true, 52.373882, 4.891711);

			Assert.AreEqual(18, result.Hour);
			Assert.AreEqual(31, result.Minute);
			Assert.AreEqual(53, result.Second);
		}
	}
}
