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
        
    }
}