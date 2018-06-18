using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Data;
using starsky.Models;
using starsky.Services;

namespace starskytests
{
    [TestClass]
    public class ImportIndexItemTest
    {
        [TestMethod]
        public void ImportIndexItemParseFileNameTest()
        {
            var createAnImage = new CreateAnImage();

            var importItem = new ImportIndexItem();
            importItem.SourceFullFilePath = createAnImage.FullFilePath;

            var fileName = importItem.ParseFileName();
            Assert.AreEqual("00010101_000000.jpg", fileName);
        }

    }
}