using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
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
			var appDataFolderFullPath =
				Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "temp_settings");
			
			_hostStorage.CreateDirectory(appDataFolderFullPath);
			var path = Path.Combine(appDataFolderFullPath, "appsettings.json");
			
			var example =
				new PlainTextFileHelper().StringToStream(
					"{\n \"app\" :{\n   \"isAccountRegisterOpen\": \"true\"\n }\n}\n");

			_hostStorage.WriteStream(example, path);
			Environment.SetEnvironmentVariable("app__AppSettingsPath", path);
			var builder = SetupAppSettings.AppSettingsToBuilder();
			var services = new ServiceCollection();
			var appSettings = SetupAppSettings.ConfigurePoCoAppSettings(services, builder);
			
			Assert.IsFalse(string.IsNullOrEmpty(appSettings.AppSettingsPath));
			Assert.IsTrue(appSettings.IsAccountRegisterOpen);
			Assert.AreEqual(path,appSettings.AppSettingsPath );
			
			_hostStorage.FolderDelete(appDataFolderFullPath);
			Environment.SetEnvironmentVariable("app__AppSettingsPath", null);
		}
		
		[TestMethod]
		public void SetLocalAppData_ShouldIgnore()
		{
			var builder = SetupAppSettings.AppSettingsToBuilder();
			var services = new ServiceCollection();
			var appSettings = SetupAppSettings.ConfigurePoCoAppSettings(services, builder);
			
			Assert.IsTrue(string.IsNullOrEmpty(appSettings.AppSettingsPath));
			Assert.IsFalse(appSettings.IsAccountRegisterOpen);
		}
		
	}
}
