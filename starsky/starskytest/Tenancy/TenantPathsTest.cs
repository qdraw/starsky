using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.sync.WatcherHelpers;
using starskytest.FakeMocks;

namespace starskytest.Tenancy;

[TestClass]
public sealed class TenantPathsTest
{
	private readonly string _root =
		Path.Combine(Path.GetTempPath(), "starsky-paths-" + Guid.NewGuid().ToString("N"));

	[TestCleanup]
	public void Cleanup()
	{
		TenantAmbient.Reset();
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

	private (AppSettings AppSettings, TenantSettingsProvider Provider) CreateProvider()
	{
		var appSettings = CreateAppSettings();
		return ( appSettings,
			new TenantSettingsProvider(appSettings,
				new FakeTenantRegistry("default", "acme", "beta")) );
	}

	private static void Touch(string folder, string name)
	{
		Directory.CreateDirectory(folder);
		File.WriteAllText(Path.Combine(folder, name), "x");
	}

	[TestMethod]
	public void ScopedTenantSettings_FollowsTheTenantContext_EvenWhenSetLater()
	{
		var (_, provider) = CreateProvider();
		var context = new TenantContext();
		var scoped = new ScopedTenantSettings(context, provider);

		context.SetTenant("default", false);
		var defaultFolder = scoped.StorageFolder;
		// the middleware sets the tenant after the scope was created
		context.SetTenant("acme", false);

		Assert.AreEqual("acme", scoped.TenantId);
		Assert.AreNotEqual(defaultFolder, scoped.StorageFolder);
		Assert.AreEqual(provider.GetSettings("acme").StorageFolder, scoped.StorageFolder);
		Assert.AreEqual(provider.GetSettings("acme").DatabasePathToFilePath("/a.jpg"),
			scoped.DatabasePathToFilePath("/a.jpg"));
		Assert.AreEqual("/a.jpg", scoped.FullPathStorageFolderToDatabaseStyle(
			provider.GetSettings("acme").DatabasePathToFilePath("/a.jpg")));
	}

	[TestMethod]
	public void FullPathToDatabaseStyle_OfAnotherTenantsFolder_IsNotMapped()
	{
		var (_, provider) = CreateProvider();
		var acme = provider.GetSettings("acme");
		var defaultSettings = provider.GetSettings("default");

		var fullPath = acme.DatabasePathToFilePath("/only-acme.jpg");

		Assert.AreEqual("/only-acme.jpg", acme.FullPathStorageFolderToDatabaseStyle(fullPath));
		// the default tenant does not own this folder: not a subPath of the default tenant
		Assert.AreNotEqual("/only-acme.jpg",
			defaultSettings.FullPathStorageFolderToDatabaseStyle(fullPath));
	}

	private sealed class TestSelectorStorage(
		AppSettings appSettings,
		TenantSettingsProvider provider,
		string tenantId) : ISelectorStorage
	{
		public IStorage Get(SelectorStorage.StorageServices storageServices)
		{
			var logger = new FakeIWebLogger();
			var context = new TenantContext();
			context.SetTenant(tenantId, false);
			return storageServices switch
			{
				SelectorStorage.StorageServices.SubPath => new StorageSubPathFilesystem(
					appSettings, logger, context, provider),
				SelectorStorage.StorageServices.Temporary => new StorageTemporaryFilesystem(
					appSettings, logger, context, provider),
				SelectorStorage.StorageServices.Thumbnail => new StorageThumbnailFilesystem(
					appSettings, logger, context, provider),
				_ => new StorageHostFullPathFilesystem(logger)
			};
		}
	}

	[TestMethod]
	public async Task FullFilePathExists_ForATenant_DoesNotFindTheFileOfTheDefaultTenant()
	{
		var (appSettings, provider) = CreateProvider();
		Touch(appSettings.StorageFolder, "same.jpg");
		Touch(provider.GetSettings("acme").StorageFolder, "only-acme.jpg");
		Directory.CreateDirectory(provider.GetSettings("acme").TempFolder);

		var forDefault = new FullFilePathExistsService(
			new TestSelectorStorage(appSettings, provider, "default"), appSettings,
			provider.GetSettings("default"));
		var forAcme = new FullFilePathExistsService(
			new TestSelectorStorage(appSettings, provider, "acme"), appSettings,
			provider.GetSettings("acme"));

		var defaultSame = await forDefault.GetFullFilePath("/same.jpg", "hash");
		var acmeSame = await forAcme.GetFullFilePath("/same.jpg", "hash");
		var acmeOnly = await forAcme.GetFullFilePath("/only-acme.jpg", "hash");

		Assert.IsTrue(defaultSame.IsSuccess);
		Assert.AreEqual(appSettings.DatabasePathToFilePath("/same.jpg"), defaultSame.FullFilePath);
		// the same subPath in acme: no such file there, and never the file of the default tenant
		Assert.IsFalse(acmeSame.IsSuccess);
		Assert.IsTrue(acmeOnly.IsSuccess);
		Assert.StartsWith(provider.GetSettings("acme").StorageFolder, acmeOnly.FullFilePath);
	}

	[TestMethod]
	public async Task SyncWatcherConnector_MapsTheFullPathToTheTenantSubPath()
	{
		var (appSettings, provider) = CreateProvider();
		var sync = new FakeISynchronize();
		var services = new ServiceCollection();
		services.AddSingleton(appSettings);
		services.AddSingleton<ITenantSettingsProvider>(provider);
		services.AddScoped<ITenantContext, TenantContext>();
		services.AddScoped<ITenantSettings, ScopedTenantSettings>();
		services.AddSingleton<ISynchronize>(sync);
		services.AddSingleton<IWebLogger, FakeIWebLogger>();
		services.AddScoped<IWebSocketConnectionsService, FakeIWebSocketConnectionsService>();
		services.AddScoped<IQuery, FakeIQuery>();
		await using var serviceProvider = services.BuildServiceProvider();
		var factory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		// a job of acme: the ambient tenant is restored by the queue
		TenantAmbient.Set("acme");
		var acmeFile = provider.GetSettings("acme").DatabasePathToFilePath("/only-acme.jpg");
		await new SyncWatcherConnector(factory).Sync(
			new Tuple<string, string?, WatcherChangeTypes>(acmeFile, null,
				WatcherChangeTypes.Created));

		Assert.AreEqual("/only-acme.jpg", sync.Inputs[0].Item1);
	}
}
