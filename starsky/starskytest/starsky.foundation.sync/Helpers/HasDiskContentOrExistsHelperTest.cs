using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.sync.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.Helpers;

[TestClass]
public sealed class HasDiskContentOrExistsHelperTest
{
	[TestMethod]
	public async Task HasDiskContentOrExists_FolderExists_ReturnsTrue()
	{
		var storage = new FakeIStorage(["/myfolder"]);
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/myfolder");

		Assert.IsTrue(has);
		Assert.AreEqual("folder exists on disk", reason);
	}

	[TestMethod]
	public async Task HasDiskContentOrExists_SubdirectoriesExist_ReturnsTrue()
	{
		// parent folder not present but subdirectory exists
		var storage = new FakeIStorage(["/parent/sub"]);
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/parent");

		Assert.IsTrue(has);
		Assert.AreEqual("subdirectories exist on disk", reason);
	}

	[TestMethod]
	public async Task HasDiskContentOrExists_FilesExist_ReturnsTrue()
	{
		var storage = new FakeIStorage(["/"], ["/parent/file.jpg"], new List<byte[]?> { null });
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/parent");

		Assert.IsTrue(has);
		Assert.AreEqual("files exist on disk", reason);
	}

	[TestMethod]
	public async Task HasDiskContentOrExists_RetryFindsFolder_ReturnsTrue()
	{
		var storage = new FakeIStorageTransient(true);
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/transient");

		Assert.IsTrue(has);
		Assert.AreEqual("folder exists on disk", reason);
	}

	[TestMethod]
	public async Task HasDiskContentOrExists_RetryFindsSubdirectories_ReturnsTrue()
	{
		var storage = new FakeIStorageTransient(subdirsAppearOnSecondCall: true);
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/transient");

		Assert.IsTrue(has);
		Assert.AreEqual("subdirectories exist on disk", reason);
	}

	[TestMethod]
	public async Task HasDiskContentOrExists_RetryFindsFiles_ReturnsTrue()
	{
		var storage = new FakeIStorageTransient(filesAppearOnSecondCall: true);
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/transient");

		Assert.IsTrue(has);
		Assert.AreEqual("files exist on disk", reason);
	}

	[TestMethod]
	public async Task HasDiskContentOrExists_NoContent_ReturnsFalse()
	{
		var storage = new FakeIStorage([], [], new List<byte[]>());
		var helper = new HasDiskContentOrExistsHelper(storage);

		var (has, reason) = await helper.HasDiskContentOrExistsAsync("/nothing");

		Assert.IsFalse(has);
		Assert.AreEqual("folder missing and no content found", reason);
	}
}
