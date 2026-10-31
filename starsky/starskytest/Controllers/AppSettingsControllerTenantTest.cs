using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.settings.Models;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class AppSettingsControllerTenantTest
{
	private static async Task<IActionResult> Update(string[] activeTenants, bool platformAdmin)
	{
		var services = new ServiceCollection();
		services.AddSingleton<ITenantRegistry>(new FakeTenantRegistry(activeTenants));
		var controller = new AppSettingsController(new AppSettings(),
			new FakeIUpdateAppSettingsByPath(new UpdateAppSettingsStatusModel()));
		var claims = platformAdmin
			? new[] { new Claim(TenantMembershipMiddleware.PlatformAdminClaim, "true") }
			: [];
		controller.ControllerContext.HttpContext = new DefaultHttpContext
		{
			RequestServices = services.BuildServiceProvider(),
			User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
		};
		return await controller.UpdateAppSettings(new AppSettingsTransferObject { Verbose = true });
	}

	[TestMethod]
	public async Task OneTenant_TenantAdministratorMayStillWrite()
	{
		var result = await Update(["default"], false);

		Assert.IsNotInstanceOfType<ObjectResult>(result);
	}

	[TestMethod]
	public async Task SeveralTenants_OnlyPlatformAdministratorMayWriteProcessWideSettings()
	{
		var forbidden = await Update(["default", "acme"], false) as ObjectResult;
		var allowed = await Update(["default", "acme"], true);

		Assert.AreEqual(403, forbidden?.StatusCode);
		Assert.IsNotInstanceOfType<ObjectResult>(allowed);
	}

	private static AppSettings EnvFor(string[] activeTenants, bool platformAdmin)
	{
		var services = new ServiceCollection();
		services.AddSingleton<ITenantRegistry>(new FakeTenantRegistry(activeTenants));
		var controller = new AppSettingsController(
			new AppSettings { StorageFolder = "/data/secret/storage", TenantsRoot = "/data/tenants" },
			new FakeIUpdateAppSettingsByPath(new UpdateAppSettingsStatusModel()));
		var claims = platformAdmin
			? new[] { new Claim(TenantMembershipMiddleware.PlatformAdminClaim, "true") }
			: [];
		controller.ControllerContext.HttpContext = new DefaultHttpContext
		{
			RequestServices = services.BuildServiceProvider(),
			User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
		};
		return ( ( JsonResult ) controller.Env() ).Value as AppSettings ?? new AppSettings();
	}

	[TestMethod]
	public void Env_OneTenant_ShowsTheHostFolders()
	{
		Assert.IsTrue(EnvFor(["default"], false).StorageFolder.Contains("secret"));
	}

	[TestMethod]
	public void Env_SeveralTenants_HidesTheHostFoldersFromMembers()
	{
		var env = EnvFor(["default", "acme"], false);

		Assert.IsFalse(env.StorageFolder.Contains("secret"));
		Assert.IsFalse(env.TenantsRoot.Contains("tenants"));
		Assert.IsFalse(env.TempFolder.Contains(System.IO.Path.GetTempPath()));
		Assert.IsEmpty(env.StorageFolderMappings);
	}

	[TestMethod]
	public void Env_SeveralTenants_PlatformAdministratorSeesThem()
	{
		var env = EnvFor(["default", "acme"], true);

		Assert.IsTrue(env.StorageFolder.Contains("secret"));
	}
}
