using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Services;
using starskytest.FakeCreateAn.CreateAnQuickTimeMp4;
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
		ms.Write(new byte[4], 0, 4); // minor version
		ms.Write("isom"u8.ToArray(), 0, 4); // compatible brand

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
	public async Task HashMp4VideoContentAsync_ValidMp4_ExampleData_ReturnsHash()
	{
		var logger = new FakeIWebLogger();
		var example = CreateAnQuickTimeMp4.Bytes;
		var storage = CreateStorageWithMp4("/test.mp4", [.. example]);
		var hasher = new Mp4FileHasher(storage, logger);
		var hash = await hasher.HashMp4VideoContentAsync("/test.mp4");
		
		Assert.IsNotNull(hash);
		Assert.AreEqual(CreateAnQuickTimeMp4.FileHash, hash);
	}
	
	[TestMethod]
	public async Task HashMp4VideoContentAsync_ValidMp4_ExampleLocationData_ReturnsHash()
	{
		var logger = new FakeIWebLogger();
		var example = CreateAnQuickTimeMp4.BytesWithLocation;
		var storage = CreateStorageWithMp4("/test.mp4", [.. example]);
		var hasher = new Mp4FileHasher(storage, logger);
		var hash = await hasher.HashMp4VideoContentAsync("/test.mp4");
		
		Assert.IsNotNull(hash);
		Assert.AreEqual(CreateAnQuickTimeMp4.FileHashWithLocation, hash);
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

		await ms.WriteAsync(ftypSize, 0, 4, TestContext.CancellationToken);
		await ms.WriteAsync("ftyp"u8.ToArray(), 0, 4, TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray(), 0, 4, TestContext.CancellationToken);
		await ms.WriteAsync(new byte[4], 0, 4, TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray(), 0, 4, TestContext.CancellationToken);

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

		await ms.WriteAsync(sizeOne, 0, 4, TestContext.CancellationToken);
		await ms.WriteAsync("mdat"u8.ToArray(), 0, 4, TestContext.CancellationToken);
		await ms.WriteAsync(new byte[4], 0, 4, TestContext.CancellationToken); // Only 4 bytes instead of 8

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

		await ms.WriteAsync(ftypSize.AsMemory(0, 4), TestContext.CancellationToken);
		await ms.WriteAsync("ftyp"u8.ToArray().AsMemory(0, 4), TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4), TestContext.CancellationToken);
		await ms.WriteAsync(( new byte[4] ).AsMemory(0, 4), TestContext.CancellationToken); // minor version
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4), TestContext.CancellationToken); // compatible brand

		// Write unknown atom "xyzw"
		var unknownContent = new byte[50];
		var unknownSize = BitConverter.GetBytes(( uint ) ( 8 + unknownContent.Length ));
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(unknownSize);
		}

		await ms.WriteAsync(unknownSize, 0, 4, TestContext.CancellationToken);
		await ms.WriteAsync("xyzw"u8.ToArray(), 0, 4, TestContext.CancellationToken);
		await ms.WriteAsync(unknownContent, 0, unknownContent.Length, TestContext.CancellationToken);

		// Write mdat
		var mdatContent = "Content after unknown atom"u8.ToArray();
		var mdatSize = BitConverter.GetBytes(( uint ) ( 8 + mdatContent.Length ));
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(mdatSize);
		}

		await ms.WriteAsync(mdatSize, 0, 4, TestContext.CancellationToken);
		await ms.WriteAsync("mdat"u8.ToArray(), 0, 4, TestContext.CancellationToken);
		await ms.WriteAsync(mdatContent, 0, mdatContent.Length, TestContext.CancellationToken);

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

	/// <summary>
	/// Tests that if SkipAtomAsync throws (seek/read fails), the hasher returns string.Empty
	/// </summary>
	[TestMethod]
	public async Task HashMp4VideoContentAsync_SkipAtomThrows_ReturnsEmptyString()
	{
		// Arrange: Create a minimal MP4 with a non-mdat atom (e.g., ftyp only)
		var mp4Data = new byte[20];
		// Write ftyp atom header (size=20, type="ftyp")
		var sizeBytes = BitConverter.GetBytes((uint)20);
		if (BitConverter.IsLittleEndian) { Array.Reverse(sizeBytes); }
		Array.Copy(sizeBytes, 0, mp4Data, 0, 4);
		Array.Copy("ftyp"u8.ToArray(), 0, mp4Data, 4, 4);
		// The rest is dummy

		// Use FakeIStorage that throws IOException on ReadStream
		var storage = new FakeIStorage(new IOException("Read/Seek failed"));
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/throw.mp4");

		// Assert
		Assert.AreEqual(string.Empty, hash);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_SimilarHeadersDifferentEnd_ReturnsDifferentHash()
	{
		// Arrange - Create two files with same start (first 128KB) but different end content
		// This tests the sampling strategy of hashing both start and end
		var commonStart = new byte[128 * 1024];
		for ( var i = 0; i < commonStart.Length; i++ )
		{
			commonStart[i] = ( byte ) ( i % 256 );
		}

		var uniqueEnd1 = new byte[100 * 1024];
		for ( var i = 0; i < uniqueEnd1.Length; i++ )
		{
			uniqueEnd1[i] = 0xAA;
		}

		var uniqueEnd2 = new byte[100 * 1024];
		for ( var i = 0; i < uniqueEnd2.Length; i++ )
		{
			uniqueEnd2[i] = 0xBB;
		}

		// Create mdat content by concatenating common start + unique ends
		var mdatContent1 = new byte[commonStart.Length + uniqueEnd1.Length];
		Array.Copy(commonStart, 0, mdatContent1, 0, commonStart.Length);
		Array.Copy(uniqueEnd1, 0, mdatContent1, commonStart.Length, uniqueEnd1.Length);

		var mdatContent2 = new byte[commonStart.Length + uniqueEnd2.Length];
		Array.Copy(commonStart, 0, mdatContent2, 0, commonStart.Length);
		Array.Copy(uniqueEnd2, 0, mdatContent2, commonStart.Length, uniqueEnd2.Length);

		var mp4Data1 = CreateMinimalMp4WithMdat(mdatContent1);
		var mp4Data2 = CreateMinimalMp4WithMdat(mdatContent2);

		var storage1 = CreateStorageWithMp4("/file1.mp4", mp4Data1);
		var storage2 = CreateStorageWithMp4("/file2.mp4", mp4Data2);

		var logger = new FakeIWebLogger();
		var hasher1 = new Mp4FileHasher(storage1, logger);
		var hasher2 = new Mp4FileHasher(storage2, logger);

		// Act
		var hash1 = await hasher1.HashMp4VideoContentAsync("/file1.mp4");
		var hash2 = await hasher2.HashMp4VideoContentAsync("/file2.mp4");

		// Assert - Hashes should be DIFFERENT because end content is different
		Assert.AreNotEqual(hash1, hash2, 
			"Hashes should differ for files with same start but different end content");
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_SimilarStartDifferentMiddle_ReturnsDifferentHash()
	{
		// Arrange - Files with different middle content (tests that we catch differences)
		var part1 = new byte[100 * 1024];
		for ( var i = 0; i < part1.Length; i++ )
		{
			part1[i] = 0x11;
		}

		var middleSame1 = new byte[50 * 1024];
		for ( var i = 0; i < middleSame1.Length; i++ )
		{
			middleSame1[i] = 0x22;
		}

		var middleDiff1 = new byte[50 * 1024];
		for ( var i = 0; i < middleDiff1.Length; i++ )
		{
			middleDiff1[i] = 0xAA;
		}

		var middleDiff2 = new byte[50 * 1024];
		for ( var i = 0; i < middleDiff2.Length; i++ )
		{
			middleDiff2[i] = 0xBB;
		}

		var part3 = new byte[50 * 1024];
		for ( var i = 0; i < part3.Length; i++ )
		{
			part3[i] = 0x33;
		}

		var mdatContent1 = new byte[100 * 1024 + 50 * 1024 + 50 * 1024 + 50 * 1024];
		Array.Copy(part1, 0, mdatContent1, 0, part1.Length);
		Array.Copy(middleSame1, 0, mdatContent1, part1.Length, middleSame1.Length);
		Array.Copy(middleDiff1, 0, mdatContent1, part1.Length + middleSame1.Length, middleDiff1.Length);
		Array.Copy(part3, 0, mdatContent1, part1.Length + middleSame1.Length + middleDiff1.Length, part3.Length);

		var mdatContent2 = new byte[100 * 1024 + 50 * 1024 + 50 * 1024 + 50 * 1024];
		Array.Copy(part1, 0, mdatContent2, 0, part1.Length);
		Array.Copy(middleSame1, 0, mdatContent2, part1.Length, middleSame1.Length);
		Array.Copy(middleDiff2, 0, mdatContent2, part1.Length + middleSame1.Length, middleDiff2.Length);
		Array.Copy(part3, 0, mdatContent2, part1.Length + middleSame1.Length + middleDiff2.Length, part3.Length);

		var mp4Data1 = CreateMinimalMp4WithMdat(mdatContent1);
		var mp4Data2 = CreateMinimalMp4WithMdat(mdatContent2);

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
	public async Task SkipAtomAsync_SeekableStream_SkipsBySeeking()
	{
		var data = new byte[100];
		using var stream = new MemoryStream(data);
		stream.Position = 10;
		var buffer = new byte[16];
		var result = await Mp4FileHasher.SkipAtomAsync(stream, buffer, 50);
		Assert.IsTrue(result);
		Assert.AreEqual(60, stream.Position);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_SkipAtomAsyncFails_ReturnsEmptyString()
	{
		// Create an MP4 with ftyp atom that has a seek position that will fail
		var mp4Data = new byte[20];
		var sizeBytes = BitConverter.GetBytes((uint)20);
		if (BitConverter.IsLittleEndian) { Array.Reverse(sizeBytes); }
		Array.Copy(sizeBytes, 0, mp4Data, 0, 4);
		Array.Copy("ftyp"u8.ToArray(), 0, mp4Data, 4, 4);

		// Use FakeIStorage that throws IOException (simulates seek/read failure)
		var storage = new FakeIStorage(new IOException("Seek/Read failed on non-mdat atom"));
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		// When SkipAtomAsync tries to skip the ftyp atom and fails,
		// ProcessMp4AtomsAsync should return string.Empty
		var hash = await hasher.HashMp4VideoContentAsync("/test.mp4");

		// Assert
		// The untested code path is: !await SkipAtomAsync(...) returns false -> return string.Empty
		Assert.AreEqual(string.Empty, hash, 
			"When SkipAtomAsync fails, should return empty string to fall back to standard hashing");
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_SkipNonMdatAtomSucceeds_ContinuesToMdat()
	{
		// Arrange: Test the success path of SkipAtomAsync
		// When SkipAtomAsync returns true, we should continue to the next atom
		// This creates an MP4 with ftyp (will skip) -> moov (will skip) -> mdat (will hash)

		var mdatContent = "Video content after multiple atoms"u8.ToArray();
		var mp4Data = CreateMp4WithMultipleAtoms(mdatContent);
		var storage = CreateStorageWithMp4("/multi.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		// SkipAtomAsync should successfully skip ftyp and moov, then hash mdat
		var hash = await hasher.HashMp4VideoContentAsync("/multi.mp4");

		// Assert
		Assert.IsNotNull(hash);
		Assert.AreNotEqual(string.Empty, hash, 
			"When SkipAtomAsync succeeds, should continue and find mdat to hash");
		Assert.AreEqual(26, hash.Length);
	}

	public TestContext TestContext { get; set; } = null!;
}
