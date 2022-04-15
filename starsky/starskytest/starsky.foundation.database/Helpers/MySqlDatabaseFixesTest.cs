using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.database.Helpers;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.database.Helpers;

[TestClass]
public class MySqlDatabaseFixesTest
{

	[TestMethod]
	public async Task FixUtf8Encoding_Null()
	{
		var result = await new MySqlDatabaseFixes(null,new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		}).FixUtf8Encoding(new List<string>());
		Assert.IsNull(result);
	}
	
	[TestMethod]
	[ExpectedException(typeof(InvalidOperationException))]
	public async Task FixUtf8Encoding_InvalidOperationException()
	{
		var fakeConnectionString =
			"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
		var result = await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql
		}).FixUtf8Encoding(new List<string>{""});
		Assert.IsNull(result);
	}
	
	[TestMethod]
	public async Task SetTableToUtf8_Null()
	{
		var result = await new MySqlDatabaseFixes(null,new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		}).SetTableToUtf8("");
		Assert.IsNull(result);
	}
	
	[TestMethod]
	[ExpectedException(typeof(InvalidOperationException))]
	public async Task SetTableToUtf8_InvalidOperationException()
	{
		var fakeConnectionString =
			"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
		var result = await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql
		}).SetTableToUtf8("");
		Assert.IsNull(result);
	}
	
	[TestMethod]
	public async Task SetDatabaseSettingToUtf8_Null()
	{
		var result = await new MySqlDatabaseFixes(null,new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		}).SetDatabaseSettingToUtf8();
		Assert.IsNull(result);
	}
	
	[TestMethod]
	[ExpectedException(typeof(InvalidOperationException))]
	public async Task SetDatabaseSettingToUtf8_InvalidOperationException()
	{
		var fakeConnectionString =
			"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
		var result = await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql
		}).SetDatabaseSettingToUtf8();
		Assert.IsNull(result);
	}
	
	[TestMethod]
	[ExpectedException(typeof(InvalidOperationException))]
	public async Task ExecuteNonQueryAsync_InvalidOperationException()
	{
		var fakeConnectionString =
			"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
		await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql
		}).ExecuteNonQueryAsync("");
	}
	
	[TestMethod]
	public async Task IsUtf8_Null()
	{
		var result = await new MySqlDatabaseFixes(null,new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		}).IsUtf8();
		Assert.IsNull(result);
	}
	
	[TestMethod]
	[ExpectedException(typeof(InvalidOperationException))]
	public async Task IsUtf8_InvalidOperationException()
	{
		var fakeConnectionString =
			"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
		var result = await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql
		}).IsUtf8();
		Assert.IsNull(result);
	}
	
	[TestMethod]
	public async Task OpenConnection_Null()
	{
		MySqlConnection connection = null;
		await new MySqlDatabaseFixes(connection,new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		}).OpenConnection();
		Assert.IsNull(connection);
	}
	
	[TestMethod]
	[ExpectedException(typeof(MySqlException))]
	public async Task OpenConnection_MySqlConnector_MySqlException()
	{
		var fakeConnectionString =
			"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
		await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.Mysql
		}).OpenConnection();
	}
	
	[TestMethod]
	public async Task FixAutoIncrement_NotUsed()
	{
		var result = await new MySqlDatabaseFixes(null,new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		}).FixAutoIncrement("test");
		Assert.IsNull(result);
	}
	
	[TestMethod]
	public async Task FixAutoIncrement_NotUsed1()
	{
		var result = await new MySqlDatabaseFixes(new MySqlConnection(),new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		}).FixAutoIncrement("test");
		Assert.IsNull(result);
	}
	
	[TestMethod]
	[ExpectedException(typeof(InvalidOperationException))]
	public async Task FixAutoIncrement_InvalidOperationException()
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
	public async Task FixAutoIncrement_ConnectionNullIgnore()
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
