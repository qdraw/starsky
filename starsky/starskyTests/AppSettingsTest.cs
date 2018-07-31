using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Middleware;
using starsky.Models;
namespace starskytests
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
            var datasource = _appSettings.SqliteFullPath("Data Source=data.db",null);
            Assert.AreEqual(datasource.Contains("data.db"),true);
            Assert.AreEqual(datasource.Contains("Data Source="),true);    
            Assert.AreEqual(datasource, "Data Source=data.db");
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
        

    }
}