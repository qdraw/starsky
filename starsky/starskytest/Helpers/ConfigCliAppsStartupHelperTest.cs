using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using starsky.foundation.readmeta.Services;
using starsky.foundation.thumbnailgeneration.Services;
using starskycore.Helpers;
using starskycore.Services;

namespace starskytest.Helpers
{
	[TestClass]
	public class ConfigCliAppsStartupHelperTest
	{
		[TestMethod]
		public void ConfigCliAppsStartupHelperTestImportService()
		{
			var cliHelper = new ConfigCliAppsStartupHelper();
			Environment.SetEnvironmentVariable("app__DatabaseType",null);

			var name = cliHelper.ExifTool().ToString();
			Assert.AreEqual(true,name.Contains(nameof(ExifTool)));
			
			name = cliHelper.SyncService().ToString();
			Assert.AreEqual(true,name.Contains(nameof(SyncService)));			
			
			name = cliHelper.ReadMeta().ToString();
			Assert.AreEqual(true,name.Contains(nameof(ReadMeta)));
			
			name = cliHelper.ImportService().ToString();
			Assert.AreEqual(true,name.Contains(nameof(ImportService)));

			name = cliHelper.UserManager().ToString();
			Assert.AreEqual(true,name.Contains(nameof(UserManager)));		
			
			name = cliHelper.ThumbnailCleaner().ToString();
			Assert.AreEqual(true,name.Contains(nameof(ThumbnailCleaner)));		
		}

		[TestMethod]
		[ExpectedException(typeof(Microsoft.EntityFrameworkCore.Storage.RetryLimitExceededException))]
		public void ConfigCliAppsStartupHelperTestImportService_MysqlCrash()
		{
			Environment.SetEnvironmentVariable("app__DatabaseType","mysql");
			new ConfigCliAppsStartupHelper();
		}
	}
}
