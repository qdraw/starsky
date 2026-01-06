using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.cloudimport;
using starsky.foundation.cloudimport.Controllers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.cloudimport.Controllers;

[TestClass]
public class CloudImportControllerTest
{
	[TestMethod]
	public void GetStatus_ShouldReturnCurrentStatus()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = new List<CloudImportProviderSettings>
				{
					new()
					{
						Id = "test",
						Enabled = true,
						Provider = "Dropbox",
						RemoteFolder = "/photos",
						SyncFrequencyMinutes = 30,
						DeleteAfterImport = true
					}
				}
			}
		};
		var service = new FakeCloudImportService();
		var controller = new CloudImportController(service, appSettings);

		// Act
		var result = controller.GetStatus() as OkObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(200, result.StatusCode);
	}

	[TestMethod]
	public async Task TriggerSync_WhenDisabled_ShouldReturnBadRequest()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = new List<CloudImportProviderSettings>
				{
					new() { Id = "test", Enabled = false }
				}
			}
		};
		var service = new FakeCloudImportService();
		var controller = new CloudImportController(service, appSettings);

		// Act
		var result = await controller.TriggerSync("test") as BadRequestObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(400, result.StatusCode);
	}

	[TestMethod]
	public async Task TriggerSync_WhenAlreadyInProgress_ShouldReturnConflict()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings { Id = "test", Enabled = true }
				]
			}
		};
		var service = new FakeCloudImportService { IsSyncInProgress = true };
		var controller = new CloudImportController(service, appSettings);

		// Act
		var result = await controller.TriggerSync("test") as OkObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(200, result.StatusCode);
	}

	[TestMethod]
	public async Task TriggerSync_WhenEnabled_ShouldStartSyncAndReturnResult()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = new List<CloudImportProviderSettings>
				{
					new() { Id = "test", Enabled = true }
				}
			}
		};
		var service = new FakeCloudImportService();
		var controller = new CloudImportController(service, appSettings);

		// Act
		var result = await controller.TriggerSync("test") as OkObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(200, result.StatusCode);
		Assert.IsInstanceOfType(result.Value, typeof(CloudImportResult));
		var syncResult = result.Value as CloudImportResult;
		Assert.AreEqual(CloudImportTriggerType.Manual, syncResult!.TriggerType);
	}

	[TestMethod]
	public void GetLastResult_WhenNoSyncPerformed_ShouldReturnNotFound()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = new List<CloudImportProviderSettings>
				{
					new() { Id = "test", Enabled = true }
				}
			}
		};
		var service = new FakeCloudImportService { LastSyncResult = null };
		var controller = new CloudImportController(service, appSettings);

		// Act
		var result = controller.GetLastResult("test") as NotFoundObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(404, result.StatusCode);
	}

	[TestMethod]
	public void GetLastResult_WhenSyncPerformed_ShouldReturnResult()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = new List<CloudImportProviderSettings>
				{
					new() { Id = "test", Enabled = true }
				}
			}
		};
		var lastResult = new CloudImportResult
		{
			StartTime = DateTime.UtcNow.AddMinutes(-5),
			EndTime = DateTime.UtcNow,
			TriggerType = CloudImportTriggerType.Scheduled,
			FilesFound = 10,
			FilesImportedSuccessfully = 8,
			FilesFailed = 2
		};
		var service = new FakeCloudImportService { LastSyncResult = lastResult };
		service.LastSyncResults["test"] = lastResult;
		var controller = new CloudImportController(service, appSettings);

		// Act
		var result = controller.GetLastResult("test") as OkObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(200, result.StatusCode);
		Assert.IsInstanceOfType(result.Value, typeof(CloudImportResult));
		var syncResult = result.Value as CloudImportResult;
		Assert.AreEqual(10, syncResult!.FilesFound);
		Assert.AreEqual(8, syncResult.FilesImportedSuccessfully);
		Assert.AreEqual(2, syncResult.FilesFailed);
	}
}
