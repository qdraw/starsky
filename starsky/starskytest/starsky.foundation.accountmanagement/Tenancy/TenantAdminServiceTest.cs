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
			new FakeIWebLogger(), null, setup.AppSettings);
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
	public async Task Create_ExplicitFolders_Rules()
	{
		await using var setup = await TenancyTestSetup.Create();
		var owner = await SeedOwner(setup);
		await using var context = setup.ContextFor("default");
		var service = Service(setup, context);

		// top level folders and volume roots are not accepted
		var rootFolder = Path.GetPathRoot(Path.GetTempPath())!;
		var topLevel = Path.Combine(rootFolder, "etc");
		Assert.AreEqual(TenantAdminResult.InvalidFolders,
			await service.CreateAsync("t1", "x", owner, new TenantRoots(rootFolder)));
		Assert.AreEqual(TenantAdminResult.InvalidFolders,
			await service.CreateAsync("t2", "x", owner, new TenantRoots(topLevel)));

		// the folders of one tenant may not overlap each other
		var shared = Path.Combine(Path.GetTempPath(), "starsky-own-" + System.Guid.NewGuid());
		try
		{
			Assert.AreEqual(TenantAdminResult.InvalidFolders,
				await service.CreateAsync("t3", "x", owner,
					new TenantRoots(shared, Path.Combine(shared, "thumbs"),
						Path.Combine(shared, "temp"))));
			Assert.AreEqual(TenantAdminResult.InvalidFolders,
				await service.CreateAsync("t4", "x", owner,
					new TenantRoots(shared, null, shared)));
			Assert.AreEqual(0, await context.Tenants.CountAsync(TestContext.CancellationToken));
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
	public async Task Create_DoesNotOverlapADisabledTenant_AndEnableIsValidatedAgain()
	{
		await using var setup = await TenancyTestSetup.Create();
		var owner = await SeedOwner(setup);
		await using var context = setup.ContextFor("default");
		var service = Service(setup, context);
		var folder = Path.Combine(Path.GetTempPath(), "starsky-disabled-" + System.Guid.NewGuid());
		try
		{
			Assert.AreEqual(TenantAdminResult.Ok,
				await service.CreateAsync("old", "x", owner, new TenantRoots(Path.Combine(folder, "a"))));
			await service.SetEnabledAsync("old", false);

			// a disabled tenant can come back, so its folders count
			Assert.AreEqual(TenantAdminResult.InvalidFolders,
				await service.CreateAsync("new", "x", owner,
					new TenantRoots(Path.Combine(folder, "a", "inside"))));

			// the same when the overlap was made behind the back of the service (an old database)
			context.Tenants.Add(new TenantItem
			{
				Id = "sneaky", DisplayName = "s", State = TenantState.Disabled,
				StorageFolder = Path.Combine(folder, "a", "storage")
			});
			await context.SaveChangesAsync(TestContext.CancellationToken);
			Assert.AreEqual(TenantAdminResult.InvalidFolders,
				await service.SetEnabledAsync("sneaky", true));
			Assert.IsFalse(setup.Registry.IsActive("sneaky"));
		}
		finally
		{
			if ( Directory.Exists(folder) )
			{
				Directory.Delete(folder, true);
			}
		}
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

	[TestMethod]
	public async Task Purge_RemovesTheDataOfTheTenant_AndNothingOfTheOthers()
	{
		await using var setup = await TenancyTestSetup.Create();
		var owner = await SeedOwner(setup);
		await using var adminContext = setup.ContextFor("default");
		var service = Service(setup, adminContext);
		await service.CreateAsync("acme", "Acme", owner);
		await service.CreateAsync("beta", "Beta", owner);

		foreach ( var tenant in new[] { "default", "acme", "beta" } )
		{
			await using var context = setup.ContextFor(tenant);
			context.FileIndex.Add(new FileIndexItem("/same.jpg"));
			context.Settings.Add(new SettingsItem { Key = "k", Value = tenant });
			context.Thumbnails.Add(new ThumbnailItem("hash", true, true, true, true));
			context.Notifications.Add(new NotificationItem
			{
				Content = tenant, DateTime = System.DateTime.UtcNow, DateTimeEpoch = 1
			});
			context.ImportIndex.Add(new ImportIndexItem { FilePath = "/i.jpg", FileHash = "h" });
			context.QueueItems.Add(new QueueItem
			{
				QueueName = "q", JobType = "t", JobId = System.Guid.NewGuid()
			});
			await context.SaveChangesAsync(TestContext.CancellationToken);
		}

		var acmeFolder = setup.SettingsProvider.GetSettings("acme").StorageFolder;
		var betaFolder = setup.SettingsProvider.GetSettings("beta").StorageFolder;
		Assert.IsTrue(Directory.Exists(acmeFolder));
		await service.DeleteAsync("acme");

		Assert.AreEqual(TenantAdminResult.Ok, await service.PurgeAsync("acme"));

		Assert.IsFalse(Directory.Exists(Path.GetDirectoryName(acmeFolder.TrimEnd(
			Path.DirectorySeparatorChar))), "the folder of the tenant is gone");
		Assert.IsTrue(Directory.Exists(betaFolder), "another tenant keeps its folder");

		foreach ( var tenant in new[] { "default", "beta" } )
		{
			await using var context = setup.ContextFor(tenant);
			Assert.AreEqual(1, await context.FileIndex.CountAsync(TestContext.CancellationToken));
			Assert.AreEqual(1, await context.Settings.CountAsync(TestContext.CancellationToken));
			Assert.AreEqual(1, await context.Thumbnails.CountAsync(TestContext.CancellationToken));
			Assert.AreEqual(1, await context.Notifications.CountAsync(TestContext.CancellationToken));
			Assert.AreEqual(1, await context.ImportIndex.CountAsync(TestContext.CancellationToken));
		}

		await using var all = setup.ContextFor("default");
		all.EnableCrossTenantAccess();
		Assert.AreEqual(2, await all.FileIndex.CountAsync(TestContext.CancellationToken));
		Assert.AreEqual(2, await all.QueueItems.CountAsync(TestContext.CancellationToken));
		Assert.AreEqual(0, await all.UserRoles.CountAsync(p => p.TenantId == "acme",
			TestContext.CancellationToken));
		Assert.AreEqual(1, await all.UserRoles.CountAsync(p => p.TenantId == "beta",
			TestContext.CancellationToken));

		var purged = await all.Tenants.SingleAsync(t => t.Id == "acme", TestContext.CancellationToken);
		Assert.AreEqual(TenantState.Deleted, purged.State);
		// the id is never free again
		Assert.AreEqual(TenantAdminResult.AlreadyExists,
			await service.CreateAsync("acme", "again", owner));
	}

	[TestMethod]
	public async Task Purge_OnlyAfterDelete_AndNotTheDefaultTenant()
	{
		await using var setup = await TenancyTestSetup.Create();
		var owner = await SeedOwner(setup);
		await using var context = setup.ContextFor("default");
		var service = Service(setup, context);
		await service.CreateAsync("acme", "Acme", owner);

		// an active, or only disabled, tenant is not purged
		Assert.AreEqual(TenantAdminResult.InvalidState, await service.PurgeAsync("acme"));
		await service.SetEnabledAsync("acme", false);
		Assert.AreEqual(TenantAdminResult.InvalidState, await service.PurgeAsync("acme"));

		Assert.AreEqual(TenantAdminResult.DefaultTenant, await service.PurgeAsync("default"));
		Assert.AreEqual(TenantAdminResult.NotFound, await service.PurgeAsync("nope"));
		Assert.IsTrue(Directory.Exists(setup.SettingsProvider.Build("acme", null).StorageFolder));
	}

	[TestMethod]
	public async Task Purge_ExplicitFolders_AreLeftAlone()
	{
		await using var setup = await TenancyTestSetup.Create();
		var owner = await SeedOwner(setup);
		await using var context = setup.ContextFor("default");
		var service = Service(setup, context);
		var folder = Path.Combine(Path.GetTempPath(), "starsky-explicit-" + System.Guid.NewGuid());
		try
		{
			await service.CreateAsync("acme", "Acme", owner, new TenantRoots(Path.Combine(folder, "photos")));
			File.WriteAllText(Path.Combine(folder, "photos", "mine.jpg"), "x");
			await service.DeleteAsync("acme");

			Assert.AreEqual(TenantAdminResult.Ok, await service.PurgeAsync("acme"));

			// the files of whoever pointed to this folder stay
			Assert.IsTrue(File.Exists(Path.Combine(folder, "photos", "mine.jpg")));
		}
		finally
		{
			if ( Directory.Exists(folder) )
			{
				Directory.Delete(folder, true);
			}
		}
	}

	[TestMethod]
	public async Task CrossTenantAccess_IsOnlyForTheOperation()
	{
		await using var setup = await TenancyTestSetup.Create();
		await using var context = setup.ContextFor("default");

		var seen = await context.RunCrossTenantAsync(() =>
			Task.FromResult(context.CrossTenantAccess));

		Assert.IsTrue(seen);
		Assert.IsFalse(context.CrossTenantAccess);
	}
}
