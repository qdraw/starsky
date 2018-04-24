using System;
using System.Collections.Generic;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Services;

namespace starskyTest
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
         public void GetObjectName1()
         {
             var dir = new IptcDirectory();
             dir.Set(IptcDirectory.TagObjectName, "test" );
             var t = ExifRead.GetObjectName(dir);
             Assert.AreEqual(t, null);
         }
         
     }
 }