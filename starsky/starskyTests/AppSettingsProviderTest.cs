using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;
namespace starskytests
{
    [TestClass]
    public class AppSettingsProviderTest
    {
        [TestMethod]
        public void AppSettingsProviderTest_ReadOnlyFoldersTest()
        {
            AppSettingsProvider.ReadOnlyFolders = new List<string> {"test"};
            CollectionAssert.AreEqual(new List<string> {"test"}, AppSettingsProvider.ReadOnlyFolders);
        }

        
        [TestMethod]
        public void AppSettingsProviderTest_SqliteFullPathTest()
        {
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.sqlite;
            var datasource = AppSettingsProvider.SqliteFullPath("Data Source=data.db");
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.inmemorydatabase;
            Assert.AreEqual(datasource.Contains("data.db"),true);
            Assert.AreEqual(datasource.Contains("Data Source="),true);            
        }

        [TestMethod]
        public void AppSettingsProviderTest_SqliteFullPathentityframeworkcoreTest()
        {
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.sqlite;
            // This one should be ignored by SqliteFullPath so it must return ==> "Data Source=data.db"
            var datasource = AppSettingsProvider.SqliteFullPath("Data Source=data.db", Path.DirectorySeparatorChar + "entityframeworkcore" + Path.DirectorySeparatorChar);
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.inmemorydatabase;
            Assert.AreEqual(datasource,"Data Source=data.db"); 
        }
        
        [TestMethod]
        public void AppSettingsProviderTest_SqliteFullPathstarskycliTest()
        {
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.sqlite;
            var datasource = AppSettingsProvider.SqliteFullPath("Data Source=data.db", Path.DirectorySeparatorChar + "starsky-cli" + Path.DirectorySeparatorChar + "data.db");
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.inmemorydatabase;
            Assert.AreEqual(datasource.Contains("data.db"),true);
            Assert.AreEqual(datasource.Contains("Data Source="),true); 
        }
        
        [TestMethod]
        public void AppSettingsProviderTest_SqliteFullPathstarskyimportercliTest()
        {
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.sqlite;
            var datasource = AppSettingsProvider.SqliteFullPath("Data Source=data.db", Path.DirectorySeparatorChar + "starskyimportercli" + Path.DirectorySeparatorChar + "data.db");
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.inmemorydatabase;
            Assert.AreEqual(datasource.Contains("data.db"),true);
            Assert.AreEqual(datasource.Contains("Data Source="),true); 
        }

    }
}