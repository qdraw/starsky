using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
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
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            var memoryCache = provider.GetService<IMemoryCache>();
            
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("test");
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _query = new Query(context,memoryCache);
            _isync = new SyncService(context, _query);
            _import = new ImportService(context,_isync);
        }

//        [TestMethod]
//        public void ImportService_NoSubPath_WithoutSlashEndingBasePath_ImportTest()
//        {
//            var createAnImage = new CreateAnImage();
//            AppSettingsProvider.Structure = "/xxxxx__yyyyMMdd_HHmmss.ext";
//            // This is not to be the first file in the test directory
//            // => otherwise SyncServiceFirstItemDirectoryTest() will fail
//            
//            // Remove last lashEndingBasePath
//            for (int i = 0; i < createAnImage.BasePath.Length-1; i++)
//            {
//                AppSettingsProvider.BasePath += createAnImage.BasePath[i];
//            }
//
//            _import.Import(createAnImage.FullFilePath);
//            
//            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
//            Assert.AreEqual(true, _import.IsHashInDatabase(fileHashCode));
//
//            // Clean file after succesfull run;
//            var fileIndexItem = ExifRead.ReadExifFromFile(createAnImage.FullFilePath);
//            var importIndexItem = new ImportIndexItem
//            {
//                SourceFullFilePath = createAnImage.FullFilePath,  
//                DateTime = fileIndexItem.DateTime
//            };
//            
//            File.Delete(
//                FileIndexItem.DatabasePathToFilePath(
//                    importIndexItem.ParseSubfolders() + "/" + importIndexItem.ParseFileName()
//                )
//            );
//            _import.RemoveItem(_import.GetItemByHash(fileHashCode));
//        }
        
        
//        [TestMethod]
//        public void ImportService_NoSubPath_InsideSubFolder_WithoutSlashEndingBasePath_ImportTest()
//        {
//            var createAnImage = new CreateAnImage();
//            AppSettingsProvider.Structure = "/tr/yyyyMMdd_HHmmss.ext";
//
//            // Remove last lashEnding slash in the BasePath config file
//            for (int i = 0; i < createAnImage.BasePath.Length-1; i++)
//            {
//                AppSettingsProvider.BasePath += createAnImage.BasePath[i];
//            }
//
//            _import.Import(createAnImage.FullFilePath);
//            
//            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
//            Assert.AreEqual(true, _import.IsHashInDatabase(fileHashCode));
//
//            // Clean file after succesfull run;
//            var fileIndexItem = ExifRead.ReadExifFromFile(createAnImage.FullFilePath);
//            var importIndexItem = new ImportIndexItem
//            {
//                SourceFullFilePath = createAnImage.FullFilePath,  
//                DateTime = fileIndexItem.DateTime
//            };
//            
//            File.Delete(
//                FileIndexItem.DatabasePathToFilePath(
//                    importIndexItem.ParseSubfolders() + "/" + importIndexItem.ParseFileName()
//                )
//            );
//            _import.RemoveItem(_import.GetItemByHash(fileHashCode));
//        }
        
        [TestMethod]
        public void ImportService_NoSubPath_slashyyyyMMdd_HHmmss_ImportTest()
        {
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.Structure = "/xxxxx__yyyyMMdd_HHmmss.ext";
            // This is not to be the first file in the test directory
            // => otherwise SyncServiceFirstItemDirectoryTest() will fail
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            _import.Import(createAnImage.FullFilePath,false,false);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));

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
            _import.Import(createAnImage.FullFilePath,false,false);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));

            // Clean file after succesfull run;
            var fileIndexItem = ExifRead.ReadExifFromFile(createAnImage.FullFilePath);
            var importIndexItem = new ImportIndexItem
            {
                SourceFullFilePath = createAnImage.FullFilePath,  
                DateTime = fileIndexItem.DateTime
            };

            File.Delete(
                FileIndexItem.DatabasePathToFilePath(
                    importIndexItem.ParseSubfolders() + "/" + importIndexItem.ParseFileName()
                )
            );
            
            _import.RemoveItem(_import.GetItemByHash(fileHashCode));
        }
        
        
        [TestMethod]
        public void ImportService_NonExistingFolder_HHmmssImportTest()
        {
            
            if (!Directory.Exists(AppSettingsProvider.BasePath + Path.DirectorySeparatorChar + "exist"))
            {
                Directory.CreateDirectory(AppSettingsProvider.BasePath + Path.DirectorySeparatorChar + "exist");
            }
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.Structure = "/\\e\\x\\i\\s\\t/\\a\\b\\c/HHmmss.ext";
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            _import.Import(createAnImage.FullFilePath,false,false);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));

            // Clean file after succesfull run;
            var fileIndexItem = ExifRead.ReadExifFromFile(createAnImage.FullFilePath);
            var importIndexItem = new ImportIndexItem {SourceFullFilePath = createAnImage.FullFilePath,  DateTime = fileIndexItem.DateTime};
            File.Delete(FileIndexItem.DatabasePathToFilePath(
                importIndexItem.ParseSubfolders() + "/" + importIndexItem.ParseFileName()
            ));
            
            Files.DeleteDirectory(FileIndexItem.DatabasePathToFilePath(importIndexItem.ParseSubfolders()));
            _import.RemoveItem(_import.GetItemByHash(fileHashCode));

        }
        
        [TestMethod]
        public void ImportService_WithoutExt_ImportTest()
        {
            // We currently force you to use an extension
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            if (!Directory.Exists(AppSettingsProvider.BasePath + Path.DirectorySeparatorChar + "exist"))
            {
                Directory.CreateDirectory(AppSettingsProvider.BasePath + Path.DirectorySeparatorChar + "exist");
            }
            
            AppSettingsProvider.Structure = "/\\e\\x\\i\\s*/ssHHmm";
            _import.Import(createAnImage.FullFilePath,false,false);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));

            // Clean file after succesfull run;
            var fileIndexItem = ExifRead.ReadExifFromFile(createAnImage.FullFilePath);
            var importIndexItem = new ImportIndexItem {SourceFullFilePath = createAnImage.FullFilePath,  DateTime = fileIndexItem.DateTime};
            File.Delete(FileIndexItem.DatabasePathToFilePath(
                importIndexItem.ParseSubfolders() + "/" + importIndexItem.ParseFileName()
            ));
            _import.RemoveItem(_import.GetItemByHash(fileHashCode));
        }

        [TestMethod]
        public void ImportService_DuplicateImport_Test()
        {
            
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            AppSettingsProvider.Structure = "/xux_ssHHmm.ext";
            Assert.AreNotEqual(string.Empty,_import.Import(createAnImage.BasePath,false,false).FirstOrDefault());  
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));
            var itemFilePath = _query.GetItemByHash(fileHashCode);
            Assert.AreNotEqual(null, itemFilePath);
                        
            // Run a second time: > Must return nothing
            Assert.AreEqual(string.Empty,_import.Import(createAnImage.BasePath,false,false).FirstOrDefault());  
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));

            // Search on filename in database
