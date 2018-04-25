using System;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Services;

namespace starskytests
 {
     public class MockDirectory : Directory
     {
         public override string Name => string.Empty;

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
         public void GetObjectNameNull()
         {
             var t = ExifRead.GetObjectName(new MockDirectory());
             Assert.AreEqual(t, null);
         }

         [TestMethod]
         public void GetObjectNameTest()
         {
             var dir = new IptcDirectory();
             dir.Set(IptcDirectory.TagObjectName, "test" );
             var t = ExifRead.GetObjectName(dir);
             Assert.AreEqual(t, "test");
             Assert.AreNotEqual(t,null);
         }

        [TestMethod]
        public void GetCaptionAbstractTest()
        {
            var dir = new IptcDirectory();
            dir.Set(IptcDirectory.TagCaption, "test123");
            var t = ExifRead.GetCaptionAbstract(dir);
            Assert.AreEqual(t, "test123");
            Assert.AreNotEqual(t,string.Empty);
            Assert.AreNotEqual(t,null);
        }
         
         [TestMethod]
         public void GetExifKeywordsSingleTest()
         {
             var dir = new IptcDirectory();
             dir.Set(IptcDirectory.TagKeywords, "test123");
             var t = ExifRead.GetExifKeywords(dir);
             Assert.AreEqual(t, "test123");
             Assert.AreNotEqual(t,null);
         }
         
         [TestMethod]
         public void GetExifKeywordsMultipleTest()
         {
             var dir = new IptcDirectory();
             dir.Set(IptcDirectory.TagKeywords, "test123;test12");
             var t = ExifRead.GetExifKeywords(dir);
             Assert.AreEqual(t, "test123, test12"); //with space
             Assert.AreNotEqual(t, "test123,test12"); // without space
             Assert.AreNotEqual(t, "test123;test12");
             Assert.AreNotEqual(t,null);
         }
         
         [TestMethod]
         public void GetExifDateTimeTest()
         {
             // Incomplete unit test 
             // todo: fix this test
             var dir2 = new ExifIfd0Directory();
             dir2.Set(IptcDirectory.TagDigitalDateCreated, "20101212");
             dir2.Set(IptcDirectory.TagDigitalTimeCreated, "124135+0000");
             dir2.Set(ExifDirectoryBase.TagDateTimeDigitized, "2010:12:12 12:41:35");
             dir2.Set(ExifDirectoryBase.TagDateTimeOriginal, "2010:12:12 12:41:35");
             dir2.Set(ExifDirectoryBase.TagDateTime, "2010:12:12 12:41:35");
             
             var t = ExifRead.GetExifDateTime(dir2);

             var date2 = new DateTime(2010, 12, 12, 12, 41, 35);
             var date = new DateTime();
             Assert.AreEqual(
                 date, t);
             Assert.AreNotEqual(t,null);
             Assert.AreNotEqual(t,date2);

         }
         
         
     }
 }
