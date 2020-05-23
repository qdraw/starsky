using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Middleware;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;

namespace starskytest.starsky.foundation.platform.Helpers
{
	[TestClass]
	public class SetupAppSettingsTest
	{
		private  readonly StorageHostFullPathFilesystem _hostStorage;

		public SetupAppSettingsTest()
		{
			_hostStorage = new StorageHostFullPathFilesystem();
		}
		
		[TestMethod]
		public void SetLocalAppData_ShouldRead()
		{
			SetupAppSettings.AppDataFolderFullPath =
				Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "temp_settings");
			
			_hostStorage.CreateDirectory(SetupAppSettings.AppDataFolderFullPath);
			var path = Path.Combine(SetupAppSettings.AppDataFolderFullPath, "appsettings.json");
			
			var example =
				new PlainTextFileHelper().StringToStream(
					"{\n \"app\" :{\n   \"isAccountRegisterOpen\": \"true\"\n }\n}\n");

			_hostStorage.WriteStream(example, path);
			Environment.SetEnvironmentVariable("app__isAppDataSettings", "true");
			var builder = SetupAppSettings.AppSettingsToBuilder();
			var services = new ServiceCollection();
			var appSettings = SetupAppSettings.ConfigurePoCoAppSettings(services, builder);
			
			Assert.IsTrue(appSettings.IsAppDataSettings);
			Assert.IsTrue(appSettings.IsAccountRegisterOpen);
			
			_hostStorage.FolderDelete(SetupAppSettings.AppDataFolderFullPath);
			Environment.SetEnvironmentVariable("app__isAppDataSettings", null);
		}
		
		[TestMethod]
		public void SetLocalAppData_ShouldIgnore()
		{
			SetupAppSettings.AppDataFolderFullPath =
				Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "temp_settings");

			var builder = SetupAppSettings.AppSettingsToBuilder();
			var services = new ServiceCollection();
			var appSettings = SetupAppSettings.ConfigurePoCoAppSettings(services, builder);
			
			Assert.IsFalse(appSettings.IsAppDataSettings);
			Assert.IsFalse(appSettings.IsAccountRegisterOpen);
		}
		
	}
}
