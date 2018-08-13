using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;

namespace starskytests
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
        public void FileIndexItemTest_SetColorClassTestDefault()
        {
            var input = new FileIndexItem().SetColorClass();
            var output = FileIndexItem.Color.None;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTestMin1()
        {
            var input = new FileIndexItem().SetColorClass("-1");
            var output = FileIndexItem.Color.DoNotChange;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest0()
        {
            var input = new FileIndexItem().SetColorClass("0");
            var output = FileIndexItem.Color.None;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest1()
        {
            var input = new FileIndexItem().SetColorClass("1");
            var output = FileIndexItem.Color.Winner;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest2()
        {
            var input = new FileIndexItem().SetColorClass("2");
            var output = FileIndexItem.Color.WinnerAlt;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest3()
        {
            var input = new FileIndexItem().SetColorClass("3");
            var output = FileIndexItem.Color.Superior;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest4()
        {
            var input = new FileIndexItem().SetColorClass("4");
            var output = FileIndexItem.Color.SuperiorAlt;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest5()
        {
            var input = new FileIndexItem().SetColorClass("5");
            var output = FileIndexItem.Color.Typical;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest6()
        {
            var input = new FileIndexItem().SetColorClass("6");
            var output = FileIndexItem.Color.TypicalAlt;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest7()
        {
            var input = new FileIndexItem().SetColorClass("7");
            var output = FileIndexItem.Color.Extras;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void FileIndexItemTest_SetColorClassTest8()
        {
            var input = new FileIndexItem().SetColorClass("8");
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
        public void FileIndexItemTest_SetOrientationrelativeRotation0()
        {
            // keep the same
            var t = new FileIndexItem {Orientation = FileIndexItem.Rotation.Horizontal};
            t.RelativeOrientation();
            Assert.AreEqual(FileIndexItem.Rotation.Horizontal,t.Orientation);
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