using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.MethodLevel)]

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public sealed class SetupAppSettingsTest
{
	private readonly StorageHostFullPathFilesystem _hostStorage;

	public SetupAppSettingsTest()
	{
		_hostStorage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
	}


	[TestMethod]
	[DataRow("app__appsettingspath")]
	[DataRow("app__appsettingslocalpath")]
	public async Task SetLocalAppData_ShouldRead(string envName)
	{
		var appDataFolderFullPath = Path.Combine(AppContext.BaseDirectory,
			$"setup_app_settings_test_{envName}");

		_hostStorage.CreateDirectory(appDataFolderFullPath);
		var path = Path.Combine(appDataFolderFullPath, "appsettings.json");

		var example =
			StringToStreamHelper.StringToStream(
				"{\n \"app\" :{\n   \"isAccountRegisterOpen\": \"true\"\n }\n}\n");

		await _hostStorage.WriteStreamAsync(example, path);
		Environment.SetEnvironmentVariable("app__appsettingspath", path);
		var builder = await SetupAppSettings.AppSettingsToBuilder();
		var services = new ServiceCollection();
		var appSettings = SetupAppSettings.ConfigurePoCoAppSettings(services, builder);

		Assert.IsFalse(string.IsNullOrEmpty(appSettings.AppSettingsPath));
		Assert.IsTrue(appSettings.IsAccountRegisterOpen);
		Assert.AreEqual(path, appSettings.AppSettingsPath);

		_hostStorage.FolderDelete(appDataFolderFullPath);
		Environment.SetEnvironmentVariable(envName, null);
	}

	[TestMethod]
	public async Task SetLocalAppData_ShouldTakeDefault()
	{
		Environment.SetEnvironmentVariable("app__appsettingspath", null);

		var builder = await SetupAppSettings.AppSettingsToBuilder();
		var services = new ServiceCollection();
		var appSettings = SetupAppSettings.ConfigurePoCoAppSettings(services, builder);

		var expectedPath =
			Path.Combine(appSettings.BaseDirectoryProject, "appsettings.patch.json");
		Assert.AreEqual(expectedPath, appSettings.AppSettingsPath);
		Assert.IsFalse(appSettings.IsAccountRegisterOpen);
	}

	[TestMethod]
	public async Task MergeJsonFiles_DefaultFile()
	{
		var testDir = Path.Combine(new AppSettings().BaseDirectoryProject, "_test");
		if ( _hostStorage.ExistFolder(testDir) )
		{
			_hostStorage.FolderDelete(testDir);
		}

		_hostStorage.CreateDirectory(testDir);

		await _hostStorage.WriteStreamAsync(StringToStreamHelper.StringToStream(
			"{\n  \"app\": {\n   " +
			" \"StorageFolder\": \"/data/test\"\n " +
			" }\n}\n"), Path.Combine(testDir, "appsettings.json"));

		var result = await SetupAppSettings.MergeJsonFiles(testDir);
		Assert.AreEqual(PathHelper.AddBackslash("/data/test"), result.StorageFolder);
	}

	[TestMethod]
	public async Task MergeJsonFiles_NoFileFound()
	{
		var testDir = Path.Combine(new AppSettings().BaseDirectoryProject, "_not_found");
		var result = await SetupAppSettings.MergeJsonFiles(testDir);
		Assert.AreEqual(result.Verbose, new AppSettings().Verbose);
	}

	[TestMethod]
	public async Task MergeJsonFiles_StackPatchFile()
	{
		var testDir = Path.Combine(new AppSettings().BaseDirectoryProject, "_test");
		if ( _hostStorage.ExistFolder(testDir) )
		{
			_hostStorage.FolderDelete(testDir);
		}

		_hostStorage.CreateDirectory(testDir);

		await _hostStorage.WriteStreamAsync(StringToStreamHelper.StringToStream(
			"{\n  \"app\": {\n   " +
			" \"StorageFolder\": \"/data/test\",\n \"addSwagger\": \"true\" " +
			" }\n}\n"), Path.Combine(testDir, "appsettings.json"));

		await _hostStorage.WriteStreamAsync(StringToStreamHelper.StringToStream(
			"{\n  \"app\": {\n  \"addSwagger\": \"false\" " +
			" }\n}\n"), Path.Combine(testDir, "appsettings.patch.json"));

		var result = await SetupAppSettings.MergeJsonFiles(testDir);
		Assert.AreEqual(PathHelper.AddBackslash("/data/test"), result.StorageFolder);
		Assert.IsFalse(result.AddSwagger);
	}

	[TestMethod]
	public async Task MergeJsonFiles_StackMachineNamePatchFile()
	{
		var testDir = Path.Combine(new AppSettings().BaseDirectoryProject, "_test");
		if ( _hostStorage.ExistFolder(testDir) )
		{
			_hostStorage.FolderDelete(testDir);
		}

		_hostStorage.CreateDirectory(testDir);

		await _hostStorage.WriteStreamAsync(StringToStreamHelper.StringToStream(
			"{\n  \"app\": {\n   " +
			" \"StorageFolder\": \"/data/test\",\n \"addSwagger\": \"true\" " +
			" }\n}\n"), Path.Combine(testDir, "appsettings.json"));

		await _hostStorage.WriteStreamAsync(StringToStreamHelper.StringToStream(
				"{\n  \"app\": {\n  \"addSwagger\": \"false\" " +
				" }\n}\n"),
			Path.Combine(testDir, $"{SetupAppSettings.AppSettingsMachineNameWithDot()}json"));

		var result = await SetupAppSettings.MergeJsonFiles(testDir);
		Assert.AreEqual(PathHelper.AddBackslash("/data/test"), result.StorageFolder);
		Assert.IsFalse(result.AddSwagger);
	}

	[TestMethod]
	public async Task MergeJsonFiles_StackFromEnv()
	{
		var testDir = Path.Combine(new AppSettings().BaseDirectoryProject, "_test");
		_hostStorage.FolderDelete(testDir);
		_hostStorage.CreateDirectory(testDir);

		await _hostStorage.WriteStreamAsync(StringToStreamHelper.StringToStream(
			"{\n  \"app\": {\n   " +
			" \"StorageFolder\": \"/data/test\",\n \"addSwagger\": \"true\" " +
			" }\n}\n"), Path.Combine(testDir, "appsettings.json"));

		await _hostStorage.WriteStreamAsync(StringToStreamHelper.StringToStream(
			"{\n  \"app\": {\n  \"addSwagger\": \"false\" " +
			" }\n}\n"), Path.Combine(testDir, "appsettings_ref_patch.json"));

		Environment.SetEnvironmentVariable("app__appsettingspath",
			Path.Combine(testDir, "appsettings_ref_patch.json"));

		var result = await SetupAppSettings.MergeJsonFiles(testDir);
		Assert.AreEqual(PathHelper.AddBackslash("/data/test"), result.StorageFolder);
		Assert.IsFalse(result.AddSwagger);

		Environment.SetEnvironmentVariable("app__appsettingspath", null);
	}

	[TestMethod]
	public async Task MergeJsonFiles_StackQueueSettings()
	{
		var testDir = Path.Combine(new AppSettings().BaseDirectoryProject, "_test_queue_merge");
		if ( _hostStorage.ExistFolder(testDir) )
		{
			_hostStorage.FolderDelete(testDir);
		}

		_hostStorage.CreateDirectory(testDir);

		await _hostStorage.WriteStreamAsync(StringToStreamHelper.StringToStream(
			"{\n  \"app\": {\n    \"queue\": {\n      \"default\": \"InMemory\",\n      \"databasePollIntervalInMilliseconds\": 500,\n      \"queues\": {\n        \"Update\": \"InMemory\"\n      }\n    }\n  }\n}\n"), Path.Combine(testDir, "appsettings.json"));

		await _hostStorage.WriteStreamAsync(StringToStreamHelper.StringToStream(
			"{\n  \"app\": {\n    \"queue\": {\n      \"default\": \"RabbitMq\",\n      \"databasePollIntervalInMilliseconds\": 900,\n      \"queues\": {\n        \"ImageClassification\": \"Database\"\n      },\n      \"rabbitMq\": {\n        \"host\": \"mq.internal\",\n        \"port\": 5673,\n        \"username\": \"starsky\",\n        \"password\": \"secret\",\n        \"virtualHost\": \"/starsky\"\n      }\n    }\n  }\n}\n"), Path.Combine(testDir, "appsettings.patch.json"));

		var result = await SetupAppSettings.MergeJsonFiles(testDir);

		Assert.AreEqual(QueueBackendType.RabbitMq, result.Queue.Default);
		Assert.AreEqual(900, result.Queue.DatabasePollIntervalInMilliseconds);
		Assert.AreEqual(QueueBackendType.Database, result.Queue.Queues["ImageClassification"]);
		Assert.AreEqual("mq.internal", result.Queue.RabbitMq.Host);

		_hostStorage.FolderDelete(testDir);
	}

	[TestMethod]
	public async Task MergeJsonFiles_StackImageClassificationSettings()
	{
		var testDir = Path.Combine(new AppSettings().BaseDirectoryProject,
			"_test_image_classification_merge");
		if ( _hostStorage.ExistFolder(testDir) )
		{
			_hostStorage.FolderDelete(testDir);
		}

		_hostStorage.CreateDirectory(testDir);

		await _hostStorage.WriteStreamAsync(StringToStreamHelper.StringToStream(
			"{\n  \"app\": {\n    \"useImageClassificationOnStartup\": false,\n    \"ollamaModel\": \"gemma3:4b\",\n    \"ollamaExecutablePath\": \"\",\n    \"imageClassificationBatchSize\": 25\n  }\n}\n"), Path.Combine(testDir, "appsettings.json"));

		await _hostStorage.WriteStreamAsync(StringToStreamHelper.StringToStream(
			"{\n  \"app\": {\n    \"useImageClassificationOnStartup\": true,\n    \"ollamaModel\": \"llava:13b\",\n    \"ollamaExecutablePath\": \"/opt/ollama/ollama\",\n    \"imageClassificationBatchSize\": 64\n  }\n}\n"), Path.Combine(testDir, "appsettings.patch.json"));

		var result = await SetupAppSettings.MergeJsonFiles(testDir);

		Assert.IsTrue(result.UseImageClassificationOnStartup.GetValueOrDefault());
		Assert.AreEqual("llava:13b", result.OllamaModel);
		Assert.AreEqual("/opt/ollama/ollama", result.OllamaExecutablePath);
		Assert.AreEqual(64, result.ImageClassificationBatchSize);

		_hostStorage.FolderDelete(testDir);
	}
}
