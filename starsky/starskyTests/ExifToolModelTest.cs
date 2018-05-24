using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;

namespace starskytests
{
    [TestClass]
    public class ExifToolModelTest
    {
        [TestMethod]
        public void ExifToolModelColorClassTest()
        {
            var exifToolModel = new ExifToolModel{ColorClass = FileIndexItem.Color.Winner};
            Assert.AreEqual(exifToolModel.ColorClass,FileIndexItem.Color.Winner);
        }

        [TestMethod]
        public void ExifToolCaptionAbstractTest()
        {
            var exifToolModel = new ExifToolModel{CaptionAbstract = "test"};
            Assert.AreEqual(exifToolModel.CaptionAbstract,"test");
        }

        [TestMethod]
        public void ExifToolPrefsParseTest()
        {
            var exifToolModel = new ExifToolModel{Prefs = "Tagged:0, ColorClass:2, Rating:0, FrameNum:0"};
            Assert.AreEqual(exifToolModel.ColorClass,FileIndexItem.Color.WinnerAlt);
        }
        
        [TestMethod]
        public void ExifToolPrefsNullTest()
        {
            var exifToolModel = new ExifToolModel();
            Assert.AreEqual(exifToolModel.Prefs,null);
        }
        
        [TestMethod]
        public void ExifToolTagsKeywordsFirstTest()
        {
            var exifToolModel = new ExifToolModel{Tags = "Schiphol, airplane"};
            Assert.AreEqual(exifToolModel.Keywords.FirstOrDefault(),"Schiphol");
        }

        [TestMethod]
        public void ExifToolSetHashSet1Test()
        {
            var list = new List<string> {"Schiphol", "Schiphol"};
            var exifToolModel = new ExifToolModel{Keywords = list.ToHashSet()};
            Assert.AreEqual(exifToolModel.Keywords.Count,1);
        }
        
        [TestMethod]
        public void ExifTool_hashSetToStringTest()
        {
            // It is in memory stored as HashSet, not as string
            var exifToolModel = new ExifToolModel{Tags = "Schiphol, airplane"};
            Assert.AreEqual(exifToolModel.Tags,"Schiphol, airplane");
        }

        [TestMethod]
        public void ExifTool_hashSetToStringNullTest()
        {
            var exifToolModel = new ExifToolModel();
            Assert.AreEqual(exifToolModel.Tags,string.Empty);
        }
        
        [TestMethod]
        public void ExifToolKeywordsNullTest()
        {
            var exifToolModel = new ExifToolModel{Keywords = null};
            Assert.AreEqual(exifToolModel.Keywords.Count,0);
        }
    }
}