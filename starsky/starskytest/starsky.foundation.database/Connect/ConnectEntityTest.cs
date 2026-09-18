using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.Connect;

[TestClass]
public sealed class ConnectEntityTest : DatabaseTest
{
	[TestMethod]
	public async Task ConnectFileMeta_SaveAndReload_RoundTrip()
	{
		var meta = new ConnectFileMeta
		{
			Folder = "default",
			Name = "photos/img.jpg",
			Version = [0x01, 0x02],
			Sequence = 42,
			BlockSize = 128 * 1024,
			Deleted = false,
			Invalid = false,
			NoPermissions = false,
			ModifiedBy = ( long )12345678UL,
			SymlinkTarget = string.Empty,
		};

		DbContext.ConnectFileMetas.Add(meta);
		await DbContext.SaveChangesAsync();

		var loaded = await DbContext.ConnectFileMetas
			.FirstAsync(m => m.Folder == "default" && m.Name == "photos/img.jpg");

		Assert.AreEqual(42, loaded.Sequence);
		Assert.AreEqual(128 * 1024, loaded.BlockSize);
		CollectionAssert.AreEqual(new byte[] { 0x01, 0x02 }, loaded.Version);
	}

	[TestMethod]
	public async Task ConnectBlockInfo_UniqueIndex_EnforcedOnFolderNameOffset()
	{
		var block1 = new ConnectBlockInfo
		{
			Folder = "default",
			Name = "photos/img.jpg",
			Offset = 0,
			Size = 128 * 1024,
			Hash = new byte[32],
		};

		DbContext.ConnectBlockInfos.Add(block1);
		await DbContext.SaveChangesAsync();

		var duplicate = new ConnectBlockInfo
		{
			Folder = "default",
			Name = "photos/img.jpg",
			Offset = 0,          // same offset → unique constraint violation
			Size = 128 * 1024,
			Hash = new byte[32],
		};

		DbContext.ConnectBlockInfos.Add(duplicate);
		await Assert.ThrowsExactlyAsync<DbUpdateException>(
			() => DbContext.SaveChangesAsync());
	}

	[TestMethod]
	public async Task ConnectFolderMeta_SaveAndReload_RoundTrip()
	{
		var meta = new ConnectFolderMeta
		{
			Folder = "default",
			IndexId = 9876543210L,
			Sequence = 99,
		};

		DbContext.ConnectFolderMetas.Add(meta);
		await DbContext.SaveChangesAsync();

		var loaded = await DbContext.ConnectFolderMetas.FindAsync("default");
		Assert.IsNotNull(loaded);
		Assert.AreEqual(9876543210L, loaded.IndexId);
		Assert.AreEqual(99, loaded.Sequence);
	}

	[TestMethod]
	public async Task ConnectDeviceIndex_SaveAndReload_RoundTrip()
	{
		var deviceId = new byte[32];
		deviceId[0] = 0xAB;

		var idx = new ConnectDeviceIndex
		{
			Folder = "default",
			DeviceId = deviceId,
			MaxSequence = 100,
			IndexId = 555,
		};

		DbContext.ConnectDeviceIndexes.Add(idx);
		await DbContext.SaveChangesAsync();

		var loaded = await DbContext.ConnectDeviceIndexes
			.FirstAsync(d => d.Folder == "default");

		Assert.AreEqual(100, loaded.MaxSequence);
		Assert.AreEqual(555, loaded.IndexId);
		Assert.AreEqual(0xAB, loaded.DeviceId[0]);
	}

	[TestMethod]
	public async Task ConnectFileMeta_SoftJoin_WithFileIndexItem()
	{
		// Add a FileIndexItem representing the actual file on disk
		var fileItem = new FileIndexItem
		{
			FileName = "img.jpg",
			ParentDirectory = "/photos",
			Size = 204800,
			FileHash = "abc123",
		};
		DbContext.FileIndex.Add(fileItem);

		// Add a ConnectFileMeta for the same file (soft join by folder+name)
		var meta = new ConnectFileMeta
		{
			Folder = "default",
			Name = "photos/img.jpg",
			Sequence = 7,
			BlockSize = 128 * 1024,
			Version = [0x08, 0x01],
		};
		DbContext.ConnectFileMetas.Add(meta);
		await DbContext.SaveChangesAsync();

		// Simulate what the sync engine does: join by constructed path
		var filePath = "/" + meta.Name;
		var fromDb = await DbContext.FileIndex
			.FirstOrDefaultAsync(f => f.FilePath == filePath);

		var connectMeta = await DbContext.ConnectFileMetas
			.FirstAsync(m => m.Folder == "default" && m.Name == "photos/img.jpg");

		Assert.IsNotNull(fromDb);
		Assert.AreEqual(204800, fromDb.Size);
		Assert.AreEqual(7, connectMeta.Sequence);
	}
}
