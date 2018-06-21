﻿using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Data;
using starsky.Helpers;
using starsky.Models;
using starsky.Services;

namespace starskytests
{
    [TestClass]
    public class ImportServiceTest
    {
        private ImportService _import;
        private Query _query;
        private SyncService _isync;

        public ImportServiceTest()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("test");
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _query = new Query(context);
            _isync = new SyncService(context, _query);
            _import = new ImportService(context,_isync);
        }

        
        [TestMethod]
        public void ImportService_slashyyyyMMdd_HHmmss_ImportTest()
        {
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.Structure = "/xxxxx__yyyyMMdd_HHmmss.ext";
            // This is not to be the first file in the test directory
            // => otherwise SyncServiceFirstItemDirectoryTest() will fail
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            _import.Import(createAnImage.FullFilePath);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            Assert.AreEqual(true, _import.IsHashInDatabase(fileHashCode));

            // Clean file after succesfull run;
            var fileIndexItem = ExifRead.ReadExifFromFile(createAnImage.FullFilePath);
            var importIndexItem = new ImportIndexItem
            {
                SourceFullFilePath = createAnImage.FullFilePath,  
                DateTime = fileIndexItem.DateTime
            };
            File.Delete(FileIndexItem.DatabasePathToFilePath(importIndexItem.ParseSubfolders() + importIndexItem.ParseFileName()));
            _import.RemoveItem(_import.GetItemByHash(fileHashCode));

        }
        
        [TestMethod]
        public void ImportService_AsteriskTRFolderHHmmss_ImportTest()
        {
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.Structure = "/\\t\\r*/HHmmss_\\d.ext";
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            _import.Import(createAnImage.FullFilePath);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            Assert.AreEqual(true, _import.IsHashInDatabase(fileHashCode));

            // Clean file after succesfull run;
            var fileIndexItem = ExifRead.ReadExifFromFile(createAnImage.FullFilePath);
            var importIndexItem = new ImportIndexItem
            {
                SourceFullFilePath = createAnImage.FullFilePath,  
                DateTime = fileIndexItem.DateTime
            };

            File.Delete(
                FileIndexItem.DatabasePathToFilePath(
                    importIndexItem.ParseSubfolders() + importIndexItem.ParseFileName()
                )
            );
            
            _import.RemoveItem(_import.GetItemByHash(fileHashCode));
        }
        
        
        [TestMethod]
        public void ImportService_NonExistingFolder_HHmmssImportTest()
        {
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.Structure = "/\\t\\r/\\a\\b\\c/HHmmss.ext";
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            _import.Import(createAnImage.FullFilePath);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            
            Assert.AreEqual(true, _import.IsHashInDatabase(fileHashCode));

            // Clean file after succesfull run;
            var fileIndexItem = ExifRead.ReadExifFromFile(createAnImage.FullFilePath);
            var importIndexItem = new ImportIndexItem {SourceFullFilePath = createAnImage.FullFilePath,  DateTime = fileIndexItem.DateTime};
            File.Delete(FileIndexItem.DatabasePathToFilePath(importIndexItem.ParseSubfolders() + importIndexItem.ParseFileName()));
            
            Files.DeleteDirectory(FileIndexItem.DatabasePathToFilePath(importIndexItem.ParseSubfolders()));
            _import.RemoveItem(_import.GetItemByHash(fileHashCode));

        }
        
        [TestMethod]
        public void ImportService_WithoutExt_ImportTest()
        {
            // We currently force you to use an extension
            
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.Structure = "/\\t\\r*/ssHHmm";
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            _import.Import(createAnImage.FullFilePath);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            
            Assert.AreEqual(true, _import.IsHashInDatabase(fileHashCode));

            // Clean file after succesfull run;
            var fileIndexItem = ExifRead.ReadExifFromFile(createAnImage.FullFilePath);
            var importIndexItem = new ImportIndexItem {SourceFullFilePath = createAnImage.FullFilePath,  DateTime = fileIndexItem.DateTime};
            File.Delete(FileIndexItem.DatabasePathToFilePath(importIndexItem.ParseSubfolders() +importIndexItem.ParseFileName()));
            _import.RemoveItem(_import.GetItemByHash(fileHashCode));

        }

        [TestMethod]
        public void ImportService_DuplicateImport_HHmmssImportTest()
        {
            // todo: implement duplicate file import
        }

        [TestMethod]
        public void ImportService_DeleteAfterTest_HHmmssImportTest()
        {
             // // Test if a source file is delete afterwards
            var createAnImage = new CreateAnImage();
            
            AppSettingsProvider.Structure = "/\\t\\r/\\a\\b\\c/yyyy/mm/HHmmss.ext";
            AppSettingsProvider.BasePath = createAnImage.BasePath;

            var fullFilePath = createAnImage.FullFilePath.Replace("00", "01");
            
            try
            {
                File.Copy(createAnImage.FullFilePath,fullFilePath);
            }
            catch (IOException)
            {
            }
            
            var fileHashCode = FileHash.GetHashCode(fullFilePath);
            
            
            _import.Import(fullFilePath, true);  
            

            Assert.AreEqual(File.Exists(fullFilePath), false);

            var importIndexItem = _import.GetItemByHash(fileHashCode);
            var outputFileName = importIndexItem.ParseFileName();
            var outputSubfolders = importIndexItem.ParseSubfolders();

            _import.RemoveItem(importIndexItem);
            File.Delete(FileIndexItem.DatabasePathToFilePath(importIndexItem.ParseSubfolders() +importIndexItem.ParseFileName()));

        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ImportService_NonExistingImportFail_ImportTest()
        {
            // Get an Not found error when parsing
            var importIndexItem = new ImportIndexItem
            {
                SourceFullFilePath = "123456789876",  
                DateTime = DateTime.Now
            };
            importIndexItem.ParseFileName();
        }

        [TestMethod]
        public void ImportService_EntireBasePath_Folder_Import_ToTR_Test()
        {
            // import folder
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.Structure = "/\\t\\r*/\\f\\o\\l\\d\\e\\r\\i\\m\\p\\o\\r\\t_HHssmm.ext";
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            
            _import.Import(createAnImage.FullFilePath);  

            Assert.AreEqual(File.Exists(createAnImage.FullFilePath), true);

            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            var importIndexItem = _import.GetItemByHash(fileHashCode);
            var outputFileName = importIndexItem.ParseFileName();
            var outputSubfolders = importIndexItem.ParseSubfolders();

            _import.RemoveItem(importIndexItem);
            File.Delete(FileIndexItem.DatabasePathToFilePath(importIndexItem.ParseSubfolders() +importIndexItem.ParseFileName()));
        }
    }
}