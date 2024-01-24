using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.database.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Helpers
{
	
	[TestClass]
	public sealed class MySqlDatabaseFixesTest
	{

		[TestMethod]
		public async Task FixUtf8Encoding_Null()
		{
			var result = await new MySqlDatabaseFixes(null,new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			},new FakeIWebLogger()).FixUtf8Encoding(new List<string?>());
			Assert.IsNull(result);
		}
	
		[TestMethod]
		public async Task FixUtf8Encoding_MissingDatabase()
		{
			var fakeConnectionString =
				"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
			var result = await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql
			},new FakeIWebLogger()).FixUtf8Encoding(new List<string?>{""});
			Assert.IsTrue(result);
		}
	
		[TestMethod]
		public async Task SetTableToUtf8_Null()
		{
			var result = await new MySqlDatabaseFixes(null,new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			},new FakeIWebLogger()).SetTableToUtf8("");
			Assert.IsNull(result);
		}
	
		[TestMethod]
		public async Task SetTableToUtf8_InvalidOperationException()
		{
			var fakeConnectionString =
				"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
			var result = await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql
			},new FakeIWebLogger()).SetTableToUtf8("");
			Assert.IsTrue(result);
		}
	
		[TestMethod]
		public async Task SetDatabaseSettingToUtf8_Null()
		{
			var result = await new MySqlDatabaseFixes(null,new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			},new FakeIWebLogger()).SetDatabaseSettingToUtf8();
			Assert.IsNull(result);
		}
	
		[TestMethod]
		public async Task SetDatabaseSettingToUtf8_InvalidOperationException()
		{
			var fakeConnectionString =
				"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
			var result = await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql
			},new FakeIWebLogger()).SetDatabaseSettingToUtf8();
			Assert.IsTrue(result);
		}
	
		[TestMethod]
		public async Task ExecuteNonQueryAsync_WithNoDb()
		{
			var fakeConnectionString =
				"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
			var result = await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql
			},new FakeIWebLogger()).ExecuteNonQueryAsync("");
			Assert.IsNull(result);
		}
	
		[TestMethod]
		public async Task IsUtf8_Null()
		{
			var result = await new MySqlDatabaseFixes(null,new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			},new FakeIWebLogger()).IsUtf8();
			Assert.IsNull(result);
		}
	
		[TestMethod]
		public async Task IsUtf8_NoDatabase()
		{
			var fakeConnectionString =
				"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
			var result = await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql
			},new FakeIWebLogger()).IsUtf8();
			Assert.IsFalse(result);
		}
	
		[TestMethod]
		public async Task OpenConnection_Null()
		{
			MySqlConnection? connection = null;
			await new MySqlDatabaseFixes(null!,new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			},new FakeIWebLogger()).OpenConnection();
			Assert.IsNull(connection);
		}
	
		[TestMethod]
		public async Task OpenConnection_MySqlConnector_MySqlException()
		{
			var fakeConnectionString =
				"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
			var logger = new FakeIWebLogger();
			await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql
			},logger).OpenConnection();
			
			Assert.IsTrue(logger.TrackedExceptions.Exists(
				x=>x.Item2?.Contains("MySqlException") == true));
		}
	
		[TestMethod]
		public async Task FixAutoIncrement_NotUsed()
		{
			var result = await new MySqlDatabaseFixes(null,new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			},new FakeIWebLogger()).FixAutoIncrement("test");
			Assert.IsNull(result);
		}
	
		[TestMethod]
		public async Task FixAutoIncrement_NotUsed1()
		{
			var result = await new MySqlDatabaseFixes(new MySqlConnection(),new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			},new FakeIWebLogger()).FixAutoIncrement("test");
			Assert.IsNull(result);
		}
	
		[TestMethod]
		public async Task FixAutoIncrement_NoDatabase()
		{
			var fakeConnectionString =
				"Persist Security Info=False;Username=user;Password=pass;database=test1;server=localhost;Port=8125;Connect Timeout=1";
			var result = await new MySqlDatabaseFixes(new MySqlConnection(fakeConnectionString),new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql
			},new FakeIWebLogger()).FixAutoIncrement("test");
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public async Task FixAutoIncrement_ConnectionNullIgnore()
		{
			var result = await new MySqlDatabaseFixes(null,new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql
			},new FakeIWebLogger()).FixAutoIncrement("test");
			Assert.IsNull(result);
		}
	
		[TestMethod]
		public async Task CheckAutoIncrementExist_DueMissingDatabase()
		{
			var appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql,
				DatabaseConnection = "Server=localhost;port=4785;database=TEST;uid=starsky;pwd=TEST;maximumpoolsize=30;"
			};
			var mysqlConnection = new MySqlConnection(appSettings.DatabaseConnection);
			var result = await new MySqlDatabaseFixes(mysqlConnection,appSettings,new FakeIWebLogger()).CheckAutoIncrementExist("test");
			Assert.IsFalse(result);
		}
	
		[TestMethod]
		public async Task CheckAutoIncrementExist_Null()
		{
			var appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql,
			};
			var result = await new MySqlDatabaseFixes(null,appSettings,new FakeIWebLogger()).CheckAutoIncrementExist("test");
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
			var result = await new MySqlDatabaseFixes(mysqlConnection,appSettings,new FakeIWebLogger()).AlterTableAutoIncrement("test");
			Assert.IsNull(result);
		}
	
		[TestMethod]
		public async Task AlterTable_Null()
		{
			var appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.Mysql,
			};
			var result = await new MySqlDatabaseFixes(null,appSettings,new FakeIWebLogger()).AlterTableAutoIncrement("test");
			Assert.IsNull(result);
		}
	}

}
