using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.settings.Services;
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
            // Arrange
            var storage = new FakeIStorage(new List<string>{"/"});
            var selectorStorage = new FakeSelectorStorage(storage);
            var updateAppSettingsByPath = new UpdateAppSettingsByPath(new AppSettings(), selectorStorage);
            var appSettingTransferObject = new AppSettingsTransferObject
            {
	            StorageFolder = "/",
	            Verbose = true
            };

            // Act
            var result = await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);

            // Assert
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("Updated", result.Message);
        }
        
        [TestMethod]
        public async Task UpdateAppSettingsAsync_ValidInput_Success_CompareJson()
        {
	        // Arrange
	        var storage = new FakeIStorage(new List<string>{"/"});
	        var selectorStorage = new FakeSelectorStorage(storage);
	        var appSettings = new AppSettings();
	        var updateAppSettingsByPath = new UpdateAppSettingsByPath(appSettings, selectorStorage);
	        var appSettingTransferObject = new AppSettingsTransferObject
	        {
		        StorageFolder = "/",
		        Verbose = true
	        };

	        // Act
	        await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);

	        var result = await StreamToStringHelper.StreamToStringAsync(storage.ReadStream(appSettings.AppSettingsPath)) ;
	        
	        // Assert
	        const string expectedResult = "{\n  \"app\": {\n    \"Verbose\": \"true\",\n    \"StorageFolder\": \"/\"\n  }\n}";

	        Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public async Task UpdateAppSettingsAsync_InvalidStorageFolder_Returns404()
        {
            // Arrange
            var selectorStorage = new FakeSelectorStorage();
            var updateAppSettingsByPath = new UpdateAppSettingsByPath(new AppSettings(), selectorStorage);
            var appSettingTransferObject = new AppSettingsTransferObject
            {
                StorageFolder = "NonExistentFolder"
            };

            // Act
            var result = await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);

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
	        var selectorStorage = new FakeSelectorStorage(new FakeIStorage(new List<string>{"/"}));
	        var updateAppSettingsByPath = new UpdateAppSettingsByPath(new AppSettings(), selectorStorage);
	        var appSettingTransferObject = new AppSettingsTransferObject
	        {
		        StorageFolder = "/"
	        };

	        // Act
	        var result = await updateAppSettingsByPath.UpdateAppSettingsAsync(appSettingTransferObject);

	        // Set back to what is was before
	        Environment.SetEnvironmentVariable("app__storageFolder", before);
	        
	        // Assert
	        Assert.AreEqual(403, result.StatusCode);
	        Assert.AreEqual("There is an Environment variable set so you can't update it here", result.Message);
        }

    }
}
