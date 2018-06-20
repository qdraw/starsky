using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Data;
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

//        [TestMethod]
//        public void ImportServiceCheckIfSubDirectoriesExistTest()
//        {
//            var createAnImage = new CreateAnImage();
//            var importItem = new ImportIndexItem();
//            importItem.SourceFullFilePath = createAnImage.FullFilePath;
//            _import.CheckIfSubDirectoriesExist(importItem.ParseSubfolders());
//        }

//        [TestMethod]
//        public void ImportServiceImportTest()
//        {
//            var createAnImage = new CreateAnImage();
//            // using default structure
//            AppSettingsProvider.BasePath = createAnImage.BasePath;
//            _import.Import(createAnImage.FullFilePath);
//        }
        
        [TestMethod]
        public void ImportServiceyyyyMMdd_HHmmssImportTest()
        {
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.Structure = "/xxxxx__yyyyMMdd_HHmmss.ext";
            // This is not to be the first file in the test directory
            // => otherwise SyncServiceFirstItemDirectoryTest() will fail
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            var importedFile = _import.Import(createAnImage.FullFilePath);
            
            var fileHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            
            Assert.AreEqual(true, _import.IsHashInDatabase(fileHashCode));

            // Clean file after succesfull run;
            var fileIndexItem = ExifRead.ReadExifFromFile(createAnImage.FullFilePath);
            var file = new ImportIndexItem {SourceFullFilePath = createAnImage.FullFilePath,  DateTime = fileIndexItem.DateTime}.ParseFileName();
            
            File.Delete(FileIndexItem.DatabasePathToFilePath(file));
        }
        
        
    }
}