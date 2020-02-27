using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Attributes;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using Directory = MetadataExtractor.Directory;


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
             Assert.AreEqual(t, null);
         }

         [TestMethod]
         [ExcludeFromCoverage]
         public void ExifRead_GetObjectNameTest()
         {
	         var iStorage = new FakeIStorage();
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
	        var iStorage = new FakeIStorage();
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
	         var iStorage = new FakeIStorage();
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
	         var iStorage = new FakeIStorage();
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
             
             Assert.AreEqual(FileIndexItem.Color.None, item.ColorClass);
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
         }
         
         // [TestMethod]
         public void ExifRead_ReadExifFromPngFileTest()
         {
	         var newImage = CreateAnPng.Bytes;
	         var fakeStorage = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.png"},new List<byte[]>{newImage});
	         
	         var item = new ReadMetaExif(fakeStorage).ReadExifFromFile("/test.png");
	         // var item = new ReadMetaExif(new StorageHostFullPathFilesystem()).ReadExifFromFile("/data/scripts/__starsky/01-dif/FF4D00-0.8.png");

	         Assert.AreEqual(FileIndexItem.Color.None, item.ColorClass);
	         Assert.AreEqual("description-orange3", item.Description );
	         Assert.AreEqual(false,item.IsDirectory );
	         Assert.AreEqual("keyword1, keyword2", item.Tags);
	         Assert.AreEqual("object name1 ,t1", item.Title);
	         Assert.AreEqual(45.56025, item.Latitude, 0.000001);
	         Assert.AreEqual(-122.6610833334, item.Longitude,  0.000001);
	         Assert.AreEqual(1, item.ImageHeight);
	         Assert.AreEqual(1,item.ImageWidth);
	         Assert.AreEqual("City", item.LocationCity);
	         Assert.AreEqual( "State", item.LocationState);
	         Assert.AreEqual( "Country",item.LocationCountry);
	         Assert.AreEqual( 0,item.LocationAltitude);
	         Assert.AreEqual(0, item.FocalLength);
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

         // https://github.com/drewnoakes/metadata-extractor-dotnet/blob/master/MetadataExtractor.Tests/DirectoryExtensionsTest.cs
         private static Directory BuildDirectory(IEnumerable<object> values)
         {
             var directory = new MockDirectory();

             foreach (var pair in Enumerable.Range(1, int.MaxValue).Zip(values, Tuple.Create))
                 directory.Set(pair.Item1, pair.Item2);

             return directory;
         }

//         [TestMethod]
//         public void ExifRead_GetOrientationTest()
//         {
//             var subDir =  new ExifIfd0Directory();
//             subDir.Set(ExifDirectoryBase.TagOrientation, 9999);
//             var directory = BuildDirectory(new List<ExifIfd0Directory> {subDir});
//             var returnNothingFalse = new ReadMeta().GetOrientation(directory);
//             Console.WriteLine();
//         }


     }
 }
