using System;
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
	public async Task AutoIncrement_NotUsed()
	{
		var result = await new MySqlDatabaseFixes(null,new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		}).FixAutoIncrement("test");
		Assert.IsNull(result);
	}
	
	[TestMethod]
	public async Task AutoIncrement_NotUsed1()
	{
		var result = await new MySqlDatabaseFixes(new MySqlConnection(),new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		}).FixAutoIncrement("test");
		Assert.IsNull(result);
	}
	
	[TestMethod]
	[ExpectedException(typeof(MySqlException))]
	public async Task AutoIncrement_MySqlException()
	{
		var fakeConnectionString =
			"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
		var result = await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql
		}).FixAutoIncrement("test");
		Assert.IsNull(result);
	}
		
	[TestMethod]
	public async Task FixMySqlAutoIncrement_ConnectionNullIgnore()
	{
		var result = await new MySqlDatabaseFixes(null,new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql
		}).FixAutoIncrement("test");
		Assert.IsNull(result);
	}
	
		
	[TestMethod]
	[ExpectedException(typeof(InvalidOperationException))]
	public async Task CheckAutoIncrementExist_True()
	{
		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql,
			DatabaseConnection = "Server=localhost;port=4785;database=TEST;uid=starsky;pwd=TEST;maximumpoolsize=30;"
		};
		var mysqlConnection = new MySqlConnection(appSettings.DatabaseConnection);
		var result = await new MySqlDatabaseFixes(mysqlConnection,appSettings).CheckAutoIncrementExist("test");
		Assert.IsTrue(result);
	}
	
	[TestMethod]
	public async Task CheckAutoIncrementExist_Null()
	{
		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql,
		};
		var result = await new MySqlDatabaseFixes(null,appSettings).CheckAutoIncrementExist("test");
		Assert.IsNull(result);
	}
			
	[TestMethod]
	[ExpectedException(typeof(InvalidOperationException))]
	public async Task AlterTable_NoConnection()
	{
		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql,
			DatabaseConnection = "Server=localhost;port=4785;database=TEST;uid=starsky;pwd=TEST;maximumpoolsize=30;"
		};
		var mysqlConnection = new MySqlConnection(appSettings.DatabaseConnection);
		var result = await new MySqlDatabaseFixes(mysqlConnection,appSettings).AlterTableAutoIncrement("test");
		Assert.IsNull(result);
	}
	
	[TestMethod]
	public async Task AlterTable_Null()
	{
		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql,
		};
		var result = await new MySqlDatabaseFixes(null,appSettings).AlterTableAutoIncrement("test");
		Assert.IsNull(result);
	}
}
