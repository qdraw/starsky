using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.cloudsync;
using starsky.foundation.cloudsync.Controllers;
using starsky.foundation.platform.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.cloudsync.Controllers;

[TestClass]
public class CloudSyncControllerTest
{
	[TestMethod]
	public void GetStatus_ShouldReturnCurrentStatus()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudSync = new CloudSyncSettings
			{
				Providers = new List<CloudSyncProviderSettings>
				{
					new CloudSyncProviderSettings
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
		var service = new FakeCloudSyncService();
		var controller = new CloudSyncController(service, appSettings);

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
			CloudSync = new CloudSyncSettings
			{
				Providers = new List<CloudSyncProviderSettings>
				{
					new CloudSyncProviderSettings { Id = "test", Enabled = false }
				}
			}
		};
		var service = new FakeCloudSyncService();
		var controller = new CloudSyncController(service, appSettings);

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
			CloudSync = new CloudSyncSettings
			{
				Providers =
				[
					new CloudSyncProviderSettings
					{
						Id = "test", Enabled = true
					}
				]
			}
		};
		var service = new FakeCloudSyncService { IsSyncInProgress = true };
		var controller = new CloudSyncController(service, appSettings);

		// Act
		var result = await controller.TriggerSync("test") as ConflictObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(409, result.StatusCode);
	}

	[TestMethod]
	public async Task TriggerSync_WhenEnabled_ShouldStartSyncAndReturnResult()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudSync = new CloudSyncSettings
			{
				Providers = new List<CloudSyncProviderSettings>
				{
					new CloudSyncProviderSettings { Id = "test", Enabled = true }
				}
			}
		};
		var service = new FakeCloudSyncService();
		var controller = new CloudSyncController(service, appSettings);

		// Act
		var result = await controller.TriggerSync("test") as OkObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(200, result.StatusCode);
		Assert.IsInstanceOfType(result.Value, typeof(CloudSyncResult));
		var syncResult = result.Value as CloudSyncResult;
		Assert.AreEqual(CloudSyncTriggerType.Manual, syncResult!.TriggerType);
	}

	[TestMethod]
	public void GetLastResult_WhenNoSyncPerformed_ShouldReturnNotFound()
	{
		// Arrange
		var appSettings = new AppSettings
		{
			CloudSync = new CloudSyncSettings
			{
				Providers = new List<CloudSyncProviderSettings>
				{
					new CloudSyncProviderSettings { Id = "test", Enabled = true }
				}
			}
		};
		var service = new FakeCloudSyncService { LastSyncResult = null };
		var controller = new CloudSyncController(service, appSettings);

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
			CloudSync = new CloudSyncSettings
			{
				Providers = new List<CloudSyncProviderSettings>
				{
					new CloudSyncProviderSettings { Id = "test", Enabled = true }
				}
			}
		};
		var lastResult = new CloudSyncResult
		{
			StartTime = DateTime.UtcNow.AddMinutes(-5),
			EndTime = DateTime.UtcNow,
			TriggerType = CloudSyncTriggerType.Scheduled,
			FilesFound = 10,
			FilesImportedSuccessfully = 8,
			FilesFailed = 2
		};
		var service = new FakeCloudSyncService { LastSyncResult = lastResult };
		service.LastSyncResults["test"] = lastResult;
		var controller = new CloudSyncController(service, appSettings);

		// Act
		var result = controller.GetLastResult("test") as OkObjectResult;

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(200, result.StatusCode);
		Assert.IsInstanceOfType(result.Value, typeof(CloudSyncResult));
		var syncResult = result.Value as CloudSyncResult;
		Assert.AreEqual(10, syncResult!.FilesFound);
		Assert.AreEqual(8, syncResult.FilesImportedSuccessfully);
		Assert.AreEqual(2, syncResult.FilesFailed);
	}
}
