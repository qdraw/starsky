using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.settings.Services;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.settings.Services;

[TestClass]
public class UpdateAppSettingsByPathTests
{
	[TestMethod]
	public async Task UpdateAppSettingsAsync_ValidInput_Success()
	{
		var before = Environment.GetEnvironmentVariable("app__storageFolder");
		Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);

		// Arrange
		var testFolderPath = Path.DirectorySeparatorChar + "test-update-appSettings-by-path" +
		                     Path.DirectorySeparatorChar;

		var storage = new FakeIStorage(["/", testFolderPath]);
		var selectorStorage = new FakeSelectorStorage(storage);
		var diskWatcher = new FakeDiskWatcher();
		var updateAppSettingsByPath =
			new UpdateAppSettingsByPath(new AppSettings(), selectorStorage, diskWatcher);
		var appSettingTransferObject = new AppSettingsTransferObject
		{
			StorageFolder = testFolderPath, Verbose = true
		};

		// Act
		var result =
			await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);

		Environment.SetEnvironmentVariable("app__storageFolder", before);

		// Assert
		Assert.AreEqual(200, result.StatusCode);
		Assert.AreEqual("Updated", result.Message);
	}

	[TestMethod]
	public async Task UpdateAppSettingsAsync_ValidInput_Success_CompareJson()
	{
		// Arrange

		var before = Environment.GetEnvironmentVariable("app__storageFolder");
		Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);

		var testFolderPath = Path.DirectorySeparatorChar + "test-update-appSettings-by-path" +
		                     Path.DirectorySeparatorChar;

		var storage = new FakeIStorage(["/", testFolderPath]);
		var selectorStorage = new FakeSelectorStorage(storage);
		var appSettings = new AppSettings();
		var diskWatcher = new FakeDiskWatcher();
		var updateAppSettingsByPath =
			new UpdateAppSettingsByPath(appSettings, selectorStorage, diskWatcher);
		var appSettingTransferObject = new AppSettingsTransferObject
		{
			StorageFolder = testFolderPath, Verbose = true
		};

		// Act
		await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);

		var result =
			( await StreamToStringHelper.StreamToStringAsync(
				storage.ReadStream(appSettings.AppSettingsPath)) ).Replace("\r\n", "\n");

		Environment.SetEnvironmentVariable("app__storageFolder", before);


		var storageFolderJson = JsonSerializer.Serialize(testFolderPath,
			DefaultJsonSerializer.NoNamingPolicyBoolAsString);


		// Assert
		var expectedResult =
			"{\n  \"app\": {\n    \"Verbose\": \"true\",\n    \"StorageFolder\": " + // rm quotes
			storageFolderJson + ",\n";

		Assert.Contains(expectedResult, result);
	}

	[TestMethod]
	public async Task UpdateAppSettingsAsync_InvalidStorageFolder_Returns404()
	{
		var before = Environment.GetEnvironmentVariable("app__storageFolder");
		Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);

		// Arrange
		var selectorStorage = new FakeSelectorStorage();
		var diskWatcher = new FakeDiskWatcher();
		var updateAppSettingsByPath =
			new UpdateAppSettingsByPath(new AppSettings(), selectorStorage, diskWatcher);
		var appSettingTransferObject = new AppSettingsTransferObject
		{
			StorageFolder = "NonExistentFolder"
		};

		// Act
		var result =
			await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);

		Environment.SetEnvironmentVariable("app__storageFolder", before);


		// Assert
		Assert.AreEqual(404, result.StatusCode);
		Assert.AreEqual("Location of StorageFolder on disk not found", result.Message);
	}

	[TestMethod]
	public async Task UpdateAppSettingsAsync_InvalidStorageFolder_Returns403()
	{
		var before = Environment.GetEnvironmentVariable("app__storageFolder");
		Environment.SetEnvironmentVariable("app__storageFolder", "test-update-appSettings-by-path");

		// Arrange
		var selectorStorage =
			new FakeSelectorStorage(new FakeIStorage(["/"]));
		var diskWatcher = new FakeDiskWatcher();
		var updateAppSettingsByPath =
			new UpdateAppSettingsByPath(new AppSettings(), selectorStorage, diskWatcher);
		var appSettingTransferObject = new AppSettingsTransferObject { StorageFolder = "/" };

		// Act
		var result =
			await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);

		// Set back to what it was before
		Environment.SetEnvironmentVariable("app__storageFolder", before);

		// Assert
		Assert.AreEqual(403, result.StatusCode);
		Assert.AreEqual("There is an Environment variable set so you can't update it here",
			result.Message);
	}


	[TestMethod]
	public async Task UpdateAppSettingsAsync_ValidInput_TwoTimes_Success()
	{
		var before = Environment.GetEnvironmentVariable("app__storageFolder");
		Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);

		// Arrange
		var testFolderPath = Path.DirectorySeparatorChar + "test-update-appSettings-by-path" +
		                     Path.DirectorySeparatorChar;

		var storage = new FakeIStorage(["/", testFolderPath]);
		var appSettings = new AppSettings();
		var selectorStorage = new FakeSelectorStorage(storage);
		var diskWatcher = new FakeDiskWatcher();
		var updateAppSettingsByPath =
			new UpdateAppSettingsByPath(appSettings, selectorStorage, diskWatcher);
		var appSettingTransferObject1 = new AppSettingsTransferObject { Verbose = true };

		// Act
		var re1 =
			await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject1);
		Assert.IsFalse(re1.IsError);

		var fileResultString1 =
			await StreamToStringHelper.StreamToStringAsync(
				storage.ReadStream(appSettings.AppSettingsPath));
		var fileResult1 = JsonSerializer.Deserialize<AppContainerAppSettings>(fileResultString1,
			DefaultJsonSerializer.NoNamingPolicyBoolAsString);

		Assert.IsNotNull(fileResult1);
		Assert.IsTrue(fileResult1.App.Verbose);

		var appSettingTransferObject2 = new AppSettingsTransferObject
		{
			StorageFolder = testFolderPath
		};

		await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject2);

		var fileResultString2 =
			await StreamToStringHelper.StreamToStringAsync(
				storage.ReadStream(appSettings.AppSettingsPath));
		var fileResult2 = JsonSerializer.Deserialize<AppContainerAppSettings>(fileResultString2,
			DefaultJsonSerializer.NoNamingPolicyBoolAsString);

		Assert.IsNotNull(fileResult2);

		// Set back to what it was before
		Environment.SetEnvironmentVariable("app__storageFolder", before);

		Assert.AreEqual(testFolderPath, fileResult2.App.StorageFolder);
		Assert.IsTrue(fileResult2.App.Verbose);
	}

	[TestMethod]
	public async Task UpdateAppSettingsAsync_ValidInput_Success_Desktop()
	{
		var storage = new FakeIStorage();
		var selectorStorage = new FakeSelectorStorage(storage);
		var diskWatcher = new FakeDiskWatcher();
		var updateAppSettingsByPath =
			new UpdateAppSettingsByPath(new AppSettings(), selectorStorage, diskWatcher);
		var appSettingTransferObject = new AppSettingsTransferObject
		{
			DesktopCollectionsOpen = CollectionsOpenType.RawJpegMode.Raw,
			DefaultDesktopEditor =
			[
				new AppSettingsDefaultEditorApplication
				{
					ApplicationPath = "/test",
					ImageFormats =
						[ExtensionRolesHelper.ImageFormat.jpg]
				}
			]
		};

		// Act
		var result =
			await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);


		// Assert
		Assert.AreEqual(200, result.StatusCode);
		Assert.AreEqual("Updated", result.Message);

		var fileResultString2 =
			await StreamToStringHelper.StreamToStringAsync(
				storage.ReadStream(new AppSettings().AppSettingsPath));
		var fileResult2 = JsonSerializer.Deserialize<AppContainerAppSettings>(fileResultString2,
			DefaultJsonSerializer.NoNamingPolicyBoolAsString);

		Assert.IsNotNull(fileResult2);
		Assert.AreEqual(CollectionsOpenType.RawJpegMode.Raw,
			fileResult2.App.DesktopCollectionsOpen);
		Assert.AreEqual("/test", fileResult2.App.DefaultDesktopEditor[0].ApplicationPath);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg,
			fileResult2.App.DefaultDesktopEditor[0].ImageFormats[0]);
	}

	[TestMethod]
	public async Task UpdateAppSettingsAsync_StorageFolderMappings_SavedToFile()
	{
		// Arrange
		var storage = new FakeIStorage();
		var selectorStorage = new FakeSelectorStorage(storage);
		var appSettings = new AppSettings();
		var diskWatcher = new FakeDiskWatcher();
		var updateAppSettingsByPath =
			new UpdateAppSettingsByPath(appSettings, selectorStorage, diskWatcher);
		var appSettingTransferObject = new AppSettingsTransferObject
		{
			StorageFolderMappings = new Dictionary<string, string>
			{
				{ "/2024", "/data/archive/2024" }
			}
		};

		// Act
		var result =
			await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);

		// Assert
		Assert.AreEqual(200, result.StatusCode);
		Assert.AreEqual("Updated", result.Message);
		Assert.AreEqual("/data/archive/2024", appSettings.StorageFolderMappings["/2024"]);

		var fileResultString =
			await StreamToStringHelper.StreamToStringAsync(
				storage.ReadStream(appSettings.AppSettingsPath));
		var fileResult = JsonSerializer.Deserialize<AppContainerAppSettings>(fileResultString,
			DefaultJsonSerializer.NoNamingPolicyBoolAsString);

		Assert.IsNotNull(fileResult);
		Assert.HasCount(1, fileResult.App.StorageFolderMappings);
		Assert.AreEqual("/data/archive/2024", fileResult.App.StorageFolderMappings["/2024"]);
	}

	[TestMethod]
	public async Task UpdateAppSettingsAsync_StorageFolderMappings_NotifiesDiskWatcher()
	{
		// Arrange
		var storage = new FakeIStorage();
		var selectorStorage = new FakeSelectorStorage(storage);
		var appSettings = new AppSettings();
		var diskWatcher = new FakeDiskWatcher();
		var updateAppSettingsByPath =
			new UpdateAppSettingsByPath(appSettings, selectorStorage, diskWatcher);
		var appSettingTransferObject = new AppSettingsTransferObject
		{
			StorageFolderMappings = new Dictionary<string, string>
			{
				{ "/2024", "/data/archive/2024" }
			}
		};

		// Act
		await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);

		// Assert - DiskWatcher should have been notified of the new path
		Assert.Contains("/data/archive/2024", diskWatcher.AddedItems);
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Linux | OperatingSystems.OSX)]
	public async Task UpdateAppSettingsAsync_RestrictedPath_Returns403__UnixOnly()
	{
		// Arrange
		var storage = new FakeIStorage();
		var appSettings = new AppSettings();
		var updateAppSettingsByPath =
			new UpdateAppSettingsByPath(appSettings, new FakeSelectorStorage(storage),
				new FakeDiskWatcher());

		var appSettingTransferObject = new AppSettingsTransferObject
		{
			StorageFolderMappings = new Dictionary<string, string>
			{
				{ "/secrets", "/etc/passwd" }
			}
		};

		// Act
		var result =
			await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);

		// Assert
		Assert.AreEqual(403, result.StatusCode);
		Assert.IsFalse(appSettings.StorageFolderMappings.ContainsKey("/secrets"));
	}

	[TestMethod]
	[OSCondition(OperatingSystems.Linux | OperatingSystems.OSX)]
	public async Task UpdateAppSettingsAsync_RestrictedRootPath_Returns403__UnixOnly()
	{
		var storage = new FakeIStorage();
		var appSettings = new AppSettings();
		var updateAppSettingsByPath =
			new UpdateAppSettingsByPath(appSettings, new FakeSelectorStorage(storage),
				new FakeDiskWatcher());

		var appSettingTransferObject = new AppSettingsTransferObject
		{
			StorageFolderMappings = new Dictionary<string, string> { { "/sysroot", "/etc" } }
		};

		var result =
			await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);

		Assert.AreEqual(403, result.StatusCode);
	}
}
