using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Data;
using starsky.Helpers;
using starsky.Models;
using starsky.Services;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Middleware;
using starskycore.Models;
using starskycore.Services;
using starskytests.FakeCreateAn;
using starskytests.Models;
using Query = starsky.core.Services.Query;
using SyncService = starskycore.Services.SyncService;

namespace starskytests.Services
{
    [TestClass]
    public class ImportServiceTest
    {
        private ImportService _import;
        private Query _query;
        private SyncService _isync;
        private IExiftool _exiftool;
        private readonly AppSettings _appSettings;
        private CreateAnImage _createAnImage;
        private readonly ReadMeta _readmeta;

        public ImportServiceTest()
        {
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            var memoryCache = provider.GetService<IMemoryCache>();
            
            var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
            builderDb.UseInMemoryDatabase("ImportServiceTest");
            var options = builderDb.Options;
            var context = new ApplicationDbContext(options);
            _query = new Query(context,memoryCache);
            
            // Inject Fake Exiftool; dependency injection
            var services = new ServiceCollection();
            services.AddSingleton<IExiftool, FakeExiftool>();    
            
            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            // random config
            _createAnImage = new CreateAnImage();
            var dict = new Dictionary<string, string>
            {
                { "App:StorageFolder", _createAnImage.BasePath },
                { "App:ThumbnailTempFolder",_createAnImage.BasePath },
                { "App:Verbose", "true" }
            };
            // Start using dependency injection
            var builder = new ConfigurationBuilder();  
            // Add random config to dependency injection
            builder.AddInMemoryCollection(dict);
            // build config
            var configuration = builder.Build();
            // inject config as object to a service
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
            // build the service
            var serviceProvider = services.BuildServiceProvider();
            // get the service
            _appSettings = serviceProvider.GetRequiredService<AppSettings>();
           
            // inject exiftool
            _exiftool = serviceProvider.GetRequiredService<IExiftool>();
            
            _readmeta = new ReadMeta(_appSettings);

            _isync = new SyncService(context, _query,_appSettings,_readmeta);
            
            //   _context = context
            //   _isync = isync
            //   _exiftool = exiftool
            //   _appSettings = appSettings
            _import = new ImportService(context,_isync,_exiftool,_appSettings,_readmeta,null);
            
            // Delete gpx files before importing
            // to avoid 1000 files in this folder
            foreach (string f in Directory.EnumerateFiles(_appSettings.StorageFolder,"*.gpx"))
            {
                File.Delete(f);
            }
        }
        
//        public ImportServiceTest()
//        {
//            var provider = new ServiceCollection()
//                .AddMemoryCache()
//                .BuildServiceProvider();
//            var memoryCache = provider.GetService<IMemoryCache>();
//            
//            var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
//            builderDb.UseInMemoryDatabase("importservice");
//            var options = builderDb.Options;
//            var context = new ApplicationDbContext(options);
//            _query = new Query(context,memoryCache);
//            
//            // Inject Fake Exiftool; dependency injection
//            // Add a dependency injection feature
//            var services = new ServiceCollection();
//            services.AddSingleton<IExiftool, FakeExiftool>();      
//
//            
//            // Inject Config helper
//            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
//            // random config
//            var newImage = new CreateAnImage();
//            var dict = new Dictionary<string, string>
//            {
//                { "App:StorageFolder", newImage.BasePath },
//                { "App:Verbose", "true" }
//            };
//            // Start using dependency injection
//            var builder = new ConfigurationBuilder();  
//            // Add random config to dependency injection
//            builder.AddInMemoryCollection(dict);
//            // build config
//            var configuration = builder.Build();
//            // inject config as object to a service
//            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
//            // build the service
//            var serviceProvider = services.BuildServiceProvider();
//            _exiftool = serviceProvider.GetRequiredService<IExiftool>();
//            
//            // get the service
//            _appSettings = serviceProvider.GetRequiredService<AppSettings>();
//            
//            _isync = new SyncService(context, _query,_appSettings);
//
//            Console.WriteLine(_appSettings);
//            //   _context = context
//            //   _isync = isync
//            //   _exiftool = exiftool
//            //   _appSettings = appSettings
//            _import = new ImportService(context,_isync,_exiftool,_appSettings);
//        }

        
        [TestMethod]
        public void ImportService_NoSubPath_slashyyyyMMdd_HHmmss_ImportTest()
        {
            var createAnImage = new CreateAnImage();
            _appSettings.Structure = "/xxxxx__yyyyMMdd_HHmmss.ext";
            // This is not to be the first file in the test directory
            // => otherwise SyncServiceFirstItemDirectoryTest() will fail
            _appSettings.StorageFolder = createAnImage.BasePath;
            var importSettings = new ImportSettingsModel();
            importSettings.DeleteAfter = false;
            importSettings.AgeFileFilterDisabled = false;
            
            _import.Import(createAnImage.FullFilePath,importSettings);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));

            // Clean file after succesfull run;
            var fileIndexItem = _readmeta.ReadExifFromFile(createAnImage.FullFilePath);
            var importIndexItem = new ImportIndexItem(_appSettings)
            {
                SourceFullFilePath = createAnImage.FullFilePath,  
                DateTime = fileIndexItem.DateTime
            };
            File.Delete(_appSettings.DatabasePathToFilePath(
                importIndexItem.ParseSubfolders() + importIndexItem.ParseFileName()));
            _import.RemoveItem(_import.GetItemByHash(fileHashCode));
        }
        
        [TestMethod]
        public void ImportService_AsteriskTRFolderHHmmss_ImportTest()
        {
            var createAnImage = new CreateAnImage();
            _appSettings.Structure = "/\\t\\r*/HHmmss_\\d.ext";
            _appSettings.StorageFolder = createAnImage.BasePath;

            Console.WriteLine(createAnImage.FullFilePath);
            var importSettings = new ImportSettingsModel
            {
                DeleteAfter = false,
                AgeFileFilterDisabled = false
            };
            _import.Import(createAnImage.FullFilePath,importSettings);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));

            // Clean file after succesfull run;
            var fileIndexItem = _readmeta.ReadExifFromFile(createAnImage.FullFilePath);
            var importIndexItem = new ImportIndexItem(_appSettings)
            {
                SourceFullFilePath = createAnImage.FullFilePath,  
                DateTime = fileIndexItem.DateTime
            };

            Console.WriteLine("---");
            Console.WriteLine();

            var path = _appSettings.DatabasePathToFilePath(
                importIndexItem.ParseSubfolders() + importIndexItem.ParseFileName()
            );

            File.Delete(path);

            var itemByHash = _import.GetItemByHash(fileHashCode);
            
            _import.RemoveItem(itemByHash);
        }
        
        
        [TestMethod]
        public void ImportService_NonExistingFolder_HHmmssImportTest()
        {
            var createAnImage = new CreateAnImage();
            _appSettings.StorageFolder = createAnImage.BasePath;

            var existDir = _appSettings.StorageFolder + Path.DirectorySeparatorChar + "exist";
            if (!Directory.Exists(existDir))
            {
                Directory.CreateDirectory(_appSettings.StorageFolder + Path.DirectorySeparatorChar + "exist");
            }
            var importSettings = new ImportSettingsModel
            {
                DeleteAfter = false,
                AgeFileFilterDisabled = false
            };
            
            _appSettings.Structure = "/\\e\\x\\i\\s\\t/\\a\\b\\c/HHmmss.ext";
            _import.Import(createAnImage.FullFilePath,importSettings);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));

            // Clean file after succesfull run;
            var fileIndexItem = _readmeta.ReadExifFromFile(createAnImage.FullFilePath);
            var importIndexItem = new ImportIndexItem(_appSettings)
            {
                SourceFullFilePath = createAnImage.FullFilePath,  DateTime = fileIndexItem.DateTime
            };
            File.Delete(_appSettings.DatabasePathToFilePath(
                importIndexItem.ParseSubfolders() + "/" + importIndexItem.ParseFileName()
            ));
            
            Files.DeleteDirectory(_appSettings.DatabasePathToFilePath(importIndexItem.ParseSubfolders()));
            _import.RemoveItem(_import.GetItemByHash(fileHashCode));
            Files.DeleteDirectory(existDir);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ImportService_WithoutExt_ImportTest()
        {
            // We don't allow no extension
            var createAnImage = new CreateAnImage();
            
            _appSettings.StorageFolder = createAnImage.BasePath;
            if (!Directory.Exists(_appSettings.StorageFolder + Path.DirectorySeparatorChar + "exist"))
            {
                Directory.CreateDirectory(_appSettings.StorageFolder + Path.DirectorySeparatorChar + "exist");
            }
            
            _appSettings.Structure = "/\\e\\x\\i\\s*/ssHHmm";
            var importSettings = new ImportSettingsModel
            {
                DeleteAfter = false,
                AgeFileFilterDisabled = false
            };
            _import.Import(createAnImage.FullFilePath,importSettings);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));

            // Clean file after succesfull run;
            var fileIndexItem = _readmeta.ReadExifFromFile(createAnImage.FullFilePath);
            var importIndexItem = new ImportIndexItem(_appSettings)
            {
                SourceFullFilePath = createAnImage.FullFilePath,  DateTime = fileIndexItem.DateTime
            };
            File.Delete(_appSettings.DatabasePathToFilePath(
                importIndexItem.ParseSubfolders() + "/" + importIndexItem.ParseFileName()
            ));
            _import.RemoveItem(_import.GetItemByHash(fileHashCode));
        }

        [TestMethod]
        public void ImportService_DuplicateImport_Test()
        {
            
            var createAnImage = new CreateAnImage();
            _appSettings.StorageFolder = createAnImage.BasePath;
            
            Console.WriteLine(_appSettings.StorageFolder);
            
            _appSettings.Structure = "/xuxuxuxu_ssHHmm.ext";


            var importSettings = new ImportSettingsModel();
            importSettings.DeleteAfter = false;
            importSettings.AgeFileFilterDisabled = false;
            Assert.AreNotEqual(string.Empty,_import.Import(createAnImage.BasePath,importSettings).FirstOrDefault());  
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));
            var itemFilePath = _query.GetItemByHash(fileHashCode);
            Assert.AreNotEqual(null, itemFilePath);
                        
            // Run a second time: > Must return nothing
            var returnObject = _import.Import(createAnImage.BasePath, importSettings);
            Assert.AreEqual(true, string.IsNullOrWhiteSpace(returnObject.FirstOrDefault()) );  
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));

            // Search on filename in database
            Assert.AreEqual(true, _query.GetAllFiles().Any(p => p.FileName.Contains("xuxuxuxu_"))   );
            
            // Clean afterwards
            var importIndexItem = _import.GetItemByHash(fileHashCode);
            var outputFileName = importIndexItem.ParseFileName();
            var outputSubfolders = importIndexItem.ParseSubfolders();

            _import.RemoveItem(importIndexItem);
            string[] picList = Directory.GetFiles(_appSettings.DatabasePathToFilePath(
                importIndexItem.ParseSubfolders()), "xuxuxuxu_*.jpg");
            // Delete source files that were copied.
            foreach (string f in picList)
            {
                File.Delete(f);
            }

        }

        
        [TestMethod]
        public void ImportService_DuplicateFileName_Test()
        {
            var importSettings = new ImportSettingsModel
            {
                DeleteAfter = false,
                AgeFileFilterDisabled = false
            };
            var createAnImage = new CreateAnImage();
            _appSettings.StorageFolder = createAnImage.BasePath;
            _appSettings.Structure = "/xux99999xxxx_ssHHmm.ext";
            Assert.AreNotEqual(string.Empty,_import.Import(createAnImage.BasePath,importSettings).FirstOrDefault());  
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));
            var itemFilePath = _query.GetItemByHash(fileHashCode);
            Assert.AreNotEqual(null, itemFilePath);
                        
            // Remove item from import index             // Run a second time: Now it not in the database
            var importIndexItem = _import.GetItemByHash(fileHashCode);
            _import.RemoveItem(importIndexItem);
            Assert.AreNotEqual(string.Empty,_import.Import(createAnImage.BasePath,importSettings).FirstOrDefault());  
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));
            
            
            // >>>> ParentDirectory ===
            // /Users/dionvanvelde/.nuget/packages/microsoft.testplatform.testhost/15.7.2/lib/netstandard1.5
                
            // Search on filename in database
            var allXuXuFiles = _query.GetAllRecursive().Where(p => p.FileName.Contains("xux99999xxxx_")).ToList();
            Assert.AreEqual(true, allXuXuFiles.Any()  );

            // Clean afterwards
            importIndexItem = _import.GetItemByHash(fileHashCode);

            var outputFileName = importIndexItem.ParseFileName();
            var outputSubfolders = importIndexItem.ParseSubfolders();

            _import.RemoveItem(importIndexItem);

            string[] picList = Directory.GetFiles(_appSettings.DatabasePathToFilePath(
                importIndexItem.ParseSubfolders()), "xux99999xxxx_*.jpg");
            // Delete source files that were copied.
            foreach (string f in picList)
            {
                File.Delete(f);
            }
                        
        }
        

        [TestMethod]
        public void ImportService_DeleteAfterTest_HHmmssImportTest()
        {
             // // Test if a source file is delete afterwards
            var createAnImage = new CreateAnImage();
            
            _appSettings.Structure = "/\\e\\x\\i\\s\\t/\\a\\b\\c/yyyy/mm/HHmmss.ext";
            _appSettings.StorageFolder = createAnImage.BasePath;

            var existDirectoryFullPath = _appSettings.StorageFolder + Path.DirectorySeparatorChar + "exist";
            Directory.CreateDirectory(existDirectoryFullPath);

            var fullFilePath = createAnImage.FullFilePath.Replace("00", "01");
            
            try
            {
                File.Copy(createAnImage.FullFilePath,fullFilePath);
            }
            catch (IOException)
            {
            }
            
            var fileHashCode = FileHash.GetHashCode(fullFilePath);
            
            var importSettings = new ImportSettingsModel
            {
                DeleteAfter = true,
                AgeFileFilterDisabled = false
            };
            _import.Import(fullFilePath, importSettings);

            Console.WriteLine(File.Exists(fullFilePath));
            Assert.AreEqual(File.Exists(fullFilePath), false);

            var importIndexItem = _import.GetItemByHash(fileHashCode);
            var outputFileName = importIndexItem.ParseFileName();
            var outputSubfolders = importIndexItem.ParseSubfolders();

            _import.RemoveItem(importIndexItem);
            File.Delete(_appSettings.DatabasePathToFilePath(
                importIndexItem.ParseSubfolders() + "/" + importIndexItem.ParseFileName()
            ));
            // delete exist dir
            Files.DeleteDirectory(existDirectoryFullPath);
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ImportService_NonExistingImportFail_ImportTest()
        {
            // Get an Not found error when parsing
            var importIndexItem = new ImportIndexItem(_appSettings)
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
            _appSettings.StorageFolder = createAnImage.BasePath;

            Console.WriteLine(createAnImage.BasePath);

            var existFolderPath = _appSettings.StorageFolder + Path.DirectorySeparatorChar + "exist";
            if (!Directory.Exists(existFolderPath))
            {
                Directory.CreateDirectory(existFolderPath);
            }
            
            _appSettings.Structure = "/\\e\\x\\i\\s*/\\f\\o\\l\\d\\e\\r\\i\\m\\p\\o\\r\\t_HHssmm.ext";
            var importSettings = new ImportSettingsModel
            {
                DeleteAfter = false,
                AgeFileFilterDisabled = false
            };
            Assert.AreNotEqual(string.Empty,_import.Import(
                createAnImage.BasePath,importSettings).FirstOrDefault());  // So testing the folder feature

            Assert.AreEqual(File.Exists(createAnImage.FullFilePath), true);

            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            var importIndexItem = _import.GetItemByHash(fileHashCode);
            var outputFileName = importIndexItem.ParseFileName();
            var outputSubfolders = importIndexItem.ParseSubfolders();

            _import.RemoveItem(importIndexItem);
            File.Delete(_appSettings.DatabasePathToFilePath(
                outputSubfolders + outputFileName
            ));
            
            // existFolderPath >= remove it afterwards
            Files.DeleteDirectory(existFolderPath);
        }

        [TestMethod]
        public void ImportService_Import_NotFound_Test()
        {
            var importSettings = new ImportSettingsModel
            {
                DeleteAfter = true,
                AgeFileFilterDisabled = false
            };
            CollectionAssert.AreEqual(new List<string>(),_import.Import(string.Empty, importSettings));
        }
        
        [TestMethod]
        public void ImportService_inputFullPathList_ListInput_ImportTest()
        {
            // test for: Import(IEnumerable<string> inputFullPathList, bool deleteAfter = false, bool ageFileFilter = true)
            var createAnImage = new CreateAnImage();
            _appSettings.Structure = "/xxxxx__yyyyMMdd_HHmmss.ext";
            // This is not to be the first file in the test directory
            // => otherwise SyncServiceFirstItemDirectoryTest() will fail
            _appSettings.StorageFolder = createAnImage.BasePath;
            
            // The only difference is that the item is in a list
            var storeItemInList = new List<string>{createAnImage.FullFilePath};
            var importSettings = new ImportSettingsModel
            {
                DeleteAfter = false,
                AgeFileFilterDisabled = false
            };
            _import.Import(storeItemInList,importSettings);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            Assert.AreEqual(true, _import.IsHashInImportDb(fileHashCode));

            // Clean file after succesfull run;
            var fileIndexItem = _readmeta.ReadExifFromFile(createAnImage.FullFilePath);
            var importIndexItem = new ImportIndexItem(_appSettings)
            {
                SourceFullFilePath = createAnImage.FullFilePath,  
                DateTime = fileIndexItem.DateTime
            };
            File.Delete(_appSettings.DatabasePathToFilePath(importIndexItem.ParseSubfolders() + importIndexItem.ParseFileName()));
            _import.RemoveItem(_import.GetItemByHash(fileHashCode));
        }

        [TestMethod]
        public void ImportService_overWriteStructure_MatchItem_ObjectCreateIndexItem()
        {
            var importSettings = new ImportSettingsModel {Structure = "/HHmmss_yyyyMMdd.ext"};
            var importIndexItem = _import.ObjectCreateIndexItem(string.Empty, string.Empty, new FileIndexItem(),
                importSettings.Structure);
            
            Assert.AreEqual(importSettings.Structure,importIndexItem.Structure);
        }
        
        [TestMethod]
        public void ImportService_overWriteStructure_NullIgnore_ObjectCreateIndexItem()
        {
            var importSettings = new ImportSettingsModel {Structure = string.Empty};
            var importIndexItem = _import.ObjectCreateIndexItem(string.Empty, string.Empty, new FileIndexItem(),
                importSettings.Structure);
            
            // Fallback to system settings
            Assert.AreEqual("/yyyy/MM/yyyy_MM_dd/yyyyMMdd_HHmmss_{filenamebase}.ext",importIndexItem.Structure);
        }

        [TestMethod]
        public void ImportService_ImportAndIgnore_ImportTest()
        {
            var createAnImage = new CreateAnImage();
            _appSettings.Structure = null;
            _appSettings.Verbose = true;
            _appSettings.StorageFolder = createAnImage.BasePath;
            var importSettings = new ImportSettingsModel
            {
                DeleteAfter = true,
                AgeFileFilterDisabled = false,
                Structure = "/HHmmss_yyyyMMdd.ext"
            };
            
            var createAnImageNoExif = new CreateAnImageNoExif();

            var result = _import.Import(createAnImageNoExif.FullFilePathWithDate,importSettings);
            
            Assert.AreEqual(string.Empty,result.FirstOrDefault());
            Files.DeleteFile(createAnImageNoExif.FullFilePathWithDate);
        }
        

        
    }
}
