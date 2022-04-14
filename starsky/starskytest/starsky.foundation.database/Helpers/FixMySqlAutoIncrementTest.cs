using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.database.Helpers;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.database.Helpers;

[TestClass]
public class FixMySqlAutoIncrementTest
{
	
	[TestMethod]
	public async Task FixMySqlAutoIncrement_NotUsed()
	{
		var result = await new FixMySqlAutoIncrement(null,new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		}).AutoIncrement("test");
		Assert.IsNull(result);
	}
		
	[TestMethod]
	public async Task FixMySqlAutoIncrement_ConnectionNullIgnore()
	{
		var result = await new FixMySqlAutoIncrement(null,new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql
		}).AutoIncrement("test");
		Assert.IsNull(result);
	}
	
		
	[TestMethod]
	public async Task CheckAutoIncrementExist_True()
	{
		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql,
			DatabaseConnection = "Server=localhost;port=4785;database=TEST;uid=starsky;pwd=TEST;maximumpoolsize=30;"
		};
		var mysqlConnection = new MySqlConnection(appSettings.DatabaseConnection);
		var result = await new FixMySqlAutoIncrement(mysqlConnection,appSettings).CheckAutoIncrementExist("test");
		Assert.IsTrue(result);
	}
	
	[TestMethod]
	public async Task CheckAutoIncrementExist_Null()
	{
		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql,
		};
		var result = await new FixMySqlAutoIncrement(null,appSettings).CheckAutoIncrementExist("test");
		Assert.IsNull(result);
	}
			
	[TestMethod]
	public async Task AlterTable_NoConnection()
	{
		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql,
			DatabaseConnection = "Server=localhost;port=4785;database=TEST;uid=starsky;pwd=TEST;maximumpoolsize=30;"
		};
		var mysqlConnection = new MySqlConnection(appSettings.DatabaseConnection);
		var result = await new FixMySqlAutoIncrement(mysqlConnection,appSettings).AlterTable("test");
		Assert.IsNull(result);
	}
	
	[TestMethod]
	public async Task AlterTable_Null()
	{
		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql,
		};
		var result = await new FixMySqlAutoIncrement(null,appSettings).AlterTable("test");
		Assert.IsNull(result);
	}
}
