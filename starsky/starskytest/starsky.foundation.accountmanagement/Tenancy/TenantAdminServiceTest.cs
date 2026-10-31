using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Tenancy;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Tenancy;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.accountmanagement.Tenancy;

[TestClass]
public sealed class TenantAdminServiceTest
{
	public TestContext TestContext { get; set; } = null!;

	private static TenantAdminService Service(TenancyTestSetup setup,
		ApplicationDbContext context)
	{
		return new TenantAdminService(context, setup.Registry, setup.SettingsProvider,
			new FakeIWebLogger());
	}

	private async Task<int> SeedOwner(TenancyTestSetup setup)
	{
		await using var context = setup.ContextFor("default");
		return ( await setup.UserManagerFor(context, "default")
			.SignUpAsync("o", "email", "owner@x.nl", "password-1234") ).User!.Id;
	}

	[TestMethod]
	public async Task Create_ProvisionsFolders_Activates_AndMakesCreatorAdministrator()
	{
		await using var setup = await TenancyTestSetup.Create();
		var owner = await SeedOwner(setup);
		await using var context = setup.ContextFor("default");

		var result = await Service(setup, context).CreateAsync("acme", "Acme Inc", owner);

		Assert.AreEqual(TenantAdminResult.Ok, result);
		Assert.IsTrue(setup.Registry.IsActive("acme"));
		var settings = setup.SettingsProvider.GetSettings("acme");
		Assert.IsTrue(Directory.Exists(settings.StorageFolder));
		Assert.IsTrue(Directory.Exists(settings.ThumbnailTempFolder));
		Assert.IsTrue(Directory.Exists(settings.TempFolder));
		Assert.AreEqual(TenantState.Active,
			( await context.Tenants.SingleAsync(TestContext.CancellationToken) ).State);
		var access = await new TenantMembershipService(context).GetAccessAsync(owner, "acme");
		Assert.AreEqual("Administrator", access.Membership?.RoleCode);
	}

	[TestMethod]
	public async Task Create_Rejects_InvalidDefaultAndDuplicate()
	{
		await using var setup = await TenancyTestSetup.Create();
		var owner = await SeedOwner(setup);
		await using var context = setup.ContextFor("default");
		var service = Service(setup, context);

		Assert.AreEqual(TenantAdminResult.DefaultTenant,
			await service.CreateAsync("default", "x", owner));
		Assert.AreEqual(TenantAdminResult.InvalidId, await service.CreateAsync("../x", "x", owner));
		Assert.AreEqual(TenantAdminResult.InvalidId, await service.CreateAsync("Acme", "x", owner));
		Assert.AreEqual(TenantAdminResult.Ok, await service.CreateAsync("acme", "x", owner));
		Assert.AreEqual(TenantAdminResult.AlreadyExists,
			await service.CreateAsync("acme", "x", owner));
	}

	[TestMethod]
	public async Task Create_OverlappingFolders_Rejected_NothingStored()
	{
		await using var setup = await TenancyTestSetup.Create();
		var owner = await SeedOwner(setup);
		await using var context = setup.ContextFor("default");
		var service = Service(setup, context);

		// inside the storage folder of the default tenant: it would index these files
		var inside = Path.Combine(setup.AppSettings.StorageFolder, "acme");
		var result = await service.CreateAsync("acme", "x", owner, new TenantRoots(inside));
		var relative = await service.CreateAsync("beta", "x", owner, new TenantRoots("relative"));

		Assert.AreEqual(TenantAdminResult.InvalidFolders, result);
		Assert.AreEqual(TenantAdminResult.InvalidFolders, relative);
		Assert.AreEqual(0, await context.Tenants.CountAsync(TestContext.CancellationToken));
	}

	[TestMethod]
	public async Task Create_SecondTenantOnSameFolder_Rejected()
	{
		await using var setup = await TenancyTestSetup.Create();
		var owner = await SeedOwner(setup);
		await using var context = setup.ContextFor("default");
		var service = Service(setup, context);
		var shared = Path.Combine(Path.GetTempPath(), "starsky-shared-" + System.Guid.NewGuid());

		try
		{
			Assert.AreEqual(TenantAdminResult.Ok,
				await service.CreateAsync("acme", "x", owner, new TenantRoots(shared)));
			Assert.AreEqual(TenantAdminResult.InvalidFolders,
				await service.CreateAsync("beta", "x", owner, new TenantRoots(shared)));
		}
		finally
		{
			if ( Directory.Exists(shared) )
			{
				Directory.Delete(shared, true);
			}
		}
	}

	[TestMethod]
	public async Task DisableEnableDelete_ChangeWhoCanUseTheTenant()
	{
		await using var setup = await TenancyTestSetup.Create();
		var owner = await SeedOwner(setup);
		await using var context = setup.ContextFor("default");
		var service = Service(setup, context);
		await service.CreateAsync("acme", "x", owner);

		Assert.AreEqual(TenantAdminResult.Ok, await service.SetEnabledAsync("acme", false));
		Assert.IsFalse(setup.Registry.IsActive("acme"));
		Assert.AreEqual(TenantAdminResult.Ok, await service.SetEnabledAsync("acme", true));
		Assert.IsTrue(setup.Registry.IsActive("acme"));

		Assert.AreEqual(TenantAdminResult.Ok, await service.DeleteAsync("acme"));
		Assert.IsFalse(setup.Registry.IsActive("acme"));
		Assert.AreEqual(TenantState.Deleting,
			( await context.Tenants.SingleAsync(TestContext.CancellationToken) ).State);
		// a deleting tenant can not be enabled again
		Assert.AreEqual(TenantAdminResult.InvalidState, await service.SetEnabledAsync("acme", true));
		// files are not removed by this call
		Assert.IsTrue(Directory.Exists(setup.SettingsProvider
			.Build("acme", null).StorageFolder));
	}

	[TestMethod]
	public async Task StateChanges_Default_And_Unknown()
	{
		await using var setup = await TenancyTestSetup.Create();
		await using var context = setup.ContextFor("default");
		var service = Service(setup, context);

		Assert.AreEqual(TenantAdminResult.DefaultTenant, await service.SetEnabledAsync("default", false));
		Assert.AreEqual(TenantAdminResult.DefaultTenant, await service.DeleteAsync("default"));
		Assert.AreEqual(TenantAdminResult.NotFound, await service.SetEnabledAsync("nope", false));
		Assert.AreEqual(TenantAdminResult.NotFound, await service.DeleteAsync("nope"));
	}

	[TestMethod]
	public async Task List_ShowsStates()
	{
		await using var setup = await TenancyTestSetup.Create();
		var owner = await SeedOwner(setup);
		await using var context = setup.ContextFor("default");
		var service = Service(setup, context);
		await service.CreateAsync("acme", "Acme", owner);
		await service.CreateAsync("beta", "Beta", owner);
		await service.SetEnabledAsync("beta", false);

		var list = await service.ListAsync();

		Assert.HasCount(2, list);
		Assert.AreEqual(TenantState.Active, list[0].State);
		Assert.AreEqual(TenantState.Disabled, list[1].State);
	}

	[TestMethod]
	public async Task Registry_ReloadsAfterInvalidate_DefaultAlwaysActive()
	{
		await using var setup = await TenancyTestSetup.Create();
		Assert.IsTrue(setup.Registry.IsActive("default"));
		Assert.IsFalse(setup.Registry.IsActive("acme"));

		await setup.AddActiveTenant("acme");

		Assert.IsTrue(setup.Registry.IsActive("acme"));
		CollectionAssert.AreEquivalent(new[] { "default", "acme" },
			setup.Registry.ActiveTenantIds.ToArray());
		await setup.AddActiveTenant("off", TenantState.Disabled);
		Assert.IsFalse(setup.Registry.IsActive("off"));
	}
}
