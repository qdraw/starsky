using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.connect.Storage;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.connect.Storage;

[TestClass]
public sealed class ConfigStoreTest : DatabaseTest
{
	private string _dir = string.Empty;
	private string _filePath = string.Empty;

	[TestInitialize]
	public void Setup()
	{
		_dir = Path.Combine(Path.GetTempPath(), "configstoretest_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_dir);
		_filePath = Path.Combine(_dir, "connect-config.json");
	}

	[TestCleanup]
	public void Cleanup()
	{
		if ( Directory.Exists(_dir) )
		{
			Directory.Delete(_dir, recursive: true);
		}
	}

	[TestMethod]
	public async Task LoadAsync_ReturnsDefaultConfig_WhenFileAbsent()
	{
		var store = new ConfigStore(_filePath);

		var config = await store.LoadAsync();

		Assert.IsNotNull(config);
		Assert.IsNull(config.CertificatePfxBase64);
		Assert.AreEqual(0, config.Devices.Count);
		Assert.AreEqual(0, config.Folders.Count);
	}

	[TestMethod]
	public async Task SaveAsync_ThenLoadAsync_RoundTripsDevicesAndFolders()
	{
		var store = new ConfigStore(_filePath);
		var config = new ConnectConfig
		{
			DeviceName = "test-host",
			CertificatePfxBase64 = null,
			Devices = [new ConnectDeviceConfig { Id = "DEVID1", Name = "Laptop" }],
			Folders = [new ConnectFolderConfig { Id = "folder1", Label = "Photos", Path = "/tmp/photos" }],
		};

		await store.SaveAsync(config);
		var loaded = await store.LoadAsync();

		Assert.AreEqual("test-host", loaded.DeviceName);
		Assert.AreEqual(1, loaded.Devices.Count);
		Assert.AreEqual("DEVID1", loaded.Devices[0].Id);
		Assert.AreEqual(1, loaded.Folders.Count);
		Assert.AreEqual("folder1", loaded.Folders[0].Id);
	}

	[TestMethod]
	public async Task SaveAsync_WritesCertToDb_WhenDbProvided()
	{
		var store = new ConfigStore(_filePath, DbContext);
		var config = new ConnectConfig { CertificatePfxBase64 = "CERTVALUE" };

		await store.SaveAsync(config);

		var row = await DbContext.Settings.FindAsync("ConnectCertificatePfxBase64");
		Assert.IsNotNull(row);
		Assert.AreEqual("CERTVALUE", row.Value);
	}

	[TestMethod]
	public async Task LoadAsync_DbCertOverridesFileCert()
	{
		// Save a cert via file only first
		var fileCert = "FILECERT";
		var store = new ConfigStore(_filePath);
		await store.SaveAsync(new ConnectConfig { CertificatePfxBase64 = fileCert });

		// Now insert a different cert in DB
		DbContext.Settings.Add(new SettingsItem
		{
			Key = "ConnectCertificatePfxBase64",
			Value = "DBCERT"
		});
		await DbContext.SaveChangesAsync();

		// Load via store that knows about both
		var storeWithDb = new ConfigStore(_filePath, DbContext);
		var config = await storeWithDb.LoadAsync();

		Assert.AreEqual("DBCERT", config.CertificatePfxBase64);
	}

	[TestMethod]
	public async Task SaveAsync_UpdatesExistingDbRow()
	{
		DbContext.Settings.Add(new SettingsItem
		{
			Key = "ConnectCertificatePfxBase64",
			Value = "OLD"
		});
		await DbContext.SaveChangesAsync();

		var store = new ConfigStore(_filePath, DbContext);
		await store.SaveAsync(new ConnectConfig { CertificatePfxBase64 = "NEW" });

		var row = await DbContext.Settings.FindAsync("ConnectCertificatePfxBase64");
		Assert.AreEqual("NEW", row!.Value);
	}

	[TestMethod]
	public async Task SaveAsync_WithoutDb_OnlyWritesFile()
	{
		var store = new ConfigStore(_filePath);
		await store.SaveAsync(new ConnectConfig { CertificatePfxBase64 = "CERT" });

		Assert.IsTrue(File.Exists(_filePath));
		// DB (DbContext) should have nothing
		Assert.IsNull(await DbContext.Settings.FindAsync("ConnectCertificatePfxBase64"));
	}

	[TestMethod]
	public void Default_ReturnsInstance()
	{
		var store = ConfigStore.Default();
		Assert.IsNotNull(store);
	}

	[TestMethod]
	public void Default_WithDb_ReturnsInstance()
	{
		var store = ConfigStore.Default(DbContext);
		Assert.IsNotNull(store);
	}

	[TestMethod]
	public async Task LoadAsync_IgnoresDbWhenCertIsEmpty()
	{
		DbContext.Settings.Add(new SettingsItem
		{
			Key = "ConnectCertificatePfxBase64",
			Value = string.Empty
		});
		await DbContext.SaveChangesAsync();

		var store = new ConfigStore(_filePath, DbContext);
		var config = await store.LoadAsync();

		// Empty DB value should not override file (file has no cert either in this case)
		Assert.IsNull(config.CertificatePfxBase64);
	}
}
