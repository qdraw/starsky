using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starskycore.Helpers;
using starskycore.Models;

namespace starskytest.Helpers
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
	    public void FileIndexCompareHelperTest_StringList_Compare()
	    {
		    var source = new FileIndexItem {CollectionPaths = new List<string>{"source"}};
		    var update = new FileIndexItem {CollectionPaths = new List<string>{"update"}};
		    FileIndexCompareHelper.Compare(source, update);
		    Assert.AreEqual("update",source.CollectionPaths[0]);
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
            var source = new FileIndexItem {ColorClass = ColorClassParser.Color.None};
            var update = new FileIndexItem {ColorClass = ColorClassParser.Color.Winner};
            FileIndexCompareHelper.Compare(source, update);
            Assert.AreEqual(ColorClassParser.Color.Winner,source.ColorClass);
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
	    public void FileIndexCompareHelperTest_ushort_Compare()
	    {
		    var source = new FileIndexItem {IsoSpeed = 0};
		    var update = new FileIndexItem {IsoSpeed = 1};
		    FileIndexCompareHelper.Compare(source, update);
		    Assert.AreEqual(1,source.IsoSpeed);
	    }
	    
	    [TestMethod]
	    public void FileIndexCompareHelperTest_double_Compare()
	    {
		    var source = new FileIndexItem {Aperture = 0};
		    var update = new FileIndexItem {Aperture = 1};
		    FileIndexCompareHelper.Compare(source, update);
		    Assert.AreEqual(1,source.Aperture);
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
	
	    [TestMethod]
	    public void FileIndexCompareHelperTest_FilePath_Compare()
	    {
		    // this one is ignored
		    var source = new FileIndexItem {FilePath = "/test"};
		    var update = new FileIndexItem {FilePath = "/ignore"};
		    FileIndexCompareHelper.Compare(source, update);
		    Assert.AreEqual("/",source.FilePath);
	    }
	    
	    
	    [TestMethod]
	    public void FileIndexCompareHelperTest__CheckIfPropertyExist_Tags_True()
	    {
		    Assert.AreEqual(true,FileIndexCompareHelper.CheckIfPropertyExist(nameof(FileIndexItem.Tags)));
	    }

	    [TestMethod]
	    public void FileIndexCompareHelperTest__CheckIfPropertyExist_False()
	    {
		    Assert.AreEqual(false,FileIndexCompareHelper.CheckIfPropertyExist("45678987654"));
	    }

	    [TestMethod]
	    public void FileIndexCompareHelperTest__SetValue_Tags()
	    {
		    Assert.AreEqual("value", FileIndexCompareHelper.Set(null,nameof(FileIndexItem.Tags),"value").Tags);
	    }
	    
	    [TestMethod]
	    public void FileIndexCompareHelperTest__SetValue_Tags_LowerCase()
	    {
		    Assert.AreEqual("value", FileIndexCompareHelper.Set(null,nameof(FileIndexItem.Tags).ToLowerInvariant(),"value").Tags);
	    }
	    
	    [TestMethod]
	    public void FileIndexCompareHelperTest__SetValue_UnknownValue()
	    {
		    // try database type that does not exist
		    Assert.AreEqual(string.Empty, FileIndexCompareHelper.Set(null,"ThisTagDoesNotExist","value").Tags);
	    }
	    
	    [TestMethod]
	    public void FileIndexCompareHelperTest__SetValue_WrongTypeCast()
	    {
		    // wrong types are ignored by default
		    Assert.AreEqual(string.Empty, FileIndexCompareHelper.Set(null,nameof(FileIndexItem.Tags),1).Tags);
	    }

	    [TestMethod]
	    public void FileIndexCompareHelperTest_GetValue()
	    {
		    var t = new FileIndexItem{Tags = "test"};
		    var result = FileIndexCompareHelper.Get(t, nameof(FileIndexItem.Tags));
		    Assert.AreEqual(t.Tags,result);
	    }
	    
	    [TestMethod]
	    public void FileIndexCompareHelperTest_GetValue_LowerCase()
	    {
		    var t = new FileIndexItem{Tags = "test"};
		    var result = FileIndexCompareHelper.Get(t, nameof(FileIndexItem.Tags).ToLowerInvariant());
		    Assert.AreEqual(t.Tags,result);
	    }

	    [TestMethod]
	    public void FileIndexCompareHelperTest_GetValue_NullFieldName()
	    {
		    var t = new FileIndexItem{Tags = "test"};
		    var result = FileIndexCompareHelper.Get(t, "ThisTagDoesNotExist");
		    Assert.AreEqual(null,result);
	    }
	    
	    [TestMethod]
	    public void FileIndexCompareHelperTest_GetValue_NullFileIndexItem()
	    {
		    var result = FileIndexCompareHelper.Get(null, nameof(FileIndexItem.Tags));
		    Assert.AreEqual(null,result);
	    }

    }
}
