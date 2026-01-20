using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Services;

[TestClass]
public sealed class Mp4FileHasherTest
{
	private static FakeIStorage CreateStorageWithMp4(string path, byte[] mp4Data)
	{
		return new FakeIStorage(
			["/"],
			[path],
			new List<byte[]> { mp4Data }
		);
	}

	/// <summary>
	/// Creates a minimal valid MP4 file with ftyp and mdat atoms
	/// </summary>
	private static byte[] CreateMinimalMp4WithMdat(byte[] mdatContent)
	{
		using var ms = new MemoryStream();

		// Write ftyp atom (file type box)
		var ftypSize = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(ftypSize);
		}

		ms.Write(ftypSize, 0, 4);
		ms.Write("ftyp"u8.ToArray(), 0, 4);
		ms.Write("isom"u8.ToArray(), 0, 4); // major brand
		ms.Write(new byte[4], 0, 4); // minor version
		ms.Write("isom"u8.ToArray(), 0, 4); // compatible brand

		// Write mdat atom (media data)
		var mdatSize = ( uint ) ( 8 + mdatContent.Length );
		var mdatSizeBytes = BitConverter.GetBytes(mdatSize);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(mdatSizeBytes);
		}

		ms.Write(mdatSizeBytes, 0, 4);
		ms.Write("mdat"u8.ToArray(), 0, 4);
		ms.Write(mdatContent, 0, mdatContent.Length);

		return ms.ToArray();
	}

	/// <summary>
	/// Creates an MP4 with extended size (size field = 1)
	/// </summary>
	private static byte[] CreateMp4WithExtendedSize(byte[] mdatContent)
	{
		using var ms = new MemoryStream();

		// Write ftyp atom
		var ftypSize = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(ftypSize);
		}

		ms.Write(ftypSize, 0, 4);
		ms.Write("ftyp"u8.ToArray(), 0, 4);
		ms.Write("isom"u8.ToArray(), 0, 4);
		ms.Write(new byte[4], 0, 4);
		ms.Write("isom"u8.ToArray(), 0, 4);

		// Write mdat atom with extended size
		var extendedSizeMarker = BitConverter.GetBytes(( uint ) 1);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(extendedSizeMarker);
		}

		ms.Write(extendedSizeMarker, 0, 4);
		ms.Write("mdat"u8.ToArray(), 0, 4);

		// Write actual size as 64-bit value
		var actualSize = ( ulong ) ( 16 + mdatContent.Length ); // 4 + 4 + 8 + content
		var actualSizeBytes = BitConverter.GetBytes(actualSize);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(actualSizeBytes);
		}

		ms.Write(actualSizeBytes, 0, 8);

		ms.Write(mdatContent, 0, mdatContent.Length);

		return ms.ToArray();
	}

	/// <summary>
	/// Creates an MP4 with multiple atoms before mdat
	/// </summary>
	private static byte[] CreateMp4WithMultipleAtoms(byte[] mdatContent)
	{
		using var ms = new MemoryStream();

		// Write ftyp atom
		var ftypSize = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(ftypSize);
		}

		ms.Write(ftypSize, 0, 4);
		ms.Write("ftyp"u8.ToArray(), 0, 4);
		ms.Write("isom"u8.ToArray(), 0, 4);
		ms.Write(new byte[4], 0, 4);
		ms.Write("isom"u8.ToArray(), 0, 4);

		// Write a moov atom (movie metadata) - empty for test
		var moovContent = new byte[100];
		var moovSize = BitConverter.GetBytes(( uint ) ( 8 + moovContent.Length ));
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(moovSize);
		}

		ms.Write(moovSize, 0, 4);
		ms.Write("moov"u8.ToArray(), 0, 4);
		ms.Write(moovContent, 0, moovContent.Length);

		// Write mdat atom
		var mdatSize = BitConverter.GetBytes(( uint ) ( 8 + mdatContent.Length ));
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(mdatSize);
		}

		ms.Write(mdatSize, 0, 4);
		ms.Write("mdat"u8.ToArray(), 0, 4);
		ms.Write(mdatContent, 0, mdatContent.Length);

		return ms.ToArray();
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_ValidMp4WithMdat_ReturnsHash()
	{
		// Arrange
		var mdatContent = "This is test video content that should be hashed"u8.ToArray();
		var mp4Data = CreateMinimalMp4WithMdat(mdatContent);
		var storage = CreateStorageWithMp4("/test.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/test.mp4");

		// Assert
		Assert.IsNotNull(hash);
		Assert.AreEqual("OW3QBLZ7FNNCSVG4SDBHPYS5LM", hash);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_LargeMdatContent_HashesOnlyFirst256KB()
	{
		// Arrange
		var largeContent = new byte[512 * 1024]; // 512 KB
		for ( var i = 0; i < largeContent.Length; i++ )
		{
			largeContent[i] = ( byte ) ( i % 256 );
		}

		var mp4Data = CreateMinimalMp4WithMdat(largeContent);
		var storage = CreateStorageWithMp4("/large.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/large.mp4");

		// Assert
		Assert.IsNotNull(hash);
		Assert.AreEqual(26, hash.Length);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_ExtendedSizeAtom_ReturnsHash()
	{
		// Arrange
		var mdatContent = "Test content with extended size atom"u8.ToArray();
		var mp4Data = CreateMp4WithExtendedSize(mdatContent);
		var storage = CreateStorageWithMp4("/extended.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/extended.mp4");

		// Assert
		Assert.IsNotNull(hash);
		Assert.AreEqual(26, hash.Length);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_MultipleAtomsBeforeMdat_SkipsToMdat()
	{
		// Arrange
		var mdatContent = "Content after multiple atoms"u8.ToArray();
		var mp4Data = CreateMp4WithMultipleAtoms(mdatContent);
		var storage = CreateStorageWithMp4("/multi.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/multi.mp4");

		// Assert
		Assert.IsNotNull(hash);
		Assert.AreEqual(26, hash.Length);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_NoMdatAtom_ReturnsEmptyString()
	{
		// Arrange - MP4 with only ftyp, no mdat
		using var ms = new MemoryStream();
		var ftypSize = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(ftypSize);
		}

		ms.Write(ftypSize, 0, 4);
		ms.Write("ftyp"u8.ToArray(), 0, 4);
		ms.Write("isom"u8.ToArray(), 0, 4);
		ms.Write(new byte[4], 0, 4);
		ms.Write("isom"u8.ToArray(), 0, 4);

		var mp4Data = ms.ToArray();
		var storage = CreateStorageWithMp4("/no-mdat.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/no-mdat.mp4");

		// Assert
		Assert.AreEqual(string.Empty, hash);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_FileNotFound_ReturnsEmptyString()
	{
		// Arrange
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/nonexistent.mp4");

		// Assert
		Assert.AreEqual(string.Empty, hash);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_EmptyFile_ReturnsEmptyString()
	{
		// Arrange
		var storage = CreateStorageWithMp4("/empty.mp4", []);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/empty.mp4");

		// Assert
		Assert.AreEqual(string.Empty, hash);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_CorruptedAtomHeader_ReturnsEmptyString()
	{
		// Arrange - File with incomplete atom header (less than 8 bytes)
		var corruptData = new byte[] { 0x00, 0x00, 0x00, 0x08, 0x66 };
		var storage = CreateStorageWithMp4("/corrupt.mp4", corruptData);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync(
			"/corrupt.mp4");

		// Assert
		Assert.AreEqual(string.Empty, hash);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_IncompleteExtendedSize_ReturnsEmptyString()
	{
		// Arrange - Atom with size=1 but incomplete extended size bytes
		using var ms = new MemoryStream();
		var sizeOne = BitConverter.GetBytes(( uint ) 1);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(sizeOne);
		}

		ms.Write(sizeOne, 0, 4);
		ms.Write("mdat"u8.ToArray(), 0, 4);
		ms.Write(new byte[4], 0, 4); // Only 4 bytes instead of 8

		var corruptData = ms.ToArray();
		var storage = CreateStorageWithMp4("/incomplete-extended.mp4", corruptData);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/incomplete-extended.mp4");

		// Assert
		Assert.AreEqual(string.Empty, hash);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_SmallMdatContent_HashesAllContent()
	{
		// Arrange
		var smallContent = "Small"u8.ToArray();
		var mp4Data = CreateMinimalMp4WithMdat(smallContent);
		var storage = CreateStorageWithMp4("/small.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/small.mp4");

		// Assert
		Assert.IsNotNull(hash);
		Assert.AreEqual(26, hash.Length);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_SameContent_ReturnsSameHash()
	{
		// Arrange
		var content = "Identical content for hash comparison"u8.ToArray();
		var mp4Data1 = CreateMinimalMp4WithMdat(content);
		var mp4Data2 = CreateMinimalMp4WithMdat(content);

		var storage1 = CreateStorageWithMp4("/file1.mp4", mp4Data1);
		var storage2 = CreateStorageWithMp4("/file2.mp4", mp4Data2);

		var logger = new FakeIWebLogger();
		var hasher1 = new Mp4FileHasher(storage1, logger);
		var hasher2 = new Mp4FileHasher(storage2, logger);

		// Act
		var hash1 = await hasher1.HashMp4VideoContentAsync("/file1.mp4");
		var hash2 = await hasher2.HashMp4VideoContentAsync("/file2.mp4");

		// Assert
		Assert.AreEqual(hash1, hash2);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_DifferentContent_ReturnsDifferentHash()
	{
		// Arrange
		var content1 = "Content one"u8.ToArray();
		var content2 = "Content two"u8.ToArray();
		var mp4Data1 = CreateMinimalMp4WithMdat(content1);
		var mp4Data2 = CreateMinimalMp4WithMdat(content2);

		var storage1 = CreateStorageWithMp4("/file1.mp4", mp4Data1);
		var storage2 = CreateStorageWithMp4("/file2.mp4", mp4Data2);

		var logger = new FakeIWebLogger();
		var hasher1 = new Mp4FileHasher(storage1, logger);
		var hasher2 = new Mp4FileHasher(storage2, logger);

		// Act
		var hash1 = await hasher1.HashMp4VideoContentAsync("/file1.mp4");
		var hash2 = await hasher2.HashMp4VideoContentAsync("/file2.mp4");

		// Assert
		Assert.AreNotEqual(hash1, hash2);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_ZeroSizedMdatAtom_ReturnsHash()
	{
		// Arrange - mdat with zero content
		var mp4Data = CreateMinimalMp4WithMdat([]);
		var storage = CreateStorageWithMp4("/zero-mdat.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/zero-mdat.mp4");

		// Assert
		Assert.IsNotNull(hash);
		Assert.AreEqual(26, hash.Length);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_MdatAtEndOfFile_ReturnsHash()
	{
		// Arrange - Create MP4 where mdat is the last atom
		var mdatContent = "Content at end"u8.ToArray();
		var mp4Data = CreateMp4WithMultipleAtoms(mdatContent);
		var storage = CreateStorageWithMp4("/mdat-at-end.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/mdat-at-end.mp4");

		// Assert
		Assert.IsNotNull(hash);
		Assert.AreEqual(26, hash.Length);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_BinaryContent_HandlesCorrectly()
	{
		// Arrange - Binary content with null bytes and special characters
		var binaryContent = new byte[1000];
		var random = new Random(42); // Fixed seed for reproducibility
		random.NextBytes(binaryContent);

		var mp4Data = CreateMinimalMp4WithMdat(binaryContent);
		var storage = CreateStorageWithMp4("/binary.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/binary.mp4");

		// Assert
		Assert.IsNotNull(hash);
		Assert.AreEqual(26, hash.Length);
		Assert.IsTrue(hash.All(c => char.IsLetterOrDigit(c) || c == '='));
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_Exactly256KBContent_HashesAllContent()
	{
		// Arrange - Exactly at the boundary
		var exactContent = new byte[256 * 1024];
		for ( var i = 0; i < exactContent.Length; i++ )
		{
			exactContent[i] = ( byte ) ( i % 256 );
		}

		var mp4Data = CreateMinimalMp4WithMdat(exactContent);
		var storage = CreateStorageWithMp4("/exact-256kb.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/exact-256kb.mp4");

		// Assert
		Assert.IsNotNull(hash);
		Assert.AreEqual(26, hash.Length);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_UnknownAtomType_SkipsAndContinues()
	{
		// Arrange - MP4 with custom/unknown atom before mdat
		using var ms = new MemoryStream();

		// Write ftyp
		var ftypSize = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(ftypSize);
		}

		ms.Write(ftypSize, 0, 4);
		ms.Write("ftyp"u8.ToArray(), 0, 4);
		ms.Write("isom"u8.ToArray(), 0, 4);
		ms.Write(new byte[4], 0, 4);
		ms.Write("isom"u8.ToArray(), 0, 4);

		// Write unknown atom "xyzw"
		var unknownContent = new byte[50];
		var unknownSize = BitConverter.GetBytes(( uint ) ( 8 + unknownContent.Length ));
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(unknownSize);
		}

		ms.Write(unknownSize, 0, 4);
		ms.Write("xyzw"u8.ToArray(), 0, 4);
		ms.Write(unknownContent, 0, unknownContent.Length);

		// Write mdat
		var mdatContent = "Content after unknown atom"u8.ToArray();
		var mdatSize = BitConverter.GetBytes(( uint ) ( 8 + mdatContent.Length ));
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(mdatSize);
		}

		ms.Write(mdatSize, 0, 4);
		ms.Write("mdat"u8.ToArray(), 0, 4);
		ms.Write(mdatContent, 0, mdatContent.Length);

		var mp4Data = ms.ToArray();
		var storage = CreateStorageWithMp4("/unknown-atom.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/unknown-atom.mp4");

		// Assert
		Assert.IsNotNull(hash);
		Assert.AreEqual(26, hash.Length);
	}

	public TestContext TestContext { get; set; } = null!;
}
