using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;
using starsky.Services;

namespace starskytests
{
    [TestClass]
    public class FilesTest
    {
        [TestMethod]
        public void IsFolderOrFileTest()
        {
            var newImage = new CreateAnImage();
            // Testing base folder of Image, and Image it self
            Assert.AreEqual(Files.IsFolderOrFile(newImage.BasePath), FolderOrFileModel.FolderOrFileTypeList.Folder);
            Assert.AreEqual(Files.IsFolderOrFile(newImage.FullFilePath), FolderOrFileModel.FolderOrFileTypeList.File);
        }

        [TestMethod]
        public void GetAllFilesDirectoryTest()
        {
            // Assumes that
            //     ~/.nuget/packages/microsoft.testplatform.testhost/15.6.0/lib/netstandard1.5/
            // has subfolders
            
            // Used For subfolders
            var newImage = new CreateAnImage();
            var filesInFolder = Files.GetAllFilesDirectory(newImage.BasePath);
            Assert.AreEqual(filesInFolder.Any(),true);
            
        }

        [TestMethod]
        public void GetFilesInDirectoryTest1()
        {
            // Used for JPEG files
            var newImage = new CreateAnImage();
            AppSettingsProvider.ThumbnailTempFolder = newImage.BasePath;
            AppSettingsProvider.BasePath = newImage.BasePath;
            var filesInFolder = Files.GetFilesInDirectory("/");
            Assert.AreEqual(filesInFolder.Any(),true);
            
        }

    }
}