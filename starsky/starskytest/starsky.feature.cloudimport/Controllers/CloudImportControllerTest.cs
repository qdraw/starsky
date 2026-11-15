using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.cloudimport;
using starsky.feature.cloudimport.Controllers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
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
		Assert.AreEqual(15d, firstType.GetProperty("syncFrequencyMinutes")?.GetValue(first));
		Assert.AreEqual(0, firstType.GetProperty("syncFrequencyHours")?.GetValue(first));
		Assert.IsFalse(( bool? ) firstType.GetProperty("deleteAfterImport")?.GetValue(first));
		var isSyncInProgress = value.GetType().GetProperty("isSyncInProgress")?.GetValue(value);
		Assert.IsFalse(( bool? ) isSyncInProgress);
	}

	[TestMethod]
	public void GetProviderStatus_ReturnsProviderStatus()
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

		var result = controller.GetProviderStatus("dropbox-1") as OkObjectResult;
		Assert.IsNotNull(result);
		var value = result.Value;
		Assert.IsNotNull(value);
		var type = value.GetType();
		Assert.AreEqual("dropbox-1", type.GetProperty("id")?.GetValue(value));
		Assert.IsTrue(( bool? ) type.GetProperty("enabled")?.GetValue(value));
		Assert.AreEqual("Dropbox", type.GetProperty("provider")?.GetValue(value));
		Assert.AreEqual("/Camera Uploads", type.GetProperty("remoteFolder")?.GetValue(value));
		Assert.AreEqual(15d, type.GetProperty("syncFrequencyMinutes")?.GetValue(value));
		Assert.AreEqual(0, type.GetProperty("syncFrequencyHours")?.GetValue(value));
		Assert.IsFalse(( bool? ) type.GetProperty("deleteAfterImport")?.GetValue(value));
		var lastSyncResult = type.GetProperty("lastSyncResult")?.GetValue(value);
		Assert.IsNotNull(lastSyncResult);
	}

	[TestMethod]
	public void GetProviderStatus_ReturnsProviderStatus_NotFound()
	{
		
		var fakeService = new FakeCloudImportService
		{
			IsSyncInProgress = false,
			LastSyncResults = new Dictionary<string, CloudImportResult>
			{
				{ "dropbox-1", new CloudImportResult() }
			}
		};
		var controller = new CloudImportController(fakeService, new AppSettings());
		
		var result = controller.GetProviderStatus("not-found") as NotFoundObjectResult;
		Assert.IsNotNull(result);
		Assert.AreEqual(404, result.StatusCode);
	}

	[TestMethod]
	public async Task TriggerSyncAll_ReturnsOkWithResults_WhenProvidersEnabledAndNoSyncInProgress()
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
		var fakeService = new FakeCloudImportService { IsSyncInProgress = false };
		var controller = new CloudImportController(fakeService, appSettings);

		var result = await controller.TriggerSyncAll() as OkObjectResult;
		Assert.IsNotNull(result);
		var value = result.Value;
		Assert.IsNotNull(value);
		var resultsProp = value.GetType().GetProperty("results");
		Assert.IsNotNull(resultsProp);
		var results = resultsProp.GetValue(value) as List<CloudImportResult>;
		Assert.IsNotNull(results);
		Assert.IsTrue(results[0].Success);
	}

	[TestMethod]
	public async Task TriggerSyncAll_ReturnsBadRequest_WhenNoProvidersEnabled()
	{
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = []
			}
		};
		var fakeService = new FakeCloudImportService { IsSyncInProgress = false };
		var controller = new CloudImportController(fakeService, appSettings);

		var result = await controller.TriggerSyncAll() as BadRequestObjectResult;
		Assert.IsNotNull(result);
		var value = result.Value;
		Assert.IsNotNull(value);
		var messageProp = value.GetType().GetProperty("message");
		Assert.IsNotNull(messageProp);
		Assert.AreEqual("No Cloud Import providers are enabled", messageProp.GetValue(value));
	}

	[TestMethod]
	public async Task TriggerSyncAll_ReturnsConflict_WhenSyncInProgress()
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
		var fakeService = new FakeCloudImportService { IsSyncInProgress = true };
		var controller = new CloudImportController(fakeService, appSettings);

		var result = await controller.TriggerSyncAll() as ConflictObjectResult;
		Assert.IsNotNull(result);
		var value = result.Value;
		Assert.IsNotNull(value);
		var messageProp = value.GetType().GetProperty("message");
		Assert.IsNotNull(messageProp);
		Assert.AreEqual("A sync operation is already in progress", messageProp.GetValue(value));
	}

	[TestMethod]
	public async Task TriggerSync_ReturnsOk_WhenProviderExistsAndEnabled()
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
			CloudImport = new CloudImportSettings
			{
				Providers = [provider]
			}
		};
		var fakeService = new FakeCloudImportService();
		var expectedResult = new CloudImportResult { ProviderId = "dropbox-1" };
		fakeService.SyncAsyncFunc = _ => Task.FromResult(expectedResult);
		var controller = new CloudImportController(fakeService, appSettings);

		var result = await controller.TriggerSync("dropbox-1") as OkObjectResult;
		Assert.IsNotNull(result);
		var value = result.Value as CloudImportResult;
		Assert.IsNotNull(value);
		Assert.AreEqual("dropbox-1", value.ProviderId);
		Assert.IsTrue(value.Success);
	}

	[TestMethod]
	public async Task TriggerSync_ReturnsNotFound_WhenProviderDoesNotExist()
	{
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = []
			}
		};
		var fakeService = new FakeCloudImportService();
		var controller = new CloudImportController(fakeService, appSettings);

		var result = await controller.TriggerSync("not-exist") as NotFoundObjectResult;
		Assert.IsNotNull(result);
		var value = result.Value;
		Assert.IsNotNull(value);
		var messageProp = value.GetType().GetProperty("message");
		Assert.IsNotNull(messageProp);
		Assert.AreEqual("Provider 'not-exist' not found", messageProp.GetValue(value));
	}

	[TestMethod]
	public async Task TriggerSync_ReturnsBadRequest_WhenProviderDisabled()
	{
		var provider = new CloudImportProviderSettings
		{
			Id = "dropbox-1",
			Enabled = false,
			Provider = "Dropbox",
			RemoteFolder = "/Camera Uploads",
			SyncFrequencyMinutes = 15,
			SyncFrequencyHours = 0,
			DeleteAfterImport = false
		};
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = [provider]
			}
		};
		var fakeService = new FakeCloudImportService();
		var controller = new CloudImportController(fakeService, appSettings);

		var result = await controller.TriggerSync("dropbox-1") as BadRequestObjectResult;
		Assert.IsNotNull(result);
		var value = result.Value;
		Assert.IsNotNull(value);
		var messageProp = value.GetType().GetProperty("message");
		Assert.IsNotNull(messageProp);
		Assert.AreEqual("Provider 'dropbox-1' is disabled", messageProp.GetValue(value));
	}

	[TestMethod]
	public void GetLastResults_ReturnsOk_WhenResultsExist()
	{
		var fakeService = new FakeCloudImportService
		{
			LastSyncResults = new Dictionary<string, CloudImportResult>
			{
				{ "dropbox-1", new CloudImportResult { ProviderId = "dropbox-1" } }
			}
		};
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = [new CloudImportProviderSettings { Id = "dropbox-1" }]
			}
		};
		var controller = new CloudImportController(fakeService, appSettings);

		var result = controller.GetLastResults() as OkObjectResult;
		Assert.IsNotNull(result);
		var value = result.Value as IDictionary<string, CloudImportResult>;
		Assert.IsNotNull(value);
		Assert.IsTrue(value.ContainsKey("dropbox-1"));
		Assert.IsTrue(value["dropbox-1"].Success);
	}

	[TestMethod]
	public void GetLastResults_ReturnsNotFound_WhenNoResults()
	{
		var fakeService = new FakeCloudImportService
		{
			LastSyncResults = new Dictionary<string, CloudImportResult>()
		};
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = []
			}
		};
		var controller = new CloudImportController(fakeService, appSettings);

		var result = controller.GetLastResults() as NotFoundObjectResult;
		Assert.IsNotNull(result);
		var value = result.Value;
		Assert.IsNotNull(value);
		var messageProp = value.GetType().GetProperty("message");
		Assert.IsNotNull(messageProp);
		Assert.AreEqual("No sync has been performed yet", messageProp.GetValue(value));
	}

	[TestMethod]
	public void GetLastResult_ReturnsOk_WhenResultExists()
	{
		var fakeService = new FakeCloudImportService
		{
			LastSyncResults = new Dictionary<string, CloudImportResult>
			{
				{ "dropbox-1", new CloudImportResult { ProviderId = "dropbox-1" } }
			}
		};
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = [new CloudImportProviderSettings { Id = "dropbox-1" }]
			}
		};
		var controller = new CloudImportController(fakeService, appSettings);

		var result = controller.GetLastResult("dropbox-1") as OkObjectResult;
		Assert.IsNotNull(result);
		var value = result.Value as CloudImportResult;
		Assert.IsNotNull(value);
		Assert.AreEqual("dropbox-1", value.ProviderId);
		Assert.IsTrue(value.Success);
	}

	[TestMethod]
	public void GetLastResult_ReturnsNotFound_WhenResultDoesNotExist()
	{
		var fakeService = new FakeCloudImportService
		{
			LastSyncResults = new Dictionary<string, CloudImportResult>()
		};
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers = []
			}
		};
		var controller = new CloudImportController(fakeService, appSettings);

		var result = controller.GetLastResult("not-exist") as NotFoundObjectResult;
		Assert.IsNotNull(result);
		var value = result.Value;
		Assert.IsNotNull(value);
		var messageProp = value.GetType().GetProperty("message");
		Assert.IsNotNull(messageProp);
		Assert.AreEqual("No sync result found for provider 'not-exist'",
			messageProp.GetValue(value));
	}

	private static (CloudImportController Controller, FakeCloudImportService Service)
		TenantController(string requestTenant)
	{
		var appSettings = new AppSettings
		{
			CloudImport = new CloudImportSettings
			{
				Providers =
				[
					new CloudImportProviderSettings { Id = "mine", Enabled = true, TenantId = "acme" },
					new CloudImportProviderSettings
					{
						Id = "default-one", Enabled = true
					}
				]
			}
		};
		var service = new FakeCloudImportService
		{
			LastSyncResults = new Dictionary<string, CloudImportResult>
			{
				{ "mine", new CloudImportResult { ProviderId = "mine" } },
				{ "default-one", new CloudImportResult { ProviderId = "default-one" } }
			}
		};
		var context = new TenantContext();
		context.SetTenant(requestTenant, false);
		return (new CloudImportController(service, appSettings, context), service);
	}

	[TestMethod]
	public void GetStatus_OnlyShowsTheProvidersOfTheTenant()
	{
		var (controller, _) = TenantController("acme");

		var result = controller.GetStatus() as OkObjectResult;

		var json = System.Text.Json.JsonSerializer.Serialize(result?.Value);
		Assert.Contains("\"id\":\"mine\"", json);
		Assert.DoesNotContain("default-one", json);
	}

	[TestMethod]
	public void GetProviderStatus_ProviderOfAnotherTenant_NotFound()
	{
		var (controller, _) = TenantController("acme");

		Assert.IsInstanceOfType<NotFoundObjectResult>(controller.GetProviderStatus("default-one"));
	}

	[TestMethod]
	public async Task TriggerSync_ProviderOfAnotherTenant_NotFound()
	{
		var (controller, service) = TenantController("acme");

		var result = await controller.TriggerSync("default-one");

		Assert.IsInstanceOfType<NotFoundObjectResult>(result);
		Assert.IsEmpty(service.SyncCalls);
	}

	[TestMethod]
	public async Task TriggerSyncAll_OnlyForTheTenantOfTheRequest()
	{
		var (controller, service) = TenantController("acme");

		await controller.TriggerSyncAll();

		Assert.AreEqual("acme", service.LastSyncAllTenantId);
	}

	[TestMethod]
	public void GetLastResults_OnlyOfTheTenant()
	{
		var (controller, _) = TenantController("acme");

		var result = controller.GetLastResults() as OkObjectResult;

		var value = result?.Value as IDictionary<string, CloudImportResult>;
		Assert.IsNotNull(value);
		Assert.HasCount(1, value);
		Assert.IsTrue(value.ContainsKey("mine"));
	}

	[TestMethod]
	public void GetLastResult_OfAnotherTenant_NotFound()
	{
		var (controller, _) = TenantController("acme");

		Assert.IsInstanceOfType<NotFoundObjectResult>(controller.GetLastResult("default-one"));
	}

	[TestMethod]
	public async Task TriggerSyncAll_TenantWithoutProviders_BadRequest()
	{
		var (controller, _) = TenantController("beta");

		Assert.IsInstanceOfType<BadRequestObjectResult>(await controller.TriggerSyncAll());
	}
}
