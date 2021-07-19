using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;

[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.MethodLevel)]
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
		public async Task SetLocalAppData_ShouldRead()
		{
			var appDataFolderFullPath =
				Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "setup_app_settings_test");
			
			_hostStorage.CreateDirectory(appDataFolderFullPath);
			var path = Path.Combine(appDataFolderFullPath, "appsettings.json");
			
			var example =
				new PlainTextFileHelper().StringToStream(
					"{\n \"app\" :{\n   \"isAccountRegisterOpen\": \"true\"\n }\n}\n");

			await _hostStorage.WriteStreamAsync(example, path);
			Environment.SetEnvironmentVariable("app__AppSettingsPath", path);
			var builder = await SetupAppSettings.AppSettingsToBuilder();
			var services = new ServiceCollection();
			var appSettings = SetupAppSettings.ConfigurePoCoAppSettings(services, builder);
			
			Assert.IsFalse(string.IsNullOrEmpty(appSettings.AppSettingsPath));
			Assert.IsTrue(appSettings.IsAccountRegisterOpen);
			Assert.AreEqual(path,appSettings.AppSettingsPath );
			
			_hostStorage.FolderDelete(appDataFolderFullPath);
			Environment.SetEnvironmentVariable("app__AppSettingsPath", null);
		}
		
		[TestMethod]
		public async Task SetLocalAppData_ShouldTakeDefault()
		{
			Environment.SetEnvironmentVariable("app__AppSettingsPath", null);
			
			var builder = await SetupAppSettings.AppSettingsToBuilder();
			var services = new ServiceCollection();
			var appSettings = SetupAppSettings.ConfigurePoCoAppSettings(services, builder);
			
			var expectedPath = Path.Combine(appSettings.BaseDirectoryProject, "appsettings.patch.json");
			Assert.AreEqual(expectedPath, appSettings.AppSettingsPath);
			Assert.IsFalse(appSettings.IsAccountRegisterOpen);
		}

		[TestMethod]
		public async Task MergeJsonFiles_DefaultFile()
		{
			var testDir = Path.Combine(new AppSettings().BaseDirectoryProject, "_test");
			_hostStorage.FolderDelete(testDir);
			_hostStorage.CreateDirectory(testDir);

			await _hostStorage.WriteStreamAsync(new PlainTextFileHelper().StringToStream(
				"{\n  \"app\": {\n   " +
				" \"StorageFolder\": \"/data/test/\"\n " +
				" }\n}\n"), Path.Combine(testDir, "appsettings.json"));

			var result = await SetupAppSettings.MergeJsonFiles(testDir);
			Assert.AreEqual("/data/test/", result.StorageFolder);
		}
		
		[TestMethod]
		public async Task MergeJsonFiles_StackPatchFile()
		{
			var testDir = Path.Combine(new AppSettings().BaseDirectoryProject, "_test");
			_hostStorage.FolderDelete(testDir);
			_hostStorage.CreateDirectory(testDir);

			await _hostStorage.WriteStreamAsync(new PlainTextFileHelper().StringToStream(
				"{\n  \"app\": {\n   " +
				" \"StorageFolder\": \"/data/test/\",\n \"addSwagger\": \"true\" " +
				" }\n}\n"), Path.Combine(testDir, "appsettings.json"));
			
			await _hostStorage.WriteStreamAsync(new PlainTextFileHelper().StringToStream(
				"{\n  \"app\": {\n  \"addSwagger\": \"false\" " +
				" }\n}\n"), Path.Combine(testDir, "appsettings.patch.json"));
			
			var result = await SetupAppSettings.MergeJsonFiles(testDir);
			Assert.AreEqual("/data/test/", result.StorageFolder);
			Assert.AreEqual(false, result.AddSwagger);
		}
				
		[TestMethod]
		public async Task MergeJsonFiles_StackFromEnv()
		{
			var testDir = Path.Combine(new AppSettings().BaseDirectoryProject, "_test");
			_hostStorage.FolderDelete(testDir);
			_hostStorage.CreateDirectory(testDir);

			await _hostStorage.WriteStreamAsync(new PlainTextFileHelper().StringToStream(
				"{\n  \"app\": {\n   " +
				" \"StorageFolder\": \"/data/test/\",\n \"addSwagger\": \"true\" " +
				" }\n}\n"), Path.Combine(testDir, "appsettings.json"));
			
			await _hostStorage.WriteStreamAsync(new PlainTextFileHelper().StringToStream(
				"{\n  \"app\": {\n  \"addSwagger\": \"false\" " +
				" }\n}\n"), Path.Combine(testDir, "appsettings_ref_patch.json"));
			
			Environment.SetEnvironmentVariable("app__AppSettingsPath", Path.Combine(testDir, "appsettings_ref_patch.json"));
			
			var result = await SetupAppSettings.MergeJsonFiles(testDir);
			Assert.AreEqual("/data/test/", result.StorageFolder);
			Assert.AreEqual(false, result.AddSwagger);
			
			Environment.SetEnvironmentVariable("app__AppSettingsPath", null);
		}
	}
}
