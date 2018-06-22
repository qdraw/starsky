using System.Linq;
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

        [TestMethod]
        public void ImportIndexItemParseSubfoldersTest()
        {
            var createAnImage = new CreateAnImage();
            var importItem = new ImportIndexItem();
            importItem.SourceFullFilePath = createAnImage.FullFilePath;
            AppSettingsProvider.Structure = "/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss.ext";
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            var s = importItem.ParseSubfolders(false);
            Assert.AreEqual("/0001/01/0001_01_01",s);
        }

        [TestMethod]
        public void ImportIndexItemParseSubfolders_TRslashABC_Test()
        {
            var createAnImage = new CreateAnImage();
            var importItem = new ImportIndexItem();
            AppSettingsProvider.Structure = "/\\t\\r/\\a\\b\\c/test";

            importItem.SourceFullFilePath = createAnImage.FullFilePath;
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            var s = importItem.ParseSubfolders(false);
            Assert.AreEqual("/tr/abc",s);
        }

    }
}