using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Tenancy;

[TestClass]
public sealed class TenantSettingsTest
{
	private readonly string _root =
		Path.Combine(Path.GetTempPath(), "starsky-tenant-test-" + Guid.NewGuid().ToString("N"));

	[TestCleanup]
	public void Cleanup()
	{
		if ( Directory.Exists(_root) )
		{
			Directory.Delete(_root, true);
		}
	}

	private AppSettings CreateAppSettings()
	{
		return new AppSettings
		{
			StorageFolder = Path.Combine(_root, "default", "storage"),
			ThumbnailTempFolder = Path.Combine(_root, "default", "thumbs"),
			TempFolder = Path.Combine(_root, "default", "temp"),
			TenantsRoot = Path.Combine(_root, "tenants")
		};
	}

	[TestMethod]
	public void Default_UsesExistingFolders()
	{
		var appSettings = CreateAppSettings();
		var provider = new TenantSettingsProvider(appSettings, new TenantRegistry());

		var settings = provider.GetSettings("default");

		Assert.AreEqual(appSettings.StorageFolder, settings.StorageFolder);
		Assert.AreEqual(appSettings.ThumbnailTempFolder, settings.ThumbnailTempFolder);
		Assert.AreEqual(appSettings.TempFolder, settings.TempFolder);
	}

	[TestMethod]
	public void Default_FollowsRuntimeEditsOfAppSettings()
	{
		var appSettings = CreateAppSettings();
		var settings = new TenantSettingsProvider(appSettings, new TenantRegistry())
			.GetSettings("default");

		appSettings.StorageFolder = Path.Combine(_root, "changed");

		Assert.AreEqual(appSettings.StorageFolder, settings.StorageFolder);
	}

	[TestMethod]
	public void NonDefault_FallbackLayoutUnderTenantsRoot()
	{
		var appSettings = CreateAppSettings();
		var provider = new TenantSettingsProvider(appSettings,
			new FakeTenantRegistry("default", "acme"));

		var settings = provider.GetSettings("acme");

		Assert.AreEqual(Path.Combine(_root, "tenants", "acme", "storage") + Path.DirectorySeparatorChar,
			settings.StorageFolder);
		Assert.AreEqual(
			Path.Combine(_root, "tenants", "acme", "thumbnails") + Path.DirectorySeparatorChar,
			settings.ThumbnailTempFolder);
		Assert.AreEqual(Path.Combine(_root, "tenants", "acme", "temp") + Path.DirectorySeparatorChar,
			settings.TempFolder);
	}

	[TestMethod]
	public void NonDefault_ExplicitRoots()
	{
		var storage = Path.Combine(_root, "elsewhere", "photos");
		var provider = new TenantSettingsProvider(CreateAppSettings(),
			new FakeTenantRegistry("default", "acme").WithRoots("acme",
				new TenantRoots(storage)));

		var settings = provider.GetSettings("acme");

		Assert.AreEqual(storage + Path.DirectorySeparatorChar, settings.StorageFolder);
		Assert.Contains(Path.Combine("tenants", "acme"), settings.TempFolder);
	}

	[TestMethod]
	public void NonDefault_DoesNotInheritStorageMappings()
	{
		var appSettings = CreateAppSettings();
		appSettings.StorageFolderMappings = new System.Collections.Generic.Dictionary<string, string>
		{
			{ "/2024", Path.Combine(_root, "outside") }
		};
		var provider = new TenantSettingsProvider(appSettings,
			new FakeTenantRegistry("default", "acme"));

		Assert.HasCount(1, provider.GetSettings("default").StorageFolderMappings);
		Assert.IsEmpty(provider.GetSettings("acme").StorageFolderMappings);
	}

	[TestMethod]
	public void GetSettings_InactiveOrInvalid_Throws()
	{
		var provider = new TenantSettingsProvider(CreateAppSettings(), new TenantRegistry());

		Assert.ThrowsExactly<InvalidOperationException>(() => provider.GetSettings("acme"));
		Assert.ThrowsExactly<ArgumentException>(() => provider.GetSettings("../acme"));
	}

	[TestMethod]
	public void ValidateRoots_DefaultAndTenant_Ok()
	{
		var provider = new TenantSettingsProvider(CreateAppSettings(),
			new FakeTenantRegistry("default", "acme", "beta"));

		provider.ValidateRoots();
	}

	[TestMethod]
	public void ValidateRoots_TenantsRootInsideDefaultStorage_Throws()
	{
		var appSettings = CreateAppSettings();
		appSettings.TenantsRoot = Path.Combine(appSettings.StorageFolder, "tenants");
		var provider = new TenantSettingsProvider(appSettings,
			new FakeTenantRegistry("default", "acme"));

		var exception = Assert.ThrowsExactly<InvalidOperationException>(provider.ValidateRoots);
		StringAssert.Contains(exception.Message, "overlap");
	}

	[TestMethod]
	public void ValidateRoots_SameFolderTwoTenants_Throws()
	{
		var shared = Path.Combine(_root, "shared");
		var provider = new TenantSettingsProvider(CreateAppSettings(),
			new FakeTenantRegistry("default", "acme", "beta")
				.WithRoots("acme", new TenantRoots(shared))
				.WithRoots("beta", new TenantRoots(shared)));

		Assert.ThrowsExactly<InvalidOperationException>(provider.ValidateRoots);
	}

	[TestMethod]
	public void ValidateRoots_RelativeFolder_Throws()
	{
		var provider = new TenantSettingsProvider(CreateAppSettings(),
			new FakeTenantRegistry("default", "acme")
				.WithRoots("acme", new TenantRoots("relative/photos")));

		StringAssert.Contains(
			Assert.ThrowsExactly<InvalidOperationException>(provider.ValidateRoots).Message,
			"absolute");
	}

	[TestMethod]
	public void Storage_SameSubPathInTwoTenants_NeverMixes()
	{
		var appSettings = CreateAppSettings();
		var provider = new TenantSettingsProvider(appSettings,
			new FakeTenantRegistry("default", "acme"));
		var logger = new FakeIWebLogger();
		var defaultContext = new TenantContext();
		var acmeContext = new TenantContext();
		acmeContext.SetTenant("acme");

		var defaultStorage = new StorageSubPathFilesystem(appSettings, logger, defaultContext,
			provider);
		var acmeStorage = new StorageSubPathFilesystem(appSettings, logger, acmeContext, provider);
		Directory.CreateDirectory(provider.GetSettings("default").StorageFolder);
		Directory.CreateDirectory(provider.GetSettings("acme").StorageFolder);

		defaultStorage.WriteStream(new MemoryStream(Encoding.UTF8.GetBytes("default")), "/x.jpg");

		Assert.IsTrue(defaultStorage.ExistFile("/x.jpg"));
		Assert.IsFalse(acmeStorage.ExistFile("/x.jpg"));

		acmeStorage.WriteStream(new MemoryStream(Encoding.UTF8.GetBytes("acme")), "/x.jpg");

		Assert.AreEqual("default",
			File.ReadAllText(Path.Combine(appSettings.StorageFolder, "x.jpg")));
		Assert.AreEqual("acme",
			File.ReadAllText(Path.Combine(provider.GetSettings("acme").StorageFolder, "x.jpg")));
	}

	[TestMethod]
	public void Storage_TenantFollowsContextSetAfterConstruction()
	{
		var appSettings = CreateAppSettings();
		var provider = new TenantSettingsProvider(appSettings,
			new FakeTenantRegistry("default", "acme"));
		var context = new TenantContext();
		var storage = new StorageSubPathFilesystem(appSettings, new FakeIWebLogger(), context,
			provider);
		Directory.CreateDirectory(provider.GetSettings("acme").StorageFolder);

		// the middleware sets the tenant after the scope (and the storage) exists
		context.SetTenant("acme");
		storage.WriteStream(new MemoryStream(Encoding.UTF8.GetBytes("a")), "/y.jpg");

		Assert.IsTrue(File.Exists(Path.Combine(provider.GetSettings("acme").StorageFolder, "y.jpg")));
		Assert.IsFalse(File.Exists(Path.Combine(appSettings.StorageFolder, "y.jpg")));
	}

	[TestMethod]
	public void Storage_Traversal_CannotLeaveTenantRoot()
	{
		var appSettings = CreateAppSettings();
		var provider = new TenantSettingsProvider(appSettings,
			new FakeTenantRegistry("default", "acme"));
		var context = new TenantContext();
		context.SetTenant("acme");
		var storage = new StorageSubPathFilesystem(appSettings, new FakeIWebLogger(), context,
			provider);

		Assert.ThrowsExactly<UnauthorizedAccessException>(() =>
			storage.ExistFile("/../other/x.jpg"));
		Assert.ThrowsExactly<UnauthorizedAccessException>(() =>
			storage.ExistFile("/../../default/storage/x.jpg"));
	}

	[TestMethod]
	public void Temporary_And_Thumbnail_UseTenantFolders()
	{
		var appSettings = CreateAppSettings();
		var provider = new TenantSettingsProvider(appSettings,
			new FakeTenantRegistry("default", "acme"));
		var context = new TenantContext();
		context.SetTenant("acme");
		var settings = provider.GetSettings("acme");
		Directory.CreateDirectory(settings.TempFolder);

		var temp = new StorageTemporaryFilesystem(appSettings, new FakeIWebLogger(), context,
			provider);
		temp.WriteStream(new MemoryStream(Encoding.UTF8.GetBytes("t")), "/t.txt");

		Assert.IsTrue(File.Exists(Path.Combine(settings.TempFolder, "t.txt")));
		Assert.IsFalse(File.Exists(Path.Combine(appSettings.TempFolder, "t.txt")));

		Directory.CreateDirectory(settings.ThumbnailTempFolder);
		var thumbs = new StorageThumbnailFilesystem(appSettings, new FakeIWebLogger(), context,
			provider);
		Assert.IsTrue(thumbs.ExistFolder("/"));
		Assert.IsFalse(thumbs.ExistFile("hash"));
	}
}
