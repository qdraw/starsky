using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.writemeta.Models;

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
			Assert.AreEqual("test",exifToolModel.CaptionAbstract);
		}


		[TestMethod]
		public void ExifToolPrefsParseTest()
		{
			var exifToolModel = new ExifToolModel{Prefs = "Tagged:0, ColorClass:2, Rating:0, FrameNum:0"};
			Assert.AreEqual(ColorClassParser.Color.WinnerAlt, exifToolModel.ColorClass);
		}
        
		[TestMethod]
		public void ExifToolPrefsNullTest()
		{
			var exifToolModel = new ExifToolModel();
			Assert.AreEqual(null,exifToolModel.Prefs);
		}
        
		[TestMethod]
		public void ExifToolTagsKeywordsFirstTest()
		{
			var exifToolModel = new ExifToolModel{Tags = "Schiphol, airplane"};
			Assert.AreEqual("Schiphol",exifToolModel.Keywords.FirstOrDefault());
		}

		[TestMethod]
		public void ExifToolSetHashSet1Test()
		{
			var list = new List<string> {"Schiphol", "Schiphol"};
			var exifToolModel = new ExifToolModel{Keywords = list.ToHashSet()};
			Assert.AreEqual(1,exifToolModel.Keywords.Count);
		}
        
		[TestMethod]
		public void ExifTool_hashSetToStringTest()
		{
			// It is in memory stored as HashSet, not as string
			var exifToolModel = new ExifToolModel{Tags = "Schiphol, airplane"};
			Assert.AreEqual("Schiphol, airplane",exifToolModel.Tags);
		}

		[TestMethod]
		public void ExifTool_hashSetToStringNullTest()
		{
			var exifToolModel = new ExifToolModel();
			Assert.AreEqual(string.Empty,exifToolModel.Tags);
		}
        
		[TestMethod]
		public void ExifToolKeywordsNullTest()
		{
			var exifToolModel = new ExifToolModel{Keywords = null};
			Assert.AreEqual(0,exifToolModel.Keywords.Count);
		}
        
		[TestMethod]
		public void ExifToolSubjectOverwrite()
		{
			var list = new List<string> {"Schiphol", "Schiphol"};
			var exifToolModel = new ExifToolModel{Subject = list.ToHashSet()};
			Assert.AreEqual(1,exifToolModel.Keywords.Count);
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
