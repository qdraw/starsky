using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.cloudimport;
using starsky.feature.cloudimport.Controllers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.cloudimport.Controllers;

[TestClass]
public class CloudImportControllerTest
{
	[TestMethod]
	public void GetStatus_ReturnsProviderStatus()
	{
		var provider = new CloudImportProviderSettings
		{
			Id = "dropbox-1",
			Enabled = true,
			Provider = "Dropbox",
			RemoteFolder = "/Camera Uploads",
			SyncFrequencyMinutes = 15,
			SyncFrequencyHours = 0,
			DeleteAfterImport = false
		};
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings { Providers = [provider] }
		};
		var fakeService = new FakeCloudImportService
		{
			IsSyncInProgress = false,
			LastSyncResults = new Dictionary<string, CloudImportResult>
			{
				{ "dropbox-1", new CloudImportResult() }
			}
		};
		var controller = new CloudImportController(fakeService, appSettings);

		var result = controller.GetStatus() as OkObjectResult;
		Assert.IsNotNull(result);
		var value = result.Value;
		Assert.IsNotNull(value);
		var providersProp = value.GetType().GetProperty("providers");
		Assert.IsNotNull(providersProp);
		var providers = providersProp.GetValue(value) as IEnumerable<object>;
		Assert.IsNotNull(providers);
		var first = providers.First();
		var firstType = first.GetType();
		Assert.AreEqual("dropbox-1", firstType.GetProperty("id")?.GetValue(first));
		Assert.IsTrue(( bool? ) firstType.GetProperty("enabled")?.GetValue(first));
		Assert.AreEqual("Dropbox", firstType.GetProperty("provider")?.GetValue(first));
		Assert.AreEqual("/Camera Uploads", firstType.GetProperty("remoteFolder")?.GetValue(first));
		Assert.AreEqual(15, firstType.GetProperty("syncFrequencyMinutes")?.GetValue(first));
		Assert.AreEqual(0, firstType.GetProperty("syncFrequencyHours")?.GetValue(first));
		Assert.IsFalse(( bool? ) firstType.GetProperty("deleteAfterImport")?.GetValue(first));
		var isSyncInProgress = value.GetType().GetProperty("isSyncInProgress")?.GetValue(value);
		Assert.IsFalse(( bool? ) isSyncInProgress);
	}
}
