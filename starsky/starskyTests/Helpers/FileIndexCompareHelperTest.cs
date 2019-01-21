using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Models;

namespace starskytests.Helpers
{
    [TestClass]
    public class FileIndexCompareHelperTest
    {
        [TestMethod]
        public void FileIndexCompareHelperTest_String_Compare()
        {
            var source = new FileIndexItem {Tags = "hi"};
            var update = new FileIndexItem {Tags = "update"};
            FileIndexCompareHelper.Compare(source, update);
            Assert.AreEqual("update",source.Tags);
        }
        [TestMethod]
        public void FileIndexCompareHelperTest_String_Tags_AppendCompare()
        {
            var source = new FileIndexItem {Tags = "hi"};
            var update = new FileIndexItem {Tags = "update"};
            FileIndexCompareHelper.Compare(source, update,true);
            Assert.AreEqual("hi, update",source.Tags);
        }
        [TestMethod]
        public void FileIndexCompareHelperTest_String_Description_AppendCompare()
        {
            var source = new FileIndexItem {Description = "hi"};
            var update = new FileIndexItem {Description = "update"};
            FileIndexCompareHelper.Compare(source, update,true);
            Assert.AreEqual("hi update",source.Description);
        }

        [TestMethod]
        public void FileIndexCompareHelperTest_colorClass_Compare()
        {
            var source = new FileIndexItem {ColorClass = FileIndexItem.Color.None};
            var update = new FileIndexItem {ColorClass = FileIndexItem.Color.Winner};
            FileIndexCompareHelper.Compare(source, update);
            Assert.AreEqual(FileIndexItem.Color.Winner,source.ColorClass);
        }
        
        [TestMethod]
        public void FileIndexCompareHelperTest_bool_Compare()
        {
            var source = new FileIndexItem {IsDirectory = false};
            var update = new FileIndexItem {IsDirectory = true};
            FileIndexCompareHelper.Compare(source, update);
            Assert.AreEqual(true,source.IsDirectory);
        }

        [TestMethod]
        public void FileIndexCompareHelperTest_DateTimeNoOverwrite_Compare()
        {
            // so no overwrite
            var source = new FileIndexItem {DateTime = DateTime.Now};
            var update = new FileIndexItem {DateTime = new DateTime()};
            FileIndexCompareHelper.Compare(source, update);
            Assert.AreNotEqual(update.DateTime,source.DateTime); 
        }
        
        [TestMethod]
        public void FileIndexCompareHelperTest_DateTime_Compare()
        {
            // source= null> update is new overwrite
            var source = new FileIndexItem {DateTime = new DateTime()};
            var update = new FileIndexItem {DateTime =  DateTime.Now};
            FileIndexCompareHelper.Compare(source, update);
            Assert.AreEqual(update.DateTime,source.DateTime); 
        }
        
        [TestMethod]
        public void FileIndexCompareHelperTest_Rotation_Compare()
        {
            var source = new FileIndexItem {Orientation = FileIndexItem.Rotation.Horizontal};
            var update = new FileIndexItem {Orientation = FileIndexItem.Rotation.Rotate90Cw};
            FileIndexCompareHelper.Compare(source, update);
            Assert.AreEqual(source.Orientation,FileIndexItem.Rotation.Rotate90Cw); 
        }

	    [TestMethod]
	    public void FileIndexCompareHelperTest_SetCompare()
	    {
		    var source = new FileIndexItem {DateTime = new DateTime()};
		    var update = new FileIndexItem {DateTime =  DateTime.Now};
		    var result =  FileIndexCompareHelper.SetCompare(source, update, new List<string>
		    {
			    "DateTime"
		    });
		    Assert.AreEqual(update.DateTime, result.DateTime);

	    }
    }
}