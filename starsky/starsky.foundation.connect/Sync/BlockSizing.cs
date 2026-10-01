namespace starsky.foundation.connect.Sync;

/// <summary>
/// Selects the BEP v1 block size for a given file, ensuring fewer than 2000 blocks.
/// </summary>
public static class BlockSizing
{
	private const int MinBlockSize = 128 * 1024;       // 128 KiB
	private const int MaxBlockSize = 16 * 1024 * 1024; // 16 MiB

	public static int SelectBlockSize(long fileSize)
	{
		var blockSize = MinBlockSize;
		while ( blockSize < MaxBlockSize && ( fileSize + blockSize - 1 ) / blockSize > 2000 )
		{
			blockSize <<= 1;
		}

		return blockSize;
	}
}
