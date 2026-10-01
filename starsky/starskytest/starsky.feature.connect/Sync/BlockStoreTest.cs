using System;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.connect.Sync;
using starsky.foundation.connect.Protocol.Generated;

namespace starskytest.starsky.feature.connect.Sync;

[TestClass]
public sealed class BlockStoreTest
{
	private string _folder = string.Empty;

	[TestInitialize]
	public void Setup()
	{
		_folder = Path.Combine(Path.GetTempPath(), "blockstoretest_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(_folder);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if ( Directory.Exists(_folder) )
		{
			Directory.Delete(_folder, recursive: true);
		}
	}

	[TestMethod]
	public async Task WriteBlockAsync_CreatesPartFile()
	{
		using var store = new BlockStore();
		var data = new byte[] { 1, 2, 3, 4 };

		await store.WriteBlockAsync(_folder, "sub/file.txt", offset: 0, data: data);

		var tempPath = Path.Combine(_folder, ".starskyconnect_tmp",
			"sub" + Path.DirectorySeparatorChar + "file.txt.part");
		Assert.IsTrue(File.Exists(tempPath));
		CollectionAssert.AreEqual(data, File.ReadAllBytes(tempPath));
	}

	[TestMethod]
	public async Task ReadBlockAsync_ReturnsWrittenData()
	{
		using var store = new BlockStore();
		var data = new byte[] { 10, 20, 30 };
		await store.WriteBlockAsync(_folder, "file.bin", offset: 0, data: data);

		var result = await store.ReadBlockAsync(_folder, "file.bin", offset: 0, size: 3);

		Assert.IsNotNull(result);
		CollectionAssert.AreEqual(data, result.Value.ToArray());
	}

	[TestMethod]
	public async Task ReadBlockAsync_ReturnsNull_WhenFileNotFound()
	{
		using var store = new BlockStore();

		var result = await store.ReadBlockAsync(_folder, "missing.bin", offset: 0, size: 4);

		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task ReadBlockAsync_ReturnsNull_OnSizeMismatch()
	{
		using var store = new BlockStore();
		var data = new byte[] { 1, 2 };
		await store.WriteBlockAsync(_folder, "small.bin", offset: 0, data: data);

		// Ask for 10 bytes but file only has 2
		var result = await store.ReadBlockAsync(_folder, "small.bin", offset: 0, size: 10);

		Assert.IsNull(result);
	}

	[TestMethod]
	public void IsComplete_ReturnsTrue_ForEmptyBlocks()
	{
		using var store = new BlockStore();

		var result = store.IsComplete(_folder, "any.txt", blocks: []);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsComplete_ReturnsFalse_WhenNothingReceived()
	{
		using var store = new BlockStore();
		var blocks = new[]
		{
			new BlockInfo { Offset = 0, Size = 4, Hash = ByteString.CopyFrom(new byte[32]) }
		};

		var result = store.IsComplete(_folder, "file.txt", blocks);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task IsComplete_ReturnsTrue_WhenAllBlocksReceived()
	{
		using var store = new BlockStore();
		var data = new byte[4];
		await store.WriteBlockAsync(_folder, "file.txt", offset: 0, data: data);

		var blocks = new[]
		{
			new BlockInfo { Offset = 0, Size = 4, Hash = ByteString.CopyFrom(new byte[32]) }
		};

		Assert.IsTrue(store.IsComplete(_folder, "file.txt", blocks));
	}

	[TestMethod]
	public async Task AssembleFileAsync_MovesFileToDestination()
	{
		using var store = new BlockStore();
		var data = new byte[] { 5, 6, 7 };
		await store.WriteBlockAsync(_folder, "out.bin", offset: 0, data: data);

		var dest = Path.Combine(_folder, "result", "out.bin");
		await store.AssembleFileAsync(_folder, "out.bin", dest);

		Assert.IsTrue(File.Exists(dest));
		CollectionAssert.AreEqual(data, File.ReadAllBytes(dest));
	}

	[TestMethod]
	public async Task AssembleFileAsync_OverwritesExistingDestination()
	{
		using var store = new BlockStore();
		var data = new byte[] { 9 };
		await store.WriteBlockAsync(_folder, "f.bin", offset: 0, data: data);

		var dest = Path.Combine(_folder, "f_dest.bin");
		File.WriteAllBytes(dest, new byte[] { 1, 2, 3 });

		await store.AssembleFileAsync(_folder, "f.bin", dest);

		CollectionAssert.AreEqual(data, File.ReadAllBytes(dest));
	}

	[TestMethod]
	public async Task Forget_RemovesTrackingAndDeletesTempFile()
	{
		using var store = new BlockStore();
		var data = new byte[] { 1 };
		await store.WriteBlockAsync(_folder, "f.txt", offset: 0, data: data);

		store.Forget(_folder, "f.txt");

		// After Forget, IsComplete should return false (state is gone)
		var blocks = new[] { new BlockInfo { Offset = 0, Size = 1 } };
		Assert.IsFalse(store.IsComplete(_folder, "f.txt", blocks));

		var tempPath = Path.Combine(_folder, ".starskyconnect_tmp", "f.txt.part");
		Assert.IsFalse(File.Exists(tempPath));
	}

	[TestMethod]
	public void Forget_DoesNotThrow_WhenKeyUnknown()
	{
		using var store = new BlockStore();
		store.Forget(_folder, "nonexistent.txt");
	}

	[TestMethod]
	public void Dispose_DisposesAllLocks()
	{
		var store = new BlockStore();
		store.Dispose();
		// No assertion needed — must not throw
	}
}
