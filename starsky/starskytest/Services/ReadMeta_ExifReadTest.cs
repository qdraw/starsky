using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.Xmp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Helpers;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starskycore.Attributes;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using XmpCore.Impl;

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

		public MockDirectory(Dictionary<int, string> tagNameMap) : base(tagNameMap)
		{
		}
	}

	[TestClass]
	public class ExifReadTest
	{

		[TestMethod]
		[ExcludeFromCoverage]
		public void ExifRead_GetObjectNameNull()
		{
			var t = ReadMetaExif.GetObjectName(new MockDirectory(null));
			Assert.AreEqual( string.Empty,t);
		}

		[TestMethod]
		[ExcludeFromCoverage]
		public void ExifRead_GetObjectNameTest()
		{
			var dir = new IptcDirectory();
			dir.Set(IptcDirectory.TagObjectName, "test" );
			var t = ReadMetaExif.GetObjectName(dir);
			Assert.AreEqual("test",t);
		}

		[TestMethod]
		[ExcludeFromCoverage]
		public void ExifRead_GetCaptionAbstractTest()
		{
			var dir = new IptcDirectory();
			dir.Set(IptcDirectory.TagCaption, "test123");
			var t = ReadMetaExif.GetCaptionAbstract(dir);
			Assert.AreEqual("test123", t);
		}
		 
		[TestMethod]
		[ExcludeFromCoverage]
		public void ExifRead_GetExifKeywordsSingleTest()
		{
			var dir = new IptcDirectory();
			dir.Set(IptcDirectory.TagKeywords, "test123");
			var t = ReadMetaExif.GetExifKeywords(dir);

			Assert.AreEqual("test123", t);
		}
		 
		[TestMethod]
		[ExcludeFromCoverage]
		public void ExifRead_GetExifKeywordsMultipleTest()
		{
			var dir = new IptcDirectory();
			dir.Set(IptcDirectory.TagKeywords, "test123;test12");
			var t = ReadMetaExif.GetExifKeywords(dir);
			Assert.AreEqual("test123, test12",t); //with space
		}
		 
		[TestMethod]
		[ExcludeFromCoverage]
		public void ExifRead_GetExifDateTimeTest()
		{
			var container = new List<Directory>();
			var dir2 = new ExifSubIfdDirectory();
			dir2.Set(IptcDirectory.TagDigitalDateCreated, "20101212");
			dir2.Set(IptcDirectory.TagDigitalTimeCreated, "124135+0000");
			dir2.Set(ExifDirectoryBase.TagDateTimeDigitized, "2010:12:12 12:41:35");
			dir2.Set(ExifDirectoryBase.TagDateTimeOriginal, "2010:12:12 12:41:35");
			dir2.Set(ExifDirectoryBase.TagDateTime, "2010:12:12 12:41:35");
			container.Add(dir2);
			
			var result = new ReadMetaExif(null).GetExifDateTime(container);
			var expectedExifDateTime = new DateTime(2010, 12, 12, 12, 41, 35);
			
			Assert.AreEqual(expectedExifDateTime, result);
		}
		
				 
		[TestMethod]
		[ExcludeFromCoverage]
		public void ExifRead_GetExifDateTimeTest_TagDateTimeOriginal()
		{
			var container = new List<Directory>();
			var dir2 = new ExifSubIfdDirectory();
			dir2.Set(ExifDirectoryBase.TagDateTimeOriginal, "2010:12:12 12:41:35");
			container.Add(dir2);
			
			var result = new ReadMetaExif(null).GetExifDateTime(container);
			var expectedExifDateTime = new DateTime(2010, 12, 12, 12, 41, 35);
			
			Assert.AreEqual(expectedExifDateTime, result);
		}
		
		[TestMethod]
		[ExcludeFromCoverage]
		public void ExifRead_GetExifDateTimeTest_QuickTimeMovieHeaderDirectory_SetUtc()
		{
			var orgCulture =
				CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
			CultureInfo.CurrentCulture = new CultureInfo("EN-us");
			
			var container = new List<Directory>();
			var dir2 = new QuickTimeMovieHeaderDirectory();
			dir2.Set(QuickTimeMovieHeaderDirectory.TagCreated, "Tue Oct 11 09:40:04 2011");
			container.Add(dir2);
			
			var result = new ReadMetaExif(null, new AppSettings{ VideoUseLocalTime = new List<CameraMakeModel>
			{
				new CameraMakeModel("test","test")
			},
				CameraTimeZone = "Europe/London"
			}).GetExifDateTime(container, new CameraMakeModel("test","test"));
			
			var expectedExifDateTime = new DateTime(2011, 10, 11, 9, 40, 4);
			
			Assert.AreEqual(expectedExifDateTime, result);
			
			CultureInfo.CurrentCulture = new CultureInfo(orgCulture);
		}
		
				
		[TestMethod]
		[ExcludeFromCoverage]
		public void ExifRead_GetExifDateTimeTest_QuickTimeMovieHeaderDirectory_BrandOnly()
		{
			var orgCulture =
				CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
			CultureInfo.CurrentCulture = new CultureInfo("EN-us");
			
			var container = new List<Directory>();
			var dir2 = new QuickTimeMovieHeaderDirectory();
			dir2.Set(QuickTimeMovieHeaderDirectory.TagCreated, "Tue Oct 11 09:40:04 2011");
			container.Add(dir2);
			
			var result = new ReadMetaExif(null, new AppSettings{ VideoUseLocalTime = new List<CameraMakeModel>
				{
					new CameraMakeModel("test", string.Empty)
				},
				CameraTimeZone = "Europe/London"
			}).GetExifDateTime(container, new CameraMakeModel("test","test"));
			
			var expectedExifDateTime = new DateTime(2011, 10, 11, 9, 40, 4);
			
			Assert.AreEqual(expectedExifDateTime, result);
			
			CultureInfo.CurrentCulture = new CultureInfo(orgCulture);
		}
		
		[TestMethod]
		[ExcludeFromCoverage]
		public void ExifRead_GetExifDateTimeTest_QuickTimeMovieHeaderDirectory_AssumeLocal()
		{
			var orgCulture =
				CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
			CultureInfo.CurrentCulture = new CultureInfo("EN-us");
			
			var container = new List<Directory>();
			var dir2 = new QuickTimeMovieHeaderDirectory();
			dir2.Set(QuickTimeMovieHeaderDirectory.TagCreated, "Tue Oct 11 09:40:04 2011");
			container.Add(dir2);
			
			var result = new ReadMetaExif(null, new AppSettings{ VideoUseLocalTime = new List<CameraMakeModel>
				{
					new CameraMakeModel("Apple", string.Empty)
				},
				CameraTimeZone = "Europe/London"
			}).GetExifDateTime(container);
			
			CultureInfo.CurrentCulture = new CultureInfo(orgCulture);

			var expectedExifDateTime = new DateTime(2011, 10, 11, 10, 40, 4);
			
			Assert.AreEqual(expectedExifDateTime, result);
		}
		
		[TestMethod]
		[ExcludeFromCoverage]
		public void ExifRead_GetExifDateTimeTest_GetXmpData()
		{
			var container = new List<Directory>();
			var dir2 = new XmpDirectory();
			dir2.SetXmpMeta(new XmpMeta());
			if ( dir2.XmpMeta == null )
			{
				throw new NullReferenceException(
					"ExifRead_GetExifDateTimeTest_GetXmpData xmpMeta Field");
			}
			
			dir2.XmpMeta.SetProperty("http://ns.adobe.com/photoshop/1.0/", "photoshop:DateCreated","2020-03-14T14:00:51" );
			container.Add(dir2);
			
			var result = new ReadMetaExif(null).GetExifDateTime(container);
			var expectedExifDateTime = new DateTime(2020, 3, 14, 14, 0, 51);
			
			Assert.AreEqual(expectedExifDateTime, result);
		}

		[TestMethod]
		public void ParseSubIfdDateTime_NotInFirstContainer_TagDateTimeOriginal()
		{
			var container = new List<Directory>();
			
			// for raw the first container does not contain dates
			var dir1 = new ExifSubIfdDirectory();
			container.Add(dir1);

			var dir2 = new ExifSubIfdDirectory();
			dir2.Set(ExifDirectoryBase.TagDateTimeOriginal, "2022:02:02 20:22:02");
			container.Add(dir2);
			var provider = CultureInfo.InvariantCulture;

			var result = ReadMetaExif.ParseSubIfdDateTime(container, provider);
			Assert.AreEqual(new DateTime(2022,02,02,20,22,02),result);
		}
		
		[TestMethod]
		public void ParseSubIfdDateTime_NotInFirstContainer_TagDateTimeDigitized()
		{
			var container = new List<Directory>();
			
			// for raw the first container does not contain dates
			var dir1 = new ExifSubIfdDirectory();
			container.Add(dir1);

			var dir2 = new ExifSubIfdDirectory();
			dir2.Set(ExifDirectoryBase.TagDateTimeDigitized, "2022:02:02 20:22:02");
			container.Add(dir2);
			var provider = CultureInfo.InvariantCulture;

			var result = ReadMetaExif.ParseSubIfdDateTime(container, provider);
			Assert.AreEqual(new DateTime(2022,02,02,20,22,02),result);
		}
		
		[TestMethod]
		public void ParseSubIfdDateTime_NonValidDate()
		{
			var container = new List<Directory>();
			var dir1 = new ExifSubIfdDirectory();
			dir1.Set(ExifDirectoryBase.TagDateTimeOriginal, "test_not_valid_date");
			dir1.Set(ExifDirectoryBase.TagDateTimeDigitized, "test_not_valid_date");

			container.Add(dir1);
			
			var provider = CultureInfo.InvariantCulture;

			var result = ReadMetaExif.ParseSubIfdDateTime(container, provider);
			Assert.AreEqual(new DateTime(),result);
		}
		
		[TestMethod]
		public void ParseSubIfdDateTime_Nothing()
		{
			var container = new List<Directory>();
			var dir1 = new ExifSubIfdDirectory();
			container.Add(dir1);
			
			var provider = CultureInfo.InvariantCulture;

			var result = ReadMetaExif.ParseSubIfdDateTime(container, provider);
			Assert.AreEqual(new DateTime(),result);
		}

		[TestMethod]
		public void ExifRead_ReadExifFromFileTest()
		{
			var newImage = CreateAnImage.Bytes;
			var fakeStorage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg"},new List<byte[]>{newImage});
		     
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
		     
			Assert.AreEqual( "Sony|SLT-A58|24-105mm F3.5-4.5", item.MakeModel);
			Assert.AreEqual( "Sony", item.Make);
			Assert.AreEqual( "SLT-A58", item.Model);
			Assert.AreEqual( "24-105mm F3.5-4.5", item.LensModel);
			Assert.AreEqual( ImageStabilisationType.Unknown, item.ImageStabilisation);

		}
		
		[TestMethod]
		public void ImageStabilisationOn()
		{
			var newImage = CreateAnImageA6600.Bytes;
			var fakeStorage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg"},new List<byte[]>{newImage});
			 
			var item = new ReadMetaExif(fakeStorage).ReadExifFromFile("/test.jpg");
			Assert.AreEqual( ImageStabilisationType.On, item.ImageStabilisation);
		}
		
		[TestMethod]
		public void ImageStabilisationOff()
		{
			var newImage = CreateAnImageA58Tamron.Bytes;
			var fakeStorage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg"},new List<byte[]>{newImage});
			 
			var item = new ReadMetaExif(fakeStorage).ReadExifFromFile("/test.jpg");
			Assert.AreEqual( ImageStabilisationType.Off, item.ImageStabilisation);
		}
				
		[TestMethod]
		public void LensModelTamRon()
		{
			var newImage = CreateAnImageA58Tamron.Bytes;
			var fakeStorage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg"},new List<byte[]>{newImage});
			 
			var item = new ReadMetaExif(fakeStorage).ReadExifFromFile("/test.jpg");
			Assert.AreEqual( "Tamron or Sigma Lens", item.LensModel);
		}

		[TestMethod]
		public void ExifRead_ReadExifFromFileTest_DeletedTag()
		{
			var newImage = CreateAnImageStatusDeleted.Bytes;
			var fakeStorage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg"},new List<byte[]>{newImage});
		     
			var item = new ReadMetaExif(fakeStorage).ReadExifFromFile("/test.jpg");
			Assert.AreEqual("!delete!", item.Tags);
		}

		[TestMethod]
		public void ExifRead_ReadExif_FromPngInFileXMP_FileTest()
		{
			var newImage = CreateAnPng.Bytes;
			var fakeStorage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.png"},new List<byte[]>{newImage});
		     
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
			var directory = new List<Directory> {BuildDirectory(new List<object>())};
			var returnNothing = ReadMetaExif.GetImageWidthHeight(directory,true);
			Assert.AreEqual(0,returnNothing);
		     
			var returnNothingFalse = ReadMetaExif.GetImageWidthHeight(directory,false);
			Assert.AreEqual(0,returnNothingFalse);
		}

		[TestMethod]
		public void ExifRead_ReadExif_FromQuickTimeMp4InFileXMP_FileTest()
		{
			// CultureInfo.CurrentCulture = new CultureInfo("Nl-nl");
			
			var newImage = CreateAnQuickTimeMp4.Bytes;
			var fakeStorage = new FakeIStorage(new List<string> {"/"},
				new List<string> {"/test.mp4"}, new List<byte[]> {newImage});

			var item = new ReadMetaExif(fakeStorage, new AppSettings{VideoUseLocalTime = new List<CameraMakeModel>
			{
				new CameraMakeModel("Apple","MacbookPro15,1")
			}}).ReadExifFromFile("/test.mp4");

			var date = new DateTime(2020, 03, 29, 13, 10, 07);
			Assert.AreEqual(date, item.DateTime);
			Assert.AreEqual(20, item.ImageWidth);
			Assert.AreEqual(20, item.ImageHeight);
			Assert.AreEqual(false,item.IsDirectory );
		}
		
		[TestMethod]
		public void ExifRead_ReadExif_FromQuickTimeMp4InFileXMP_FileTest_DutchCulture()
		{
			var currentCultureThreeLetterIsoLanguageName = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
			CultureInfo.CurrentCulture = new CultureInfo("NL-nl");
			
			var newImage = CreateAnQuickTimeMp4.Bytes;
			var fakeStorage = new FakeIStorage(new List<string> {"/"},
				new List<string> {"/test.mp4"}, new List<byte[]> {newImage});

			var item = new ReadMetaExif(fakeStorage, new AppSettings{VideoUseLocalTime = new List<CameraMakeModel>
			{
				new CameraMakeModel("Apple","MacbookPro15,1")
			}}).ReadExifFromFile("/test.mp4");

			var date = new DateTime(2020, 03, 29, 13, 10, 07);
			Assert.AreEqual(date, item.DateTime);
			Assert.AreEqual(20, item.ImageWidth);
			Assert.AreEqual(20, item.ImageHeight);
			Assert.AreEqual(false,item.IsDirectory );
			
			CultureInfo.CurrentCulture = new CultureInfo(currentCultureThreeLetterIsoLanguageName);
		}

		[TestMethod]
		public void ExifRead_ParseQuickTimeDateTime_AssumeUtc_CameraTimeZoneMissing()
		{
			var currentCultureThreeLetterIsoLanguageName = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
			CultureInfo.CurrentCulture = new CultureInfo("EN-us");
			
			// CameraTimeZone = "Europe/London" is missing

			var fakeStorage = new FakeIStorage();
			
			var item = new ReadMetaExif(fakeStorage);

			var dir = new QuickTimeMovieHeaderDirectory();
			dir.Set(QuickTimeMovieHeaderDirectory.TagCreated, "Tue Oct 11 09:40:04 2011" );
			
			var result = item.ParseQuickTimeDateTime(new CameraMakeModel(),
				new List<Directory>{dir}, CultureInfo.InvariantCulture);
		
			var expectedExifDateTime = new DateTime(2011, 10, 11, 9, 40, 4);
			CultureInfo.CurrentCulture = new CultureInfo(currentCultureThreeLetterIsoLanguageName);

			Assert.AreEqual(expectedExifDateTime, result);
		}
		
		[TestMethod]
		public void ExifRead_ParseQuickTimeDateTime_UseLocalTime()
		{
			var currentCultureThreeLetterIsoLanguageName = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
			CultureInfo.CurrentCulture = new CultureInfo("EN-us");
			
			var fakeStorage = new FakeIStorage();
			var item = new ReadMetaExif(fakeStorage, new AppSettings
			{
				VideoUseLocalTime = new List<CameraMakeModel>{new CameraMakeModel("test","test")},
				CameraTimeZone = "Europe/London"
			});

			var dir = new QuickTimeMovieHeaderDirectory();
			dir.Set(QuickTimeMovieHeaderDirectory.TagCreated, "Tue Oct 11 09:40:04 2011" );
			
			var result = item.ParseQuickTimeDateTime(new CameraMakeModel("test","test"),
				new List<Directory>{dir}, CultureInfo.InvariantCulture);
		
			var expectedExifDateTime = new DateTime(2011, 10, 11, 9, 40, 4);
			CultureInfo.CurrentCulture = new CultureInfo(currentCultureThreeLetterIsoLanguageName);

			Assert.AreEqual(expectedExifDateTime, result);
		}
		
		[TestMethod]
		public void ExifRead_ParseQuickTimeDateTime_UseLocalTime1_WithTimeZone()
		{
			var currentCultureThreeLetterIsoLanguageName = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
			CultureInfo.CurrentCulture = new CultureInfo("EN-us");
			
			var fakeStorage = new FakeIStorage();
			var item = new ReadMetaExif(fakeStorage, new AppSettings
			{
				VideoUseLocalTime = new List<CameraMakeModel>{},
				CameraTimeZone = "Europe/London"
			});

			var dir = new QuickTimeMovieHeaderDirectory();
			dir.Set(QuickTimeMovieHeaderDirectory.TagCreated, "Tue Oct 11 09:40:04 2011" );
			
			var result = item.ParseQuickTimeDateTime(new CameraMakeModel("test","test"),
				new List<Directory>{dir}, CultureInfo.InvariantCulture);
		
			var expectedExifDateTime = new DateTime(2011, 10, 11, 10, 40, 4);

			CultureInfo.CurrentCulture = new CultureInfo(currentCultureThreeLetterIsoLanguageName);

			Assert.AreEqual(expectedExifDateTime, result);
		}
		
		[TestMethod]
		public void ExifRead_ParseQuickTimeDateTime_UseLocalTime_WithTimeZone_Wrong()
		{
			var currentCultureThreeLetterIsoLanguageName = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
			CultureInfo.CurrentCulture = new CultureInfo("EN-us");

			var fakeStorage = new FakeIStorage();
			var item = new ReadMetaExif(fakeStorage, new AppSettings
			{
				VideoUseLocalTime = new List<CameraMakeModel>{},
				CameraTimeZone = ""
			});

			var dir = new QuickTimeMovieHeaderDirectory();
			dir.Set(QuickTimeMovieHeaderDirectory.TagCreated, "Tue Oct 11 09:40:04 2011" );
			
			var result = item.ParseQuickTimeDateTime(new CameraMakeModel("test","test"),
				new List<Directory>{dir}, CultureInfo.InvariantCulture);
		
			var expectedExifDateTime = new DateTime(2011, 10, 11, 9, 40, 4).ToLocalTime();
			CultureInfo.CurrentCulture = new CultureInfo(currentCultureThreeLetterIsoLanguageName);

			Assert.AreEqual(expectedExifDateTime, result);
		}
		
		[TestMethod]
		public void ExifRead_ParseQuickTimeDateTime_NoVideoUsedSet()
		{
			var currentCultureThreeLetterIsoLanguageName = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
			CultureInfo.CurrentCulture = new CultureInfo("EN-us");

			var fakeStorage = new FakeIStorage();
			var item = new ReadMetaExif(fakeStorage, new AppSettings
			{
				VideoUseLocalTime = null,
				CameraTimeZone = ""
			});

			var dir = new QuickTimeMovieHeaderDirectory();
			dir.Set(QuickTimeMovieHeaderDirectory.TagCreated, "Tue Oct 11 09:40:04 2011" );
			
			var result = item.ParseQuickTimeDateTime(new CameraMakeModel("test","test"),
				new List<Directory>{dir}, CultureInfo.InvariantCulture);
		
			var expectedExifDateTime = new DateTime(2011, 10, 11, 9, 40, 4).ToLocalTime();
			CultureInfo.CurrentCulture = new CultureInfo(currentCultureThreeLetterIsoLanguageName);

			Assert.AreEqual(expectedExifDateTime, result);
		}

		[TestMethod]
		public void ExifRead_ReadExif_FromQuickTimeMp4InFileXMP_WithLocation_FileTest_DutchCulture()
		{
			var currentCultureThreeLetterIsoLanguageName = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
			CultureInfo.CurrentCulture = new CultureInfo("NL-nl");
			
			var newImage = CreateAnQuickTimeMp4.BytesWithLocation;
			var fakeStorage = new FakeIStorage(new List<string> {"/"},
				new List<string> {"/test.mp4"}, new List<byte[]> {newImage});

			var item = new ReadMetaExif(fakeStorage).ReadExifFromFile("/test.mp4");

			var date = new DateTime(2020, 04, 04, 12, 50, 19, DateTimeKind.Local).ToLocalTime();
			Assert.AreEqual(date, item.DateTime);
			Assert.AreEqual(640, item.ImageWidth);
			Assert.AreEqual(360, item.ImageHeight);
			 
			Assert.AreEqual(52.23829861111111, item.Latitude,0.001);
			Assert.AreEqual(6.025800238715278, item.Longitude,0.001);

			Assert.AreEqual(false,item.IsDirectory );
			CultureInfo.CurrentCulture =
				new CultureInfo(currentCultureThreeLetterIsoLanguageName);
		}
		
		[TestMethod]
		public void ExifRead_ReadExif_FromQuickTimeMp4InFileXMP_WithLocation_FileTest()
		{
			var newImage = CreateAnQuickTimeMp4.BytesWithLocation;
			var fakeStorage = new FakeIStorage(new List<string> {"/"},
				new List<string> {"/test.mp4"}, new List<byte[]> {newImage});

			var item = new ReadMetaExif(fakeStorage).ReadExifFromFile("/test.mp4");

			var date = new DateTime(2020, 04, 04, 12, 50, 19, DateTimeKind.Local).ToLocalTime();
			Assert.AreEqual(date, item.DateTime);
			Assert.AreEqual(640, item.ImageWidth);
			Assert.AreEqual(360, item.ImageHeight);
			 
			Assert.AreEqual(52.23829861111111, item.Latitude,0.001);
			Assert.AreEqual(6.025800238715278, item.Longitude,0.001);

			Assert.AreEqual(false,item.IsDirectory );
		}

		[TestMethod]
		public void ExifRead_DataParsingCorruptFailsData()
		{
			var newImage = CreateAnPng.Bytes.Take(200).ToArray(); // corrupt
			var fakeStorage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.png"},new List<byte[]>{newImage});
		     
			var item = new ReadMetaExif(fakeStorage).ReadExifFromFile("/test.png");
			 
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, item.Status);
			Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, item.ImageFormat);
		}

		[TestMethod]
		public void ExifRead_DataParsingCorruptStreamNull()
		{
			var fakeStorage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.png"},new List<byte[]>{null});
			var item = new ReadMetaExif(fakeStorage).ReadExifFromFile("/test.png");
			// streamNull
			 
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, item.Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, item.Status);
		}

		// https://github.com/drewnoakes/metadata-extractor-dotnet/blob/master/MetadataExtractor.Tests/DirectoryExtensionsTest.cs
		private static Directory BuildDirectory(IEnumerable<object> values)
		{
			var directory = new MockDirectory(null);

			foreach (var pair in Enumerable.Range(1, int.MaxValue).Zip(values, Tuple.Create))
				directory.Set(pair.Item1, pair.Item2);

			return directory;
		}
	}
}
