using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.connect.Protocol;

namespace starskytest.starsky.foundation.connect.Protocol;

[TestClass]
public sealed class CompressionTest
{
	[TestMethod]
	public void Compress_Decompress_RoundTrip()
	{
		var original = Encoding.UTF8.GetBytes("hello syncthing block exchange protocol");
		var compressed = Compression.Compress(original);
		var decompressed = Compression.Decompress(compressed);
		CollectionAssert.AreEqual(original, decompressed);
	}

	[TestMethod]
	public void Compress_LengthPrefixMatchesOriginalSize()
	{
		var original = new byte[1000];
		Random.Shared.NextBytes(original);

		var compressed = Compression.Compress(original);

		// First 4 bytes = big-endian uncompressed length
		var declaredLength = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(compressed);
		Assert.AreEqual((uint)original.Length, declaredLength);
	}

	[TestMethod]
	public void Decompress_TruncatedData_ThrowsBepProtocolException()
	{
		// Fewer than 4 bytes — can't read the length prefix
		Assert.ThrowsExactly<BepProtocolException>(
			() => Compression.Decompress([0x00, 0x01]));
	}

	[TestMethod]
	public void Compress_EmptyInput_RoundTrips()
	{
		var original = Array.Empty<byte>();
		var compressed = Compression.Compress(original);
		var decompressed = Compression.Decompress(compressed);
		Assert.AreEqual(0, decompressed.Length);
	}

	[TestMethod]
	public void Compress_LargeRepeatingData_IsSmaller()
	{
		var original = new byte[64 * 1024];
		Array.Fill(original, (byte)0xAB);
		var compressed = Compression.Compress(original);

		// Highly repetitive data should compress to well under half size
		Assert.IsTrue(compressed.Length < original.Length / 2,
			$"Compressed size {compressed.Length} should be much less than {original.Length}.");
	}
}
