using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Middleware;
using starsky.Models;

namespace starskytests.Models
{
    [TestClass]
    public class AppSettingsProviderTest
    {
        private readonly AppSettings _appSettings;

        public AppSettingsProviderTest()
        {
            // Add a dependency injection feature
            var services = new ServiceCollection();
            // Inject Config helper
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            // random config
            var newImage = new CreateAnImage();
            var dict = new Dictionary<string, string>
            {
                { "App:StorageFolder", newImage.BasePath },
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
        }
        
        [TestMethod]
        public void AppSettingsProviderTest_ReadOnlyFoldersTest()
        {
            _appSettings.ReadOnlyFolders = new List<string> {"test"};
            CollectionAssert.AreEqual(new List<string> {"test"}, _appSettings.ReadOnlyFolders);
        }

        
        [TestMethod]
        public void AppSettingsProviderTest_SqliteFullPathTest()
        {
            var datasource = _appSettings.SqliteFullPath("Data Source=data.db",string.Empty);
            Assert.AreEqual(datasource.Contains("data.db"),true);
            Assert.AreEqual(datasource.Contains("Data Source="),true);    
        }

       
        [TestMethod]
        public void AppSettingsProviderTest_SqliteFullPathstarskycliTest()
        {
            _appSettings.DatabaseType = AppSettings.DatabaseTypeList.Sqlite;

            var datasource = _appSettings.SqliteFullPath(
                "Data Source=data.db",  Path.DirectorySeparatorChar + "starsky");
            Assert.AreEqual(datasource.Contains("data.db"),true);
            Assert.AreEqual(datasource.Contains("Data Source="),true); 
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AppSettingsProviderTest_SQLite_ExpectException()
        {
            _appSettings.DatabaseType = AppSettings.DatabaseTypeList.Sqlite;
            var datasource = _appSettings.SqliteFullPath(string.Empty,null);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AppSettingsProviderTest_MySQL_ExpectException()
        {
            _appSettings.DatabaseType = AppSettings.DatabaseTypeList.Mysql;
            _appSettings.SqliteFullPath(string.Empty,null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AppSettingsProviderTest_StructureFails_withoutExtAndNoSlash()
        {
            _appSettings.Structure = "\\d";
            Assert.AreEqual("d", _appSettings.Structure);
        }

        [TestMethod]
        public void AppSettingsProviderTest_StructureFails_withExtAndNoSlash()
        {
            _appSettings.Structure = "\\d.ext";
            Assert.AreEqual("/\\d.ext", _appSettings.Structure);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AppSettingsProviderTest_StructureCheck_MissingFirstSlash()
        {
            AppSettings.StructureCheck("d/test.ext");
            // >= ArgumentException
        }

        [TestMethod]
        public void AppSettingsProviderTest_FolderWithFirstSlash()
        {
            AppSettings.StructureCheck("/d/dion.ext");
        }
        
        [TestMethod]
        public void AppSettingsProviderTest_NoFolderWithFirstSlash()
        {
            AppSettings.StructureCheck("/dion.ext");
        }  
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AppSettingsProviderTest_NoFolderMissingFirstSlash()
        {
            AppSettings.StructureCheck("dion.ext");
            // >= ArgumentException
        }

        [TestMethod]
        public void AppSettingsStructureExampleNoSetting()
        {
            var content = _appSettings.StructureExampleNoSetting;
            Assert.AreEqual(content.Contains(DateTime.Now.Year.ToString()), true );
        }

        [TestMethod]
        public void AppSettingsNameNullTest()
        {
            var appSettings = new AppSettings{Name = null};
            Assert.AreEqual(string.Empty,appSettings.Name);
        }
        
        [TestMethod]
        public void AppSettingsNameNoWebSafeNameTest()
        {
            var appSettings = new AppSettings{Name = "Non websafe name"};
            Assert.AreEqual("non-websafe-name/",appSettings.GetWebSafeReplacedName("{name}"));
            
        }
    }
}