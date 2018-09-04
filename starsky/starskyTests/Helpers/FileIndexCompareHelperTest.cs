using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;
using starsky.Models;

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
            Assert.AreEqual("hiupdate",source.Description);
        }

        [TestMethod]
        public void FileIndexCompareHelperTest_colorClass_Compare()
        {
            var source = new FileIndexItem {ColorClass = FileIndexItem.Color.None};
            var update = new FileIndexItem {ColorClass = FileIndexItem.Color.Winner};
            FileIndexCompareHelper.Compare(source, update);
            Assert.AreEqual(FileIndexItem.Color.Winner,source.ColorClass);
        }
    }
}