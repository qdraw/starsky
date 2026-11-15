using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.trash.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.trash.Services;

[TestClass]
public sealed class SystemTrashMoverTest
{
	[TestMethod]
	[DataRow(true, true, true)]
	[DataRow(true, false, false)]
	[DataRow(false, true, false)]
	public void IsEnabled_NeedsToggleAndSupport(bool toggle, bool supported, bool expected)
	{
		var mover = new SystemTrashMover(new AppSettings { UseSystemTrash = toggle },
			new FakeITrashService(supported));

		Assert.AreEqual(expected, mover.IsEnabled());
		Assert.AreEqual(supported, mover.DetectToUseSystemTrash());
	}

	[TestMethod]
	public void Trash_UsesTheStorageFolderOfTheTenant()
	{
		var trashService = new FakeITrashService();
		var appSettings = new AppSettings { UseSystemTrash = true };
		var tenantSettings = TenantSettings.ForFolders("acme", appSettings,
			"/tenants/acme/storage", "/tenants/acme/thumbnails", "/tenants/acme/temp");
		var mover = new SystemTrashMover(appSettings, trashService, tenantSettings);

		mover.Trash([new FileIndexItem("/a/b.jpg")]);

		Assert.HasCount(1, trashService.InTrash);
		var fullPath = trashService.InTrash.Single();
		Assert.StartsWith("/tenants/acme/storage", fullPath.Replace('\\', '/'));
		Assert.EndsWith("/a/b.jpg".Replace('/', Path.DirectorySeparatorChar), fullPath);
	}

	[TestMethod]
	public void Trash_WithoutTenantSettings_UsesTheDefaultStorageFolder()
	{
		var trashService = new FakeITrashService();
		var appSettings = new AppSettings();

		new SystemTrashMover(appSettings, trashService).Trash([new FileIndexItem("/a/b.jpg")]);

		Assert.AreEqual(appSettings.StorageFolder + "/a/b.jpg".Replace('/', Path.DirectorySeparatorChar),
			trashService.InTrash.Single());
	}
}
