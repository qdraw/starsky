using System;
using System.Collections.Generic;
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
	         var iStorage = new FakeIStorage();
             var t = new ReadMeta(iStorage).GetObjectName(new MockDirectory());
             Assert.AreEqual(t, null);
         }

         [TestMethod]
         [ExcludeFromCoverage]
         public void ExifRead_GetObjectNameTest()
         {
	         var iStorage = new FakeIStorage();
             var dir = new IptcDirectory();
             dir.Set(IptcDirectory.TagObjectName, "test" );
             var t = new ReadMeta(iStorage).GetObjectName(dir);
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
            var t = new ReadMeta(iStorage).GetCaptionAbstract(dir);
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
             var t = new ReadMeta(iStorage).GetExifKeywords(dir);
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
             var t = new ReadMeta(iStorage).GetExifKeywords(dir);
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
             var t = new ReadMeta(iStorage).GetExifDateTime(dir2);

             var date2 = new DateTime(2010, 12, 12, 12, 41, 35);
             var date = new DateTime();
             Assert.AreEqual(
                 date, t);
             Assert.AreNotEqual(t,null);
             Assert.AreNotEqual(t,date2);

         }


         
         [TestMethod]
         public void ExifRead_ParseGpsTest()
         {
	         var iStorage = new FakeIStorage();

	         var latitude = new ReadMeta(iStorage).ConvertDegreeMinutesSecondsToDouble("52° 18' 29.54\"", "N");
             Assert.AreEqual(latitude,  52.308205555500003, 0.000001);
             
             var longitude = new ReadMeta(iStorage).ConvertDegreeMinutesSecondsToDouble("6° 11' 36.8\"", "E");
             Assert.AreEqual(longitude,  6.1935555554999997, 0.000001);

         }
         
         [TestMethod]
         public void ExifRead_ReadExifFromFileTest()
         {
             var newImage = new CreateAnImage();
	         var iStorage = new FakeIStorage();
             var item = new ReadMeta(iStorage).ReadExifFromFile(newImage.FullFilePath);
             
             Assert.AreEqual(item.ColorClass,FileIndexItem.Color.None);
             Assert.AreEqual(item.Description, "caption");
             Assert.AreEqual(item.IsDirectory, false);
             Assert.AreEqual(item.Tags, "test, sion");
             Assert.AreEqual(item.Title, "title");
             Assert.AreEqual(item.Latitude,  52.308205555500003, 0.000001);
             Assert.AreEqual(item.Longitude, 6.1935555554999997, 0.000001);
             Assert.AreEqual(item.ImageHeight, 2);
             Assert.AreEqual(item.ImageWidth, 3);
             Assert.AreEqual(item.LocationCity, "Diepenveen");
             Assert.AreEqual(item.LocationState, "Overijssel");
             Assert.AreEqual(item.LocationCountry, "Nederland");
             Assert.AreEqual(item.LocationAltitude, 6);

         }
         
         
         
         [TestMethod]
         public void ExifRead_ConvertDegreeMinutesToDouble_ConvertLongLat()
         {
	         var iStorage = new FakeIStorage();

             var input = "52,20.708N";
             string refGps = input.Substring(input.Length-1, 1);
             var data = new ReadMeta(iStorage).ConvertDegreeMinutesToDouble(input, refGps);
             Assert.AreEqual(52.3451333333,data,0.001);

            
             var input1 = "5,55.840E";
             string refGps1 = input1.Substring(input1.Length-1, 1);
             var data1 = new ReadMeta(iStorage).ConvertDegreeMinutesToDouble(input1, refGps1);
             Assert.AreEqual(5.930,data1,0.001);

         }

         [TestMethod]
         public void ExifRead_GetImageWidthHeight_returnNothing()
         {
	         var iStorage = new FakeIStorage();
             var directory = new List<Directory> {BuildDirectory(new List<object>())};
             var returnNothing = new ReadMeta(iStorage).GetImageWidthHeight(directory,true);
             Assert.AreEqual(returnNothing,0);
             
             var returnNothingFalse = new ReadMeta(iStorage).GetImageWidthHeight(directory,false);
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
