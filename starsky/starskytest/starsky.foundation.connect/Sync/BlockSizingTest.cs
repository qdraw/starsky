using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.connect.Sync;

namespace starskytest.starsky.foundation.connect.Sync;

[TestClass]
public sealed class BlockSizingTest
{
	[TestMethod]
	[DataRow(0L, 128 * 1024)]
	[DataRow(1L, 128 * 1024)]
	[DataRow(128 * 1024L, 128 * 1024)]
	[DataRow(250L * 1024 * 1024, 128 * 1024)]         // just under 250 MiB
	[DataRow(250L * 1024 * 1024 + 1, 256 * 1024)]     // just over 250 MiB → 256 KiB
	[DataRow(500L * 1024 * 1024 + 1, 512 * 1024)]
	[DataRow(1L * 1024 * 1024 * 1024 + 1, 1 * 1024 * 1024)]
	[DataRow(2L * 1024 * 1024 * 1024 + 1, 2 * 1024 * 1024)]
	[DataRow(4L * 1024 * 1024 * 1024 + 1, 4 * 1024 * 1024)]
	[DataRow(8L * 1024 * 1024 * 1024 + 1, 8 * 1024 * 1024)]
	[DataRow(16L * 1024 * 1024 * 1024, 16 * 1024 * 1024)] // 16 GiB → max 16 MiB
	[DataRow(100L * 1024 * 1024 * 1024, 16 * 1024 * 1024)] // very large → capped at 16 MiB
	public void SelectBlockSize_ReturnsExpectedSize(long fileSize, int expectedBlockSize)
	{
		var actual = BlockSizing.SelectBlockSize(fileSize);
		Assert.AreEqual(expectedBlockSize, actual,
			$"File size {fileSize} bytes should use block size {expectedBlockSize} bytes.");
	}

	[TestMethod]
	public void SelectBlockSize_ResultIsAlwaysPowerOfTwo()
	{
		foreach ( var size in new long[] { 0, 1, 1024, 1024 * 1024, 1024L * 1024 * 1024 } )
		{
			var block = BlockSizing.SelectBlockSize(size);
			Assert.IsTrue(( block & ( block - 1 ) ) == 0,
				$"Block size {block} for file size {size} must be a power of two.");
		}
	}
}
