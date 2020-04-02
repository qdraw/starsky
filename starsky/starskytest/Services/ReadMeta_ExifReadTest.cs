using System;
using System.Collections.Generic;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.readmeta.Services;
using starskycore.Attributes;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Services
{
	public class MockDirectory : Directory
	{
		public override string Name => string.Empty;

		[ExcludeFromCoverage]
		protected override bool TryGetTagName(int tagType, out string tagName)
		{
			tagName = null;
			return false;
		}
	}

	[TestClass]
	public class ExifReadTest
	{

		 [TestMethod]
		 [ExcludeFromCoverage]
		 public void ExifRead_GetObjectNameNull()
		 {
		     var t = new ReadMetaExif(null).GetObjectName(new MockDirectory());
		     Assert.AreEqual( string.Empty,t);
		 }

		 [TestMethod]
		 [ExcludeFromCoverage]
		 public void ExifRead_GetObjectNameTest()
		 {
		     var dir = new IptcDirectory();
		     dir.Set(IptcDirectory.TagObjectName, "test" );
		     var t = new ReadMetaExif(null).GetObjectName(dir);
		     Assert.AreEqual(t, "test");
		     Assert.AreNotEqual(t,null);
		 }

		[TestMethod]
		[ExcludeFromCoverage]
		public void ExifRead_GetCaptionAbstractTest()
		{
		    var dir = new IptcDirectory();
		    dir.Set(IptcDirectory.TagCaption, "test123");
		    var t = new ReadMetaExif(null).GetCaptionAbstract(dir);
		    Assert.AreEqual(t, "test123");
		    Assert.AreNotEqual(t,string.Empty);
		    Assert.AreNotEqual(t,null);
		}
		 
		 [TestMethod]
		 [ExcludeFromCoverage]
		 public void ExifRead_GetExifKeywordsSingleTest()
		 {
		     var dir = new IptcDirectory();
		     dir.Set(IptcDirectory.TagKeywords, "test123");
		     var t = new ReadMetaExif(null).GetExifKeywords(dir);
		     Assert.AreEqual(t, "test123");
		     Assert.AreNotEqual(t,null);
		 }
		 
		 [TestMethod]
		 [ExcludeFromCoverage]
		 public void ExifRead_GetExifKeywordsMultipleTest()
		 {
		     var dir = new IptcDirectory();
		     dir.Set(IptcDirectory.TagKeywords, "test123;test12");
		     var t = new ReadMetaExif(null).GetExifKeywords(dir);
		     Assert.AreEqual(t, "test123, test12"); //with space
		     Assert.AreNotEqual(t, "test123,test12"); // without space
		     Assert.AreNotEqual(t, "test123;test12");
		     Assert.AreNotEqual(t,null);
		 }
		 
		 [TestMethod]
		 [ExcludeFromCoverage]
		 public void ExifRead_GetExifDateTimeTest()
		 {
		     // Incomplete unit test 
		     // todo: fix this test
		     var dir2 = new ExifIfd0Directory();
		     dir2.Set(IptcDirectory.TagDigitalDateCreated, "20101212");
		     dir2.Set(IptcDirectory.TagDigitalTimeCreated, "124135+0000");
		     dir2.Set(ExifDirectoryBase.TagDateTimeDigitized, "2010:12:12 12:41:35");
		     dir2.Set(ExifDirectoryBase.TagDateTimeOriginal, "2010:12:12 12:41:35");
		     dir2.Set(ExifDirectoryBase.TagDateTime, "2010:12:12 12:41:35");

		     var iStorage = new FakeIStorage();
		     var t = new ReadMetaExif(null).GetExifDateTime(dir2);

		     var date2 = new DateTime(2010, 12, 12, 12, 41, 35);
		     var date = new DateTime();
		     Assert.AreEqual(
		         date, t);
		     Assert.AreNotEqual(t,null);
		     Assert.AreNotEqual(t,date2);

		 }



		 [TestMethod]
		 public void ExifRead_ReadExifFromFileTest()
		 {
		     var newImage = CreateAnImage.Bytes;
		     var fakeStorage = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.jpg"},new List<byte[]>{newImage});
		     
		     var item = new ReadMetaExif(fakeStorage).ReadExifFromFile("/test.jpg");
		     
		     Assert.AreEqual(ColorClassParser.Color.None, item.ColorClass);
		     Assert.AreEqual("caption", item.Description );
		     Assert.AreEqual(false,item.IsDirectory );
		     Assert.AreEqual("test, sion", item.Tags);
		     Assert.AreEqual("title", item.Title);
		     Assert.AreEqual(52.308205555500003, item.Latitude, 0.000001);
		     Assert.AreEqual(6.1935555554999997, item.Longitude,  0.000001);
		     Assert.AreEqual(2, item.ImageHeight);
		     Assert.AreEqual(3,item.ImageWidth);
		     Assert.AreEqual("Diepenveen", item.LocationCity);
		     Assert.AreEqual( "Overijssel", item.LocationState);
		     Assert.AreEqual( "Nederland",item.LocationCountry);
		     Assert.AreEqual( 6,item.LocationAltitude);
		     Assert.AreEqual(100, item.FocalLength);
		     Assert.AreEqual(new DateTime(2018,04,22,16,14,54), item.DateTime);
		 }
		 
		 [TestMethod]
		 public void ExifRead_ReadExif_FromPngInFileXMP_FileTest()
		 {
		     var newImage = CreateAnPng.Bytes;
		     var fakeStorage = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.png"},new List<byte[]>{newImage});
		     
		     var item = new ReadMetaExif(fakeStorage).ReadExifFromFile("/test.png");

		     Assert.AreEqual(ColorClassParser.Color.SuperiorAlt, item.ColorClass);
		     Assert.AreEqual("Description", item.Description );
		     Assert.AreEqual(false,item.IsDirectory );
		     Assert.AreEqual("tags", item.Tags);
		     Assert.AreEqual("title", item.Title);
		     Assert.AreEqual(35.0379999999, item.Latitude, 0.000001);
		     Assert.AreEqual(-81.0520000001, item.Longitude,  0.000001);
		     Assert.AreEqual(1, item.ImageHeight);
		     Assert.AreEqual(1,item.ImageWidth);
		     Assert.AreEqual("City", item.LocationCity);
		     Assert.AreEqual( "State", item.LocationState);
		     Assert.AreEqual( "Country",item.LocationCountry);
		     Assert.AreEqual( 10,item.LocationAltitude);
		     Assert.AreEqual(80, item.FocalLength);
		     Assert.AreEqual(new DateTime(2022,06,12,10,45,31), item.DateTime);
		 }
		 
		 [TestMethod]
		 public void ExifRead_GetImageWidthHeight_returnNothing()
		 {
		     var iStorage = new FakeIStorage();
		     var directory = new List<Directory> {BuildDirectory(new List<object>())};
		     var returnNothing = new ReadMetaExif(null).GetImageWidthHeight(directory,true);
		     Assert.AreEqual(returnNothing,0);
		     
		     var returnNothingFalse = new ReadMetaExif(null).GetImageWidthHeight(directory,false);
		     Assert.AreEqual(returnNothingFalse,0);
		 }

		 [TestMethod]
		 public void ExifRead_ReadExif_FromQuickTimeMp4InFileXMP_FileTest()
		 {
		     var newImage = CreateAnQuickTimeMp4.Bytes;
		     var fakeStorage = new FakeIStorage(new List<string> {"/"},
		         new List<string> {"/test.mp4"}, new List<byte[]> {newImage});

		     var item = new ReadMetaExif(fakeStorage).ReadExifFromFile("/test.mp4");

		     var date = new DateTime(2020, 03, 29, 13, 10, 07, DateTimeKind.Utc).ToLocalTime();
		     Assert.AreEqual(date, item.DateTime);
		     Assert.AreEqual(20, item.ImageWidth);
		     Assert.AreEqual(20, item.ImageHeight);
		     Assert.AreEqual(false,item.IsDirectory );
		 }
		 
		 [TestMethod]
		 public void ExifRead_ReadExif_FromQuickTimeMp4InFileXMP_FileTest_BytesWithLocation()
		 {
			 var newImage = CreateAnQuickTimeMp4.BytesWithLocation;
			 var fakeStorage = new FakeIStorage(new List<string> {"/"},
				 new List<string> {"/test.mp4"}, new List<byte[]> {newImage});

			 var item = new ReadMetaExif(fakeStorage).ReadExifFromFile("/test.mp4");

			 var date = new DateTime(2020, 04, 02, 17, 04, 02, DateTimeKind.Utc).ToLocalTime();
			 Assert.AreEqual(date, item.DateTime);
			 Assert.AreEqual(360, item.ImageHeight);
			 Assert.AreEqual(640, item.ImageWidth);
			 
			 Assert.AreEqual(51.72969618055556, item.Latitude,0.0001);
			 Assert.AreEqual(5.417600368923611, item.Longitude,0.0001);
			 Assert.AreEqual(false,item.IsDirectory );

			 // not supported yet
			 // Assert.AreEqual("Apple", item.Make);
			 // Assert.AreEqual("iPhone SE", item.Model);
		 } 

		 // https://github.com/drewnoakes/metadata-extractor-dotnet/blob/master/MetadataExtractor.Tests/DirectoryExtensionsTest.cs
		 private static Directory BuildDirectory(IEnumerable<object> values)
		 {
		     var directory = new MockDirectory();

		     foreach (var pair in Enumerable.Range(1, int.MaxValue).Zip(values, Tuple.Create))
		         directory.Set(pair.Item1, pair.Item2);

		     return directory;
		 }
	}
}
