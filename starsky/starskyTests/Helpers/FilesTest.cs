using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;
using starsky.Models;
using starsky.Services;

namespace starskytests
{
    [TestClass]
    public class FilesTest
    {
        [TestMethod]
        public void Files_IsFolderOrFileTest()
        {
            var newImage = new CreateAnImage();
            // Testing base folder of Image, and Image it self
            
            Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.Folder, Files.IsFolderOrFile(newImage.BasePath));
            Assert.AreEqual(FolderOrFileModel.FolderOrFileTypeList.File,Files.IsFolderOrFile(newImage.FullFilePath));
        }

        [TestMethod]
        public void Files_GetAllFilesDirectoryTest()
        {
            // Assumes that
            //     ~/.nuget/packages/microsoft.testplatform.testhost/15.6.0/lib/netstandard1.5/
            // has subfolders
            
            // Used For subfolders
            var newImage = new CreateAnImage();
            var filesInFolder = Files.GetAllFilesDirectory(newImage.BasePath);
            Assert.AreEqual(true,filesInFolder.Any());
            
        }

//        [TestMethod]
//        public void Files_GetFilesInDirectoryTest1()
//        {
//            // Used for JPEG files
//            var newImage = new CreateAnImage();
//            AppSettingsProvider.ThumbnailTempFolder = newImage.BasePath;
//            AppSettingsProvider.BasePath = newImage.BasePath;
//            var filesInFolder = Files.GetFilesInDirectory("/");
//            Assert.AreEqual(filesInFolder.Any(),true);
//        }

//        [TestMethod]
//        public void Files_GetFilesRecrusiveTest()
//        {            
//            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;
//
//            var content = Files.GetFilesRecrusive(path,false);
//
//            Console.WriteLine("count => "+ content.Count());
//
//            // Gives a list of the content in the temp folder.
//            Assert.AreEqual(true, content.Count() >= 5);            
//
//        }
    }
}