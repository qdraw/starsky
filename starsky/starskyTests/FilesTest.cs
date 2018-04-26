using System;
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
            AppSettingsProvider.BasePath = newImage.BasePath;

            Console.WriteLine(newImage.BasePath);
            Console.WriteLine(newImage.FullFilePath);
            
            Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Folder, Files.IsFolderOrFile("/"));
            Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.File,Files.IsFolderOrFile(newImage.DbPath));
        }

        [TestMethod]
        public void GetAllFilesDirectoryTest()
        {
            // Assumes that
            //     ~/.nuget/packages/microsoft.testplatform.testhost/15.6.0/lib/netstandard1.5/
            // has subfolders
            
            // Used For subfolders
            var newImage = new CreateAnImage();
            AppSettingsProvider.BasePath = newImage.BasePath;
            var filesInFolder = Files.GetAllFilesDirectory();
            Assert.AreEqual(true,filesInFolder.Any());
            
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