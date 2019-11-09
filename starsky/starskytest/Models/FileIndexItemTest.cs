using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Models;

namespace starskytest.Models
{
    [TestClass]
    public class FileIndexItemTest
    {
        [TestMethod]
        public void FileIndexItemTest_SetTagsToNull()
        {
            var item = new FileIndexItem{Tags = null};
            Assert.AreEqual(item.Tags,string.Empty);
        }
	    
	    [TestMethod]
	    public void FileIndexItemTest_KeywordsToNull()
	    {
		    var item = new FileIndexItem{Keywords = null};
		    // > read tags instead of keywords
		    Assert.AreEqual(item.Tags,string.Empty);
	    }

	    [TestMethod]
	    public void FileIndexItem_DoubleSpaces()
	    {
		    var item = new FileIndexItem{Tags = "test0, test1  ,   test2,   test3, test4,   test5, test6, test7,   "};
		    
		    Assert.AreEqual("test1", item.Keywords.ToList()[1]);
		    Assert.AreEqual("test2", item.Keywords.ToList()[2]);
		    Assert.AreEqual("test5", item.Keywords.ToList()[5]);
		    Assert.AreEqual("test7", item.Keywords.ToList()[7]);
		    Assert.AreEqual(8,item.Keywords.Count);
		}
        	    
        [TestMethod]
        public void FileIndexItemTest_SetDescriptionsToNull()
        {
            var item = new FileIndexItem{Description = null};
            Assert.AreEqual(item.Description,string.Empty);
        }
        
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTestDefault()
        {
            var input = new FileIndexItem().GetColorClass();
            var output = FileIndexItem.Color.None;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTestMin1()
        {
            var input = new FileIndexItem().GetColorClass("-1");
            var output = FileIndexItem.Color.DoNotChange;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest0()
        {
            var input = new FileIndexItem().GetColorClass("0");
            var output = FileIndexItem.Color.None;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest1()
        {
            var input = new FileIndexItem().GetColorClass("1");
            var output = FileIndexItem.Color.Winner;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest2()
        {
            var input = new FileIndexItem().GetColorClass("2");
            var output = FileIndexItem.Color.WinnerAlt;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest3()
        {
            var input = new FileIndexItem().GetColorClass("3");
            var output = FileIndexItem.Color.Superior;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest4()
        {
            var input = new FileIndexItem().GetColorClass("4");
            var output = FileIndexItem.Color.SuperiorAlt;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest5()
        {
            var input = new FileIndexItem().GetColorClass("5");
            var output = FileIndexItem.Color.Typical;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest6()
        {
            var input = new FileIndexItem().GetColorClass("6");
            var output = FileIndexItem.Color.TypicalAlt;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest7()
        {
            var input = new FileIndexItem().GetColorClass("7");
            var output = FileIndexItem.Color.Extras;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest8()
        {
            var input = new FileIndexItem().GetColorClass("8");
            var output = FileIndexItem.Color.Trash;
            Assert.AreEqual(input,output);
        }

        [TestMethod]
        public void FileIndexItemTest_GetColorClassListTestEightSeven()
        {
            var input = "8,7";
            var eightSeven = new List<FileIndexItem.Color> {FileIndexItem.Color.Trash,FileIndexItem.Color.Extras};
            var output = new FileIndexItem().GetColorClassList(input);
            CollectionAssert.AreEqual(eightSeven,output);
        }

	    [TestMethod]
	    public void FileIndexItemTest_GetColorClassListString()
	    {
		    var input = "string";
		    var output = new FileIndexItem().GetColorClassList(input);
		    Assert.AreEqual(0,output.Count); // <= 0
	    }

	    [TestMethod]
        public void FileIndexItemTest_GetAllColorTest()
        {
            Assert.IsTrue(FileIndexItem.GetAllColor().Any());
        }
        
        [TestMethod]
        public void FileIndexItemTest_FileIndexItemTitleTest()
        {
            var fileIndexItem = new FileIndexItem {Title = null};
            Assert.AreEqual(fileIndexItem.Title,string.Empty);
        }

        [TestMethod]
        public void FileIndexItemTest_FileNameNull()
        {
            var t = new FileIndexItem();
            Assert.AreEqual(string.Empty,t.FileName);
        }
        
        [TestMethod]
        public void FileIndexItemTest_ParentDirectoryNull()
        {
            var t = new FileIndexItem();
            Assert.AreEqual(string.Empty,t.ParentDirectory);
        }
        
        [TestMethod]
        public void FileIndexItemTest_FilePathNull()
        {
            var t = new FileIndexItem();
            Assert.AreEqual("/",t.FilePath);
        }
        
        [TestMethod]
        public void FileIndexItemTest_OrientationrelativeRotation0()
        {
            // keep the same
            var t = new FileIndexItem {Orientation = FileIndexItem.Rotation.Horizontal};
            Assert.AreEqual(FileIndexItem.Rotation.Horizontal,t.RelativeOrientation());
        }

        [TestMethod]
        public void FileIndexItemTest_SetOrientationrelativeRotation0()
        {
            var fileObject = new FileIndexItem {Orientation = FileIndexItem.Rotation.Horizontal};
            fileObject.SetRelativeOrientation();
            Assert.AreEqual(FileIndexItem.Rotation.Horizontal,fileObject.Orientation);
        }
            
        [TestMethod]
        public void FileIndexItemTest_OrientationrelativeRotation_270CwTest()
        {
	        var fileObject = new FileIndexItem {Orientation = FileIndexItem.Rotation.Rotate270Cw};
	        Assert.AreEqual(FileIndexItem.Rotation.Horizontal,fileObject.RelativeOrientation(1));
        }
        

        [TestMethod]
        public void FileIndexItemTest_SetOrientationrelativeRotationMinus1()
        {
            var t = new FileIndexItem {Orientation = FileIndexItem.Rotation.Horizontal};
            Assert.AreEqual(FileIndexItem.Rotation.Rotate270Cw,t.RelativeOrientation(-1));
        }

        [TestMethod]
        public void FileIndexItemTest_SetOrientationrelativeRotationPlus1()
        {
            var t = new FileIndexItem {Orientation = FileIndexItem.Rotation.Horizontal};
            Assert.AreEqual(FileIndexItem.Rotation.Rotate90Cw,t.RelativeOrientation(1));
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetOrientationrelativeRotation_Rotate270Cw_Plus1()
        {
            var t = new FileIndexItem {Orientation = FileIndexItem.Rotation.Rotate270Cw};
            Assert.AreEqual(FileIndexItem.Rotation.Horizontal,t.RelativeOrientation(1));
        }


        [TestMethod]
        public void FileIndexItemTest_SetOrientationrelativeRelativeOrientation_Plus5()
        {
            // test not very good
            var t = new FileIndexItem {Orientation = FileIndexItem.Rotation.Rotate270Cw};
            Assert.AreEqual(FileIndexItem.Rotation.Horizontal,t.RelativeOrientation(5));
        }

        [TestMethod]
        public void FileIndexItemTest_SetAbsoluteOrientation_DoNotChange()
        {
            var rotationItem = new FileIndexItem().SetAbsoluteOrientation("0");
            Assert.AreEqual(FileIndexItem.Rotation.DoNotChange,rotationItem);
        }

        [TestMethod]
        public void FileIndexItemTest_SetAbsoluteOrientation_Rotate90Cw()
        {
            var rotationItem = new FileIndexItem().SetAbsoluteOrientation("6");
            Assert.AreEqual(FileIndexItem.Rotation.Rotate90Cw,rotationItem);
        }

        [TestMethod]
        public void FileIndexItemTest_SetAbsoluteOrientation_Rotate180()
        {
            var rotationItem = new FileIndexItem().SetAbsoluteOrientation("3");
            Assert.AreEqual(FileIndexItem.Rotation.Rotate180,rotationItem);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetAbsoluteOrientation_Rotate270Cw()
        {
            var rotationItem = new FileIndexItem().SetAbsoluteOrientation("8");
            Assert.AreEqual(FileIndexItem.Rotation.Rotate270Cw,rotationItem);
        }

        [TestMethod]
        public void FileIndexItemTest_colorDisplayName_WinnerAlt()
        {
            var colorDisplayName = EnumHelper.GetDisplayName(FileIndexItem.Color.WinnerAlt);
            Assert.AreEqual("Winner Alt",colorDisplayName);
        }
	    
	    [TestMethod]
	    public void FileIndexItemTest_MakeModel_UsingField()
	    {
		    var item = new FileIndexItem{MakeModel = "Apple|iPad|??"};
		    Assert.AreEqual("Apple", item.Make);
		    Assert.AreEqual("iPad",item.Model);
	    }

	    [TestMethod]
	    public void FileIndexItemTest_MakeModel_UsingFieldNull()
	    {
		    var item = new FileIndexItem{MakeModel = null};
		    Assert.AreEqual(string.Empty, item.Make);
	    }

	    [TestMethod]
	    public void FileIndexItemTest_SetMakeModel_Model()
	    {
		    var item = new FileIndexItem();
		    item.SetMakeModel("iPhone", 1);

			Assert.AreEqual("iPhone", item.Model);
		    Assert.AreEqual("|iPhone|", item.MakeModel);

		}

		[TestMethod]
	    public void FileIndexItemTest_SetMakeModel_Make()
	    {
		    var item = new FileIndexItem();
		    item.SetMakeModel("APPLE", 0);

		    Assert.AreEqual("Apple", item.Make);
		    Assert.AreEqual("Apple||", item.MakeModel);
		}

	    [TestMethod]
	    public void FileIndexItemTest_SetMakeModel_MakeWrongPipeLength()
	    {
		    var item = new FileIndexItem{MakeModel = "Apple|||||||"};
		    Assert.AreEqual(string.Empty, item.Make);
		    Assert.AreEqual(string.Empty, item.Model);
	    }

	    [TestMethod]
		public void FileIndexItemTest_SetMakeModel_WrongOrder_MakeANDModel()
		{
			var item = new FileIndexItem();
			// the wrong order > 1,0,2
			item.SetMakeModel("iPhone", 1);
			item.SetMakeModel("iPad", 1);

			item.SetMakeModel("Apple", 0);

			item.SetMakeModel("Lens", 2);

			Assert.AreEqual("Apple|iPad|Lens", item.MakeModel);

			Assert.AreEqual("Apple", item.Make);
			Assert.AreEqual("iPad", item.Model);
		}

	    [TestMethod]
	    [ExpectedException(typeof(AggregateException))]
	    public void FileIndexItemTest_SetMakeModel_WrongPipeLength()
	    {
		    var item = new FileIndexItem();
		    item.SetMakeModel("Apple", 95);
		    // this index (95) never exist
	    }

	    [TestMethod]
	    public void FileIndexItemTest_SetMakeModel_RightOrder_MakeANDModel()
	    {
		    var item = new FileIndexItem();

		    item.SetMakeModel("Apple", 0);
		    item.SetMakeModel("iPhone", 1);

		    Assert.AreEqual("Apple|iPhone|", item.MakeModel);

		    Assert.AreEqual("Apple", item.Make);
		    Assert.AreEqual("iPhone", item.Model);
	    }

	    [TestMethod]
	    public void FileIndexItemTest_IsRelativeOrientation()
	    {
			var item = FileIndexItem.IsRelativeOrientation(-1);
		    Assert.AreEqual(true,item);
		    
		    var item2 = FileIndexItem.IsRelativeOrientation(1);
		    Assert.AreEqual(true,item2);
		    
		    var item999 = FileIndexItem.IsRelativeOrientation(999);
		    Assert.AreEqual(false,item999);
	    }


	    [TestMethod]
	    public void FileIndexItemTest_Ctor_SpaceName()
	    {
		    var item = new FileIndexItem("/test/image with space.jpg");
			Assert.AreEqual("image with space.jpg",item.FileName);
		    Assert.AreEqual("image with space",item.FileCollectionName);
		    Assert.AreEqual("/test",item.ParentDirectory);
	    }



	    //        [TestMethod]
		//        public void FileIndexItemParseFileNameTest()
		//        {
		//            var createAnImage = new CreateAnImage();
		//
		//            var fileIndexItem = new FileIndexItem
		//            {
		//                FilePath = createAnImage.FullFilePath
		//            };
		////            var t = fileIndexItem.ParseFileName();
		//            
		//        }


	}
}
