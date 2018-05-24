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
        public void SetTagsToNull()
        {
            var item = new FileIndexItem{Tags = null};
            Assert.AreEqual(item.Tags,string.Empty);
        }
        
        
        [TestMethod]
        public void SetColorClassTestDefault()
        {
            var input = new FileIndexItem().SetColorClass();
            var output = FileIndexItem.Color.None;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void SetColorClassTestMin1()
        {
            var input = new FileIndexItem().SetColorClass("-1");
            var output = FileIndexItem.Color.DoNotChange;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void SetColorClassTest0()
        {
            var input = new FileIndexItem().SetColorClass("0");
            var output = FileIndexItem.Color.None;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void SetColorClassTest1()
        {
            var input = new FileIndexItem().SetColorClass("1");
            var output = FileIndexItem.Color.Winner;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void SetColorClassTest2()
        {
            var input = new FileIndexItem().SetColorClass("2");
            var output = FileIndexItem.Color.WinnerAlt;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void SetColorClassTest3()
        {
            var input = new FileIndexItem().SetColorClass("3");
            var output = FileIndexItem.Color.Superior;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void SetColorClassTest4()
        {
            var input = new FileIndexItem().SetColorClass("4");
            var output = FileIndexItem.Color.SuperiorAlt;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void SetColorClassTest5()
        {
            var input = new FileIndexItem().SetColorClass("5");
            var output = FileIndexItem.Color.Typical;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void SetColorClassTest6()
        {
            var input = new FileIndexItem().SetColorClass("6");
            var output = FileIndexItem.Color.TypicalAlt;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void SetColorClassTest7()
        {
            var input = new FileIndexItem().SetColorClass("7");
            var output = FileIndexItem.Color.Extras;
            Assert.AreEqual(input,output);
        }
        
        [TestMethod]
        public void SetColorClassTest8()
        {
            var input = new FileIndexItem().SetColorClass("8");
            var output = FileIndexItem.Color.Trash;
            Assert.AreEqual(input,output);
        }

        [TestMethod]
        public void GetColorClassListTestEightSeven()
        {
            var input = "8,7";
            var eightSeven = new List<FileIndexItem.Color> {FileIndexItem.Color.Trash,FileIndexItem.Color.Extras};
            var output = new FileIndexItem().GetColorClassList(input);
            CollectionAssert.AreEqual(eightSeven,output);
        }

        [TestMethod]
        public void GetAllColorTest()
        {
            Assert.IsTrue(FileIndexItem.GetAllColor().Any());
        }
        
        [TestMethod]
        public void FileIndexItemTitleTest()
        {
            var fileIndexItem = new FileIndexItem {Title = null};
            Assert.AreEqual(fileIndexItem.Title,string.Empty);
        }
        

    }
}