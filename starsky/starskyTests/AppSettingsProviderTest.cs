using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;
namespace starskytests
{
    [TestClass]
    public class AppSettingsProviderTest
    {
        [TestMethod]
        public void SqliteFullPathTest()
        {
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.sqlite;
            var datasource = AppSettingsProvider.SqliteFullPath("Data Source=data.db");
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.inmemorydatabase;
            Assert.AreEqual(datasource.Contains("data.db"),true);
            Assert.AreEqual(datasource.Contains("Data Source="),true);            
        }

        [TestMethod]
        public void SqliteFullPathentityframeworkcoreTest()
        {
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.sqlite;
            // This one should be ignored ==>
            var datasource = AppSettingsProvider.SqliteFullPath("Data Source=data.db", Path.DirectorySeparatorChar + "entityframeworkcore" + Path.DirectorySeparatorChar);
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.inmemorydatabase;
            Assert.AreEqual(datasource.Contains("data.db"),true);
            Assert.AreEqual(datasource.Contains("Data Source="),true); 
        }
        
        [TestMethod]
        public void SqliteFullPathstarskycliTest()
        {
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.sqlite;
            var datasource = AppSettingsProvider.SqliteFullPath("Data Source=data.db", Path.DirectorySeparatorChar + "starsky-cli" + Path.DirectorySeparatorChar + "data.db");
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.inmemorydatabase;
            Assert.AreEqual(datasource.Contains("data.db"),true);
            Assert.AreEqual(datasource.Contains("Data Source="),true); 
        }
        
        [TestMethod]
        public void SqliteFullPathstarskyimportercliTest()
        {
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.sqlite;
            var datasource = AppSettingsProvider.SqliteFullPath("Data Source=data.db", Path.DirectorySeparatorChar + "starskyimportercli" + Path.DirectorySeparatorChar + "data.db");
            AppSettingsProvider.DatabaseType = AppSettingsProvider.DatabaseTypeList.inmemorydatabase;
            Assert.AreEqual(datasource.Contains("data.db"),true);
            Assert.AreEqual(datasource.Contains("Data Source="),true); 
        }

    }
}