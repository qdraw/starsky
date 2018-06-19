﻿using Microsoft.EntityFrameworkCore;
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

        [TestMethod]
        public void ImportServiceCheckIfSubDirectoriesExistTest()
        {
            var createAnImage = new CreateAnImage();
            var importItem = new ImportIndexItem();
            importItem.SourceFullFilePath = createAnImage.FullFilePath;
            _import.CheckIfSubDirectoriesExist(importItem.ParseSubfolders());
        }

        [TestMethod]
        public void ImportServiceImportTest()
        {
            var createAnImage = new CreateAnImage();
            // using default structure
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            _import.Import(createAnImage.FullFilePath);
        }
        
        [TestMethod]
        public void ImportServiceYYYYMMdd_HHmmssImportTest()
        {
            var createAnImage = new CreateAnImage();
            AppSettingsProvider.Structure = "/yyyyMMdd_HHmmss.ext";
            AppSettingsProvider.BasePath = createAnImage.BasePath;
            _import.Import(createAnImage.FullFilePath);
        }
    }
}