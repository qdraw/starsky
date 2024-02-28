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

namespace starskytest.starsky.feature.settings.Services
{
	[TestClass]
	public class UpdateAppSettingsByPathTests
	{
		[TestMethod]
		public async Task UpdateAppSettingsAsync_ValidInput_Success()
		{
			var before = Environment.GetEnvironmentVariable("app__storageFolder");
			Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);

			// Arrange
			var testFolderPath = Path.DirectorySeparatorChar.ToString() + "test" +
			                     Path.DirectorySeparatorChar.ToString();

			var storage = new FakeIStorage(new List<string> { "/", testFolderPath });
			var selectorStorage = new FakeSelectorStorage(storage);
			var updateAppSettingsByPath =
				new UpdateAppSettingsByPath(new AppSettings(), selectorStorage);
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

			var testFolderPath = Path.DirectorySeparatorChar + "test" +
			                     Path.DirectorySeparatorChar;

			var storage = new FakeIStorage(new List<string> { "/", testFolderPath });
			var selectorStorage = new FakeSelectorStorage(storage);
			var appSettings = new AppSettings();
			var updateAppSettingsByPath = new UpdateAppSettingsByPath(appSettings, selectorStorage);
			var appSettingTransferObject = new AppSettingsTransferObject
			{
				StorageFolder = testFolderPath, Verbose = true,
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


			Assert.AreEqual(true, result.Contains(expectedResult));
		}

		[TestMethod]
		public async Task UpdateAppSettingsAsync_InvalidStorageFolder_Returns404()
		{
			var before = Environment.GetEnvironmentVariable("app__storageFolder");
			Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);

			// Arrange
			var selectorStorage = new FakeSelectorStorage();
			var updateAppSettingsByPath =
				new UpdateAppSettingsByPath(new AppSettings(), selectorStorage);
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
			Environment.SetEnvironmentVariable("app__storageFolder", "test");

			// Arrange
			var selectorStorage =
				new FakeSelectorStorage(new FakeIStorage(new List<string> { "/" }));
			var updateAppSettingsByPath =
				new UpdateAppSettingsByPath(new AppSettings(), selectorStorage);
			var appSettingTransferObject = new AppSettingsTransferObject { StorageFolder = "/" };

			// Act
			var result =
				await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);

			// Set back to what is was before
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
			var testFolderPath = Path.DirectorySeparatorChar + "test" + Path.DirectorySeparatorChar;

			var storage = new FakeIStorage(new List<string> { "/", testFolderPath });
			var appSettings = new AppSettings();
			var selectorStorage = new FakeSelectorStorage(storage);
			var updateAppSettingsByPath = new UpdateAppSettingsByPath(appSettings, selectorStorage);
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

			// Set back to what is was before
			Environment.SetEnvironmentVariable("app__storageFolder", before);

			Assert.AreEqual(testFolderPath, fileResult2.App.StorageFolder);
			Assert.IsTrue(fileResult2.App.Verbose);
		}

		[TestMethod]
		public async Task UpdateAppSettingsAsync_ValidInput_Success_Desktop()
		{
			var storage = new FakeIStorage();
			var selectorStorage = new FakeSelectorStorage(storage);
			var updateAppSettingsByPath =
				new UpdateAppSettingsByPath(new AppSettings(), selectorStorage);
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
	}
}