//            Assert.AreEqual(true, _query.GetAllFiles().Any(p => p.FileName.Contains("xux_"))   );
            
            // Clean afterwards
            var importIndexItem = _import.GetItemByHash(fileHashCode);
            var outputFileName = importIndexItem.ParseFileName();
            var outputSubfolders = importIndexItem.ParseSubfolders();

            _import.RemoveItem(importIndexItem);
            File.Delete(FileIndexItem.DatabasePathToFilePath(
                importIndexItem.ParseSubfolders() + "/" + importIndexItem.ParseFileName()
            ));
        }

        
        [TestMethod]
        public void ImportService_DuplicateFileName_Test()
        {
            
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            AppSettingsProvider.Structure = "/xux_ssHHmm.ext";
            Assert.AreNotEqual(string.Empty,_import.Import(createAnImage.BasePath,false,false).FirstOrDefault());  
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));
            var itemFilePath = _query.GetItemByHash(fileHashCode);
            Assert.AreNotEqual(null, itemFilePath);
                        
            // Remove item from import index             // Run a second time: Now it not in the database
            var importIndexItem = _import.GetItemByHash(fileHashCode);
            _import.RemoveItem(importIndexItem);
            Assert.AreNotEqual(string.Empty,_import.Import(createAnImage.BasePath,false,false).FirstOrDefault());  
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));
            
            
            // >>>> ParentDirectory ===  /Users/dionvanvelde/.nuget/packages/microsoft.testplatform.testhost/15.7.2/lib/netstandard1.5
                
            // Search on filename in database
            var allXuXuFiles = _query.GetAllRecursive().Where(p => p.FileName.Contains("xux_")).ToList();
            Assert.AreEqual(true, allXuXuFiles.Any()  );

            // Clean afterwards
            importIndexItem = _import.GetItemByHash(fileHashCode);

            var outputFileName = importIndexItem.ParseFileName();
            var outputSubfolders = importIndexItem.ParseSubfolders();

            _import.RemoveItem(importIndexItem);

            foreach (var item in allXuXuFiles)
            {
                Console.WriteLine("item.FilePath" + item.FilePath);
                Console.WriteLine("---");
                Console.WriteLine(FileIndexItem.DatabasePathToFilePath(
                    item.FilePath
                ));
                //File.Delete(FileIndexItem.DatabasePathToFilePath(
                //    item.FilePath
                //));
            }
            
        }
        
        [TestMethod]
        public void ImportService_DuplicateDateStamp_Import_HHmmssImportTest()
        {
            
        }

        [TestMethod]
        public void ImportService_DeleteAfterTest_HHmmssImportTest()
        {
             // // Test if a source file is delete afterwards
            var createAnImage = new CreateAnImage();
            
            AppSettingsProvider.Structure = "/\\e\\x\\i\\s\\t/\\a\\b\\c/yyyy/mm/HHmmss.ext";
            AppSettingsProvider.BasePath = createAnImage.BasePath;

            Directory.CreateDirectory(AppSettingsProvider.BasePath + Path.DirectorySeparatorChar + "exist");

            var fullFilePath = createAnImage.FullFilePath.Replace("00", "01");
            
            try
            {
                File.Copy(createAnImage.FullFilePath,fullFilePath);
            }
            catch (IOException)
            {
            }
            
            var fileHashCode = FileHash.GetHashCode(fullFilePath);
            
            
            _import.Import(fullFilePath, true,false);  
            

            Assert.AreEqual(File.Exists(fullFilePath), false);

            var importIndexItem = _import.GetItemByHash(fileHashCode);
            var outputFileName = importIndexItem.ParseFileName();
            var outputSubfolders = importIndexItem.ParseSubfolders();

            _import.RemoveItem(importIndexItem);
            File.Delete(FileIndexItem.DatabasePathToFilePath(
                importIndexItem.ParseSubfolders() + "/" + importIndexItem.ParseFileName()
            ));
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
        public void ImportService_EntireBasePath_Folder_Import_ToFolderExist_Test()
        {
            // import folder
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.BasePath = createAnImage.BasePath;

            if (!Directory.Exists(AppSettingsProvider.BasePath + Path.DirectorySeparatorChar + "exist"))
            {
                Directory.CreateDirectory(AppSettingsProvider.BasePath + Path.DirectorySeparatorChar + "exist");
            }
            
            AppSettingsProvider.Structure = "/\\e\\x\\i\\s*/\\f\\o\\l\\d\\e\\r\\i\\m\\p\\o\\r\\t_HHssmm.ext";
            
            Assert.AreNotEqual(string.Empty,_import.Import(createAnImage.BasePath,false,false).FirstOrDefault());  // So testing the folder feature

            Assert.AreEqual(File.Exists(createAnImage.FullFilePath), true);

            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            var importIndexItem = _import.GetItemByHash(fileHashCode);
            var outputFileName = importIndexItem.ParseFileName();
            var outputSubfolders = importIndexItem.ParseSubfolders();

            _import.RemoveItem(importIndexItem);
            File.Delete(FileIndexItem.DatabasePathToFilePath(
                outputSubfolders + outputFileName
            ));
        }

        [TestMethod]
        public void ImportService_Import_NotFound_Test()
        {
            CollectionAssert.AreEqual(new List<string>(),_import.Import(null, true, false));
        }
    }
}