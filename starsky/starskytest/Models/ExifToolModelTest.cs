using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starskycore.Models;

namespace starskytest.Models
{
    [TestClass]
    public class ExifToolModelTest
    {
        [TestMethod]
        public void ExifToolModelColorClassTest()
        {
            var exifToolModel = new ExifToolModel{ColorClass = ColorClassParser.Color.Winner};
            Assert.AreEqual(exifToolModel.ColorClass,ColorClassParser.Color.Winner);
        }

        [TestMethod]
        public void ExifToolCAllDatesDateTimeTest()
        {
            var datetime = new DateTime();
            var exifToolModel = new ExifToolModel{AllDatesDateTime = datetime};
            Assert.AreEqual(datetime,exifToolModel.AllDatesDateTime);
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
            Assert.AreEqual(exifToolModel.ColorClass,ColorClassParser.Color.WinnerAlt);
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
        
        [TestMethod]
        public void ExifToolSubjectOverwrite()
        {
            var list = new List<string> {"Schiphol", "Schiphol"};
            var exifToolModel = new ExifToolModel{Subject = list.ToHashSet()};
            Assert.AreEqual(exifToolModel.Keywords.Count,1);
            Assert.AreEqual(0, exifToolModel.Subject.Count);
        }

        [TestMethod]
        public void ExifToolTitleOverwrite()
        {
            var exifToolModel = new ExifToolModel{Title = "testung"};
            Assert.AreEqual("testung",exifToolModel.ObjectName);
            Assert.AreEqual(null, exifToolModel.Title);
        }
        
        [TestMethod]
        public void ExifToolDescriptionOverwrite()
        {
            var exifToolModel = new ExifToolModel{Description = "testung"};
            Assert.AreEqual("testung",exifToolModel.CaptionAbstract);
            Assert.AreEqual(null, exifToolModel.Description);
        }

	    [TestMethod]
	    public void ExifToolModelSourceFile()
	    {
		    var exifToolModel = new ExifToolModel { SourceFile = "testung" };
		    Assert.AreEqual("testung", exifToolModel.SourceFile);
	    }

	}
}
