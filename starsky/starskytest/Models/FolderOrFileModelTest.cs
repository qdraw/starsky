using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Models;

namespace starskytest.Models
{
    [TestClass]
    public class FolderOrFileModelTest
    {
        [TestMethod]
        public void FolderOrFileModelFolderOrFileTypeListTest()
        {
            var ToSearchType = FolderOrFileModel.FolderOrFileTypeList.Folder;
            var folderOrFileModel = new FolderOrFileModel 
                {
                    IsFolderOrFile = ToSearchType
                };
            Assert.AreEqual(folderOrFileModel.IsFolderOrFile,ToSearchType);
        }

    }
}
