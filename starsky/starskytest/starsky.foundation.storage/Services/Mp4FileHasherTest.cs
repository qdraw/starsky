using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Services;

[TestClass]
public sealed class Mp4FileHasherTest
{
	public TestContext TestContext { get; set; } = null!;

	private static FakeIStorage CreateStorageWithMp4(string path, byte[] mp4Data)
	{
		return new FakeIStorage(
			["/"],
			[path],
			new List<byte[]> { mp4Data }
		);
	}


	/// <summary>
	///     Creates an MP4 with multiple atoms before mdat
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

	/// <summary>
	///     Creates an MP4 with two mdat atoms in the specified order
	/// </summary>
	private static byte[] CreateMp4WithTwoMdats(byte[] firstMdat, byte[] secondMdat)
	{
		using var ms = new MemoryStream();

		// ftyp
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

		// first mdat
		var mdat1Size = BitConverter.GetBytes(( uint ) ( 8 + firstMdat.Length ));
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(mdat1Size);
		}

		ms.Write(mdat1Size, 0, 4);
		ms.Write("mdat"u8.ToArray(), 0, 4);
		ms.Write(firstMdat, 0, firstMdat.Length);

		// some small filler atom
		var freeSize = BitConverter.GetBytes(( uint ) 12);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(freeSize);
		}

		ms.Write(freeSize, 0, 4);
		ms.Write("free"u8.ToArray(), 0, 4);
		ms.Write(new byte[4], 0, 4);

		// second mdat
		var mdat2Size = BitConverter.GetBytes(( uint ) ( 8 + secondMdat.Length ));
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(mdat2Size);
		}

		ms.Write(mdat2Size, 0, 4);
		ms.Write("mdat"u8.ToArray(), 0, 4);
		ms.Write(secondMdat, 0, secondMdat.Length);

		return ms.ToArray();
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_ValidMp4WithMdat_ReturnsHash()
	{
		// Arrange
		var mdatContent = "This is test video content that should be hashed"u8.ToArray();
		var mp4Data = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(mdatContent);
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

		var mp4Data = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(largeContent);
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
		var mp4Data = CreateMinimalMp4WithMdatHelper.CreateMp4WithExtendedSize(mdatContent);
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

		await ms.WriteAsync(ftypSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("ftyp"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync(new byte[4].AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);

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

		await ms.WriteAsync(sizeOne.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("mdat"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync(new byte[4].AsMemory(0, 4),
			TestContext.CancellationToken); // Only 4 bytes instead of 8

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
		var mp4Data = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(smallContent);
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
		var mp4Data1 = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(content);
		var mp4Data2 = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(content);

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
		var mp4Data1 = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(content1);
		var mp4Data2 = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(content2);

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
	public async Task HashMp4VideoContentAsync_ZeroSizedMdatAtom_Invalid()
	{
		// Arrange - mdat with zero content
		var mp4Data = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat([]);
		var storage = CreateStorageWithMp4("/zero-mdat.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/zero-mdat.mp4");

		// Assert
		Assert.IsNotNull(hash);
		Assert.AreEqual(0, hash.Length);
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

		var mp4Data = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(binaryContent);
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

		var mp4Data = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(exactContent);
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

		await ms.WriteAsync(ftypSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("ftyp"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync(new byte[4].AsMemory(0, 4),
			TestContext.CancellationToken); // minor version
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken); // compatible brand

		// Write unknown atom "xyzw"
		var unknownContent = new byte[50];
		var unknownSize = BitConverter.GetBytes(( uint ) ( 8 + unknownContent.Length ));
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(unknownSize);
		}

		await ms.WriteAsync(unknownSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("xyzw"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync(unknownContent, TestContext.CancellationToken);

		// Write mdat
		var mdatContent = "Content after unknown atom"u8.ToArray();
		var mdatSize = BitConverter.GetBytes(( uint ) ( 8 + mdatContent.Length ));
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(mdatSize);
		}

		await ms.WriteAsync(mdatSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("mdat"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync(mdatContent, TestContext.CancellationToken);

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

	[TestMethod]
	public async Task HashMp4VideoContentAsync_MdatHeaderOnly_ReturnsEmptyString()
	{
		// Arrange: ftyp + mdat header only (size = 8, no payload)
		using var ms = new MemoryStream();

		var ftypSize = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(ftypSize);
		}

		await ms.WriteAsync(ftypSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("ftyp"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync(new byte[4].AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);

		// mdat header size = 8 (header only)
		var mdatSize = BitConverter.GetBytes(( uint ) 8);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(mdatSize);
		}

		await ms.WriteAsync(mdatSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("mdat"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);

		var mp4Data = ms.ToArray();
		var storage = CreateStorageWithMp4("/mdat-header-only.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/mdat-header-only.mp4");

		// Assert - nothing hashed, should return empty to force fallback
		Assert.AreEqual(string.Empty, hash);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_SizeZeroAtom_ExtendsToEOF_HashesPayload()
	{
		// Arrange: size==0 atom extends to EOF and contains payload
		using var ms = new MemoryStream();

		var ftypSize = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(ftypSize);
		}

		await ms.WriteAsync(ftypSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("ftyp"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync(new byte[4].AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);

		// mdat size = 0 -> extends to EOF
		var zeroSize = BitConverter.GetBytes(( uint ) 0);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(zeroSize);
		}

		await ms.WriteAsync(zeroSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("mdat"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		// payload
		var payload = "payload-data"u8.ToArray();
		await ms.WriteAsync(payload, TestContext.CancellationToken);

		var mp4Data = ms.ToArray();
		var storage = CreateStorageWithMp4("/size-zero.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/size-zero.mp4");

		// Assert - should hash payload and return a Base32 string
		Assert.IsNotNull(hash);
		Assert.AreEqual(26, hash.Length);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_SizeZeroNonSeekable_ReturnsEmptyString()
	{
		// Arrange: size==0 atom but stream is non-seekable -> ReadAtomAsync should return null
		using var ms = new MemoryStream();

		var ftypSize = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(ftypSize);
		}

		await ms.WriteAsync(ftypSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("ftyp"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync(new byte[4].AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);

		var zeroSize = BitConverter.GetBytes(( uint ) 0);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(zeroSize);
		}

		await ms.WriteAsync(zeroSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("mdat"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		// no payload

		var mp4Data = ms.ToArray();

		// Non-seekable stream class defined in this test file
		await using var nonSeek = new NonSeekableStream(mp4Data);

		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		using var md5 = MD5.Create();
		var sut = new Mp4FileHasher(storage, logger);

		// Act - call internal ProcessMp4AtomsAsync directly using non-seekable stream
		var hash =
			await sut.ProcessMp4AtomsAsync(nonSeek, md5, new byte[16],
				CancellationToken.None);

		// Assert - should return empty since ReadAtomAsync cannot determine atom size
		Assert.AreEqual(string.Empty, hash);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_NonSeekable_Mdat_HashesSameAsSeekable()
	{
		// Arrange - create an mp4 with a single mdat payload
		var mdatContent = "nonseek mdat payload"u8.ToArray();
		var mp4Data = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(mdatContent);

		// Seekable storage (reference)
		var storageSeek = CreateStorageWithMp4("/seek.mp4", mp4Data);
		var loggerSeek = new FakeIWebLogger();
		var hasherSeek = new Mp4FileHasher(storageSeek, loggerSeek);
		var hashSeek = await hasherSeek.HashMp4VideoContentAsync("/seek.mp4");

		// Non-seekable storage: wrap bytes in NonSeekableStream and return via
		// StreamReturningStorage
		await using var nonSeek = new NonSeekableStream(mp4Data);
		var storageNonSeek = new StreamReturningStorage(nonSeek);
		var loggerNonSeek = new FakeIWebLogger();
		var hasherNonSeek = new Mp4FileHasher(storageNonSeek, loggerNonSeek);
		var hashNonSeek = await hasherNonSeek.HashMp4VideoContentAsync("/seek.mp4");

		// Assert - non-seekable should hash the mdat immediately and match seekable hash
		Assert.IsNotNull(hashSeek);
		Assert.IsNotNull(hashNonSeek);
		Assert.AreEqual(hashSeek, hashNonSeek);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_NonSeekable_Mdat_NoPayload_ReturnsEmpty()
	{
		// Arrange: create MP4 with mdat header declaring payload but no payload bytes
		using var ms = new MemoryStream();
		var ftypSize = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(ftypSize);
		}

		await ms.WriteAsync(ftypSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("ftyp"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync(new byte[4].AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);

		// mdat header with declared payload size 50 but do not write payload
		var payloadLen = 50;
		var mdatSize = BitConverter.GetBytes(( uint ) ( 8 + payloadLen ));
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(mdatSize);
		}

		await ms.WriteAsync(mdatSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("mdat"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		// no payload written

		var mp4Data = ms.ToArray();

		await using var nonSeek = new NonSeekableStream(mp4Data);
		var storage = new StreamReturningStorage(nonSeek);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/no-payload.mp4");

		// Assert - HashMdatAtomAsync should read 0 bytes and return empty to indicate fallback
		Assert.AreEqual(string.Empty, hash);
	}

	[TestMethod]
	public async Task
		HashMp4VideoContentAsync_SeekThrowsNotSupported_FallbacksToRead_HashesSameAsSeekable()
	{
		// Arrange - create an mp4 with atoms before mdat so SkipAtomAsync will be invoked
		var mdatContent = "fallback read mdat"u8.ToArray();
		var mp4Data = CreateMp4WithMultipleAtoms(mdatContent);

		// Reference seekable storage
		var storageSeek = CreateStorageWithMp4("/seekref.mp4", mp4Data);
		var loggerSeek = new FakeIWebLogger();
		var hasherSeek = new Mp4FileHasher(storageSeek, loggerSeek);
		var hashSeek = await hasherSeek.HashMp4VideoContentAsync("/seekref.mp4");

		// Stream that reports CanSeek=true but whose Seek throws NotSupportedException
		using var inner = new MemoryStream(mp4Data);
		using var seekNotSupported = new SeekNotSupportedStream(inner);

		var storage = new StreamReturningStorage(seekNotSupported);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/seekref.mp4");

		// Assert - fallback-to-read should allow scanning and produce same hash
		Assert.IsNotNull(hashSeek);
		Assert.IsNotNull(hash);
		Assert.AreEqual(hashSeek, hash);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_MdatDeclaredTooLarge_Truncated_ReturnsEmptyString()
	{
		// Arrange: mdat claims very large size but file is truncated (no payload)
		using var ms = new MemoryStream();

		var ftypSize = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(ftypSize);
		}

		await ms.WriteAsync(ftypSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("ftyp"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync(new byte[4].AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);

		// huge declared size
		var hugeSize = ( uint ) 67108880; // large number
		var hugeSizeBytes = BitConverter.GetBytes(hugeSize);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(hugeSizeBytes);
		}

		await ms.WriteAsync(hugeSizeBytes.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("mdat"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		// no payload - stream truncated

		var mp4Data = ms.ToArray();
		var storage = CreateStorageWithMp4("/huge-declared.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/huge-declared.mp4");

		// Assert - since nothing hashed, should return empty
		Assert.AreEqual(string.Empty, hash);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_MultipleSmallMdats_OrderInvariant_LargestFirst()
	{
		// Arrange - create two small mdats where one is larger than the other
		var small1 = "aaa"u8.ToArray();
		var small2 = "bbbbbbbb"u8.ToArray(); // larger

		// File A: small1 then small2
		var mp4A = CreateMp4WithTwoMdats(small1, small2);
		// File B: small2 then small1
		var mp4B = CreateMp4WithTwoMdats(small2, small1);

		var mp4C = CreateMp4WithTwoMdats([], small1);

		var storageA = CreateStorageWithMp4("/a.mp4", mp4A);
		var storageB = CreateStorageWithMp4("/b.mp4", mp4B);
		var storageC = CreateStorageWithMp4("/c.mp4", mp4C);

		var logger = new FakeIWebLogger();
		var hasherA = new Mp4FileHasher(storageA, logger);
		var hasherB = new Mp4FileHasher(storageB, logger);
		var hasherC = new Mp4FileHasher(storageC, logger);

		// Act
		var hashA = await hasherA.HashMp4VideoContentAsync("/a.mp4");
		var hashB = await hasherB.HashMp4VideoContentAsync("/b.mp4");
		var hashC = await hasherC.HashMp4VideoContentAsync("/c.mp4");

		// Assert - since largest-first is used, both should produce same result
		Assert.IsNotNull(hashA);
		Assert.IsNotNull(hashB);
		Assert.HasCount(26, hashA);
		Assert.HasCount(26, hashB);
		Assert.HasCount(26, hashC);
		Assert.AreEqual(hashA, hashB);
		Assert.AreNotEqual(hashB, hashC);
	}

	/// <summary>
	///     Tests that if SkipAtomAsync throws (seek/read fails), the hasher returns string.Empty
	/// </summary>
	[TestMethod]
	public async Task HashMp4VideoContentAsync_SkipAtomThrows_ReturnsEmptyString()
	{
		// Arrange: Create a minimal MP4 with a non-mdat atom (e.g., ftyp only)
		var mp4Data = new byte[20];
		// Write ftyp atom header (size=20, type="ftyp")
		var sizeBytes = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(sizeBytes);
		}

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
		Array.Copy(commonStart, 0, mdatContent1,
			0, commonStart.Length);
		Array.Copy(uniqueEnd1, 0, mdatContent1,
			commonStart.Length, uniqueEnd1.Length);

		var mdatContent2 = new byte[commonStart.Length + uniqueEnd2.Length];
		Array.Copy(commonStart, 0, mdatContent2,
			0, commonStart.Length);
		Array.Copy(uniqueEnd2, 0, mdatContent2,
			commonStart.Length, uniqueEnd2.Length);

		var mp4Data1 = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(mdatContent1);
		var mp4Data2 = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(mdatContent2);

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
		Array.Copy(part1, 0, mdatContent1,
			0, part1.Length);
		Array.Copy(middleSame1, 0, mdatContent1,
			part1.Length, middleSame1.Length);
		Array.Copy(middleDiff1, 0, mdatContent1,
			part1.Length + middleSame1.Length,
			middleDiff1.Length);
		Array.Copy(part3, 0, mdatContent1,
			part1.Length + middleSame1.Length + middleDiff1.Length,
			part3.Length);

		var mdatContent2 = new byte[100 * 1024 + 50 * 1024 + 50 * 1024 + 50 * 1024];
		Array.Copy(part1, 0, mdatContent2,
			0, part1.Length);
		Array.Copy(middleSame1, 0, mdatContent2,
			part1.Length, middleSame1.Length);
		Array.Copy(middleDiff2, 0, mdatContent2,
			part1.Length + middleSame1.Length,
			middleDiff2.Length);
		Array.Copy(part3, 0, mdatContent2,
			part1.Length + middleSame1.Length + middleDiff2.Length,
			part3.Length);

		var mp4Data1 = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(mdatContent1);
		var mp4Data2 = CreateMinimalMp4WithMdatHelper.CreateMinimalMp4WithMdat(mdatContent2);

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
	public async Task SkipAtomAsync_NonSeekableStream_SkipsByReading()
	{
		var data = new byte[100];
		await using var stream = new NonSeekableStream(data);
		stream.Position = 10;
		var buffer = new byte[16];
		var result = await Mp4FileHasher.SkipAtomAsync(stream, buffer, 50);
		Assert.IsTrue(result);
		Assert.AreEqual(60, stream.Position);
	}

	[TestMethod]
	public async Task SkipAtomAsync_SeekThrows_ReturnsFalse()
	{
		var data = new byte[100];
		await using var stream = new ThrowingSeekStream(data);
		stream.Position = 10;
		var buffer = new byte[16];
		var result = await Mp4FileHasher.SkipAtomAsync(stream, buffer, 50);
		Assert.IsFalse(result);
	}

	/// <summary>
	///     Tests that when SkipAtomAsync fails (returns false),
	///     HashMp4VideoContentAsync returns empty string
	///     This covers the untested code path:
	/// if ( !await SkipAtomAsync(...) ) { return string.Empty; }
	/// </summary>
	[TestMethod]
	public async Task HashMp4VideoContentAsync_SkipAtomFails_ReturnsEmptyString()
	{
		// Arrange - Use FakeIStorage with exception to simulate stream failure
		// This will trigger the exception handler and return string.Empty
		var storageWithException = new FakeIStorage(new IOException("Stream operation failed"));
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storageWithException, logger);

		// Act - Try to hash a file, but storage will throw an exception
		var hash = await hasher.HashMp4VideoContentAsync("/failing.mp4");

		// Assert - Should return empty string when skip/stream operation fails
		Assert.AreEqual(string.Empty, hash);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_CorruptMp4WithSkipFailure_ReturnsEmptyString()
	{
		// Arrange - Create an MP4 with ftyp atom before mdat
		// Use a stream that will throw IOException when trying to skip the ftyp atom
		using var ms = new MemoryStream();

		// Write ftyp atom header (size=20, type="ftyp")
		var ftypSize = BitConverter.GetBytes(( uint ) 20);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(ftypSize);
		}

		await ms.WriteAsync(ftypSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("ftyp"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync(new byte[4].AsMemory(0, 4),
			TestContext.CancellationToken); // minor version
		await ms.WriteAsync("isom"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken); // compatible brand

		// Write mdat atom header (which would never be reached due to skip failure)
		var mdatSize = BitConverter.GetBytes(( uint ) 50);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(mdatSize);
		}

		await ms.WriteAsync(mdatSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("mdat"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("test content"u8.ToArray().AsMemory(0, 12),
			TestContext.CancellationToken);

		var mp4Data = ms.ToArray();

		// Create a stream that throws when trying to skip
		var throwingStream = new LimitedStream(mp4Data);

		// Setup logger to capture the LogInformation call
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();

		using var md5 = MD5.Create();

		var sut = new Mp4FileHasher(storage, logger);

		// Act - Call ProcessMp4AtomsAsync which will attempt to skip ftyp atom and fail
		var hash = await sut.ProcessMp4AtomsAsync(throwingStream, md5,
			new byte[16], CancellationToken.None);

		// Assert - Should return empty string when skip fails
		Assert.AreEqual(string.Empty, hash);
		Assert.IsTrue(logger.TrackedInformation[0].Item2
			?.Contains("Mp4FileHasher.ProcessNonSeekableStreamAsync_non_mdat Failed to skip atom"));
	}

	[TestMethod]
	public async Task ProcessSeekableStream_SkipAtomThrows_ReturnsEmptyString()
	{
		// Arrange - create MP4 with atoms before mdat so parser will try to skip a non-mdat atom
		var mdatContent = "seek-fail"u8.ToArray();
		var mp4Data = CreateMp4WithMultipleAtoms(mdatContent);
		await using var throwingStream = new ThrowingSeekStream(mp4Data);

		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		using var md5 = MD5.Create();

		var sut = new Mp4FileHasher(storage, logger);

		// Act - ProcessMp4AtomsAsync should take the seekable path and fail when
		// SkipAtomAsync triggers Seek exception
		var result =
			await sut.ProcessMp4AtomsAsync(throwingStream, md5, new byte[16],
				CancellationToken.None);

		// Assert
		Assert.AreEqual(string.Empty, result);
		Assert.Contains(t => t.Item2?.Contains("Failed to skip") == true,
			logger.TrackedInformation);
	}

	[TestMethod]
	public async Task ProcessSeekableStream_MdatSkipFails_ReturnsEmptyAndLogs()
	{
		// Arrange - create MP4 whose first atom is mdat so HandleMdatSeekableAsync will try to skip it
		var mdatContent = "mdat-skip-fail"u8.ToArray();
		using var ms = new MemoryStream();
		// mdat atom: 4-byte big-endian size, 4-byte type, then payload
		var mdatSize = BitConverter.GetBytes(( uint ) ( 8 + mdatContent.Length ));
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(mdatSize);
		}

		await ms.WriteAsync(mdatSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("mdat"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync(mdatContent, TestContext.CancellationToken);
		var mp4Data = ms.ToArray();
		await using var throwingStream = new ThrowingSeekStream(mp4Data);

		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		using var md5 = MD5.Create();

		var sut = new Mp4FileHasher(storage, logger);

		// Act - ProcessMp4AtomsAsync should attempt to skip the mdat payload and fail
		var result =
			await sut.ProcessMp4AtomsAsync(throwingStream, md5, new byte[16],
				CancellationToken.None);

		// Assert - should return empty and log the mdat-specific skip failure
		Assert.AreEqual(string.Empty, result);
		Assert.Contains(
			t => t.Item2?.Contains(
				"Mp4FileHasher.ProcessSeekableStreamAsync_mdat Failed to skip atom") == true,
			logger.TrackedInformation);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_LimitedNonSeekableStream_ReturnsEmptyString()
	{
		// Arrange - create MP4 with atoms before mdat so parser will try to skip a non-mdat atom
		var mdatContent = "nonseek-fail"u8.ToArray();
		var mp4Data = CreateMp4WithMultipleAtoms(mdatContent);

		// Use LimitedStream which is non-seekable and will throw during skip/read
		await using var failingStream = new LimitedStream(mp4Data);

		var storage = new StreamReturningStorage(failingStream);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/nonseek-fail.mp4");

		// Assert - when skipping fails on non-seekable stream we get empty result
		Assert.AreEqual(string.Empty, hash);
		Assert.Contains(
			t => t.Item2?.Contains("Failed to skip") == true, logger.TrackedInformation);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_InvalidPayloadSize_Seekable_LogsAndReturnsEmpty()
	{
		// Arrange - craft an atom with size smaller than header (size=4 < header 8) to force payloadSize < 0
		using var ms = new MemoryStream();
		var badSize = BitConverter.GetBytes(( uint ) 4);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(badSize);
		}

		await ms.WriteAsync(badSize.AsMemory(0, 4), TestContext.CancellationToken);
		await ms.WriteAsync("free"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken); // atom type
		var mp4Data = ms.ToArray();

		var storage = CreateStorageWithMp4("/bad-seekable.mp4", mp4Data);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/bad-seekable.mp4");

		// Assert
		Assert.AreEqual(string.Empty, hash);
		Assert.Contains(
			t => t.Item2?.Contains(
				"Mp4FileHasher.ProcessSeekableStreamAsync invalid payload size") == true,
			logger.TrackedInformation);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_InvalidPayloadSize_NonSeekable_LogsAndReturnsEmpty()
	{
		// Arrange - same malformed atom but use non-seekable stream
		using var ms = new MemoryStream();
		var badSize = BitConverter.GetBytes(( uint ) 4);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(badSize);
		}

		await ms.WriteAsync(badSize.AsMemory(0, 4),
			TestContext.CancellationToken);
		await ms.WriteAsync("free"u8.ToArray().AsMemory(0, 4),
			TestContext.CancellationToken);
		var mp4Data = ms.ToArray();

		await using var nonSeek = new NonSeekableStream(mp4Data);
		var storage = new StreamReturningStorage(nonSeek);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/bad-nonseek.mp4");

		// Assert
		Assert.AreEqual(string.Empty, hash);
		Assert.Contains(
			t => t.Item2?.Contains(
				"Mp4FileHasher.ProcessNonSeekableStreamAsync invalid payload size") == true,
			logger.TrackedInformation);
	}

	[TestMethod]
	public async Task HashMp4VideoContentAsync_NonSeekable_ZeroSizeNonMdat_LogsAndReturnsEmpty()
	{
		// Arrange - craft a non-mdat atom with size == header (8) and use a non-seekable stream
		using var ms = new MemoryStream();
		var size8 = BitConverter.GetBytes(( uint ) 8);
		if ( BitConverter.IsLittleEndian )
		{
			Array.Reverse(size8);
		}

		await ms.WriteAsync(size8.AsMemory(0, 4), 
			TestContext.CancellationToken);
		await ms.WriteAsync("free"u8.ToArray().AsMemory(0, 4), 
			TestContext.CancellationToken);
		var mp4Data = ms.ToArray();

		await using var nonSeek = new NonSeekableStream(mp4Data);
		var storage = new StreamReturningStorage(nonSeek);
		var logger = new FakeIWebLogger();
		var hasher = new Mp4FileHasher(storage, logger);

		// Act
		var hash = await hasher.HashMp4VideoContentAsync("/bad-zero-nonseek.mp4");

		// Assert
		Assert.AreEqual(string.Empty, hash);
		Assert.Contains(
			t => t.Item2?.Contains(
				     "Mp4FileHasher.ProcessNonSeekableStreamAsync invalid zero-size non-mdat atom") ==
			     true,
			logger.TrackedInformation);
	}

	private sealed class NonSeekableStream(byte[] buffer) : MemoryStream(buffer)
	{
		public override bool CanSeek => false;
	}

	private sealed class ThrowingSeekStream(byte[] buffer) : MemoryStream(buffer)
	{
		public override long Seek(long offset, SeekOrigin loc)
		{
			throw new IOException("Seek failed");
		}
	}

	/// <summary>
	///     Stream that reports CanSeek=true but throws NotSupportedException when Seek is used
	///     to force SkipAtomAsync to fallback to SkipByReadingAsync path.
	/// </summary>
	private sealed class SeekNotSupportedStream : Stream
	{
		private readonly Stream _inner;

		public SeekNotSupportedStream(Stream inner)
		{
			_inner = inner;
		}

		public override bool CanRead => _inner.CanRead;
		public override bool CanSeek => true; // report seekable so Seek branch is taken
		public override bool CanWrite => _inner.CanWrite;
		public override long Length => _inner.Length;

		public override long Position
		{
			get => _inner.Position;
			set => _inner.Position = value;
		}

		public override void Flush()
		{
			_inner.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _inner.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			// Simulate a stream that doesn't support relative seeks (Current),
			// but supports absolute seeks (Begin)
			return origin == SeekOrigin.Current
				? throw new NotSupportedException("Simulated NotSupported Seek for Current")
				: _inner.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_inner.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_inner.Write(buffer, offset, count);
		}

		public override ValueTask<int> ReadAsync(Memory<byte> buffer,
			CancellationToken cancellationToken = default)
		{
			return _inner.ReadAsync(buffer, cancellationToken);
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count,
			CancellationToken cancellationToken)
		{
			return _inner.ReadAsync(buffer, offset, count, cancellationToken);
		}

		protected override void Dispose(bool disposing)
		{
			if ( disposing )
			{
				_inner.Dispose();
			}

			base.Dispose(disposing);
		}
	}

	/// <summary>
	///     Stream that throws IOException when trying to seek, simulating a skip failure
	///     This forces SkipMp4AtomAsync to catch IOException and return false
	/// </summary>
	private sealed class LimitedStream(byte[] buffer) : MemoryStream(buffer)
	{
		private int _readCount;

		public override bool CanSeek => false;

		public override long Seek(long offset, SeekOrigin loc)
		{
			// Throw exception when trying to seek (used by skip operations)
			throw new IOException("Cannot seek on limited stream");
		}

		public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
			CancellationToken cancellationToken = default)
		{
			switch ( _readCount )
			{
				// First read: atom header (allowed)
				case 0:
					_readCount++;
					return await base.ReadAsync(buffer, cancellationToken);
				// Subsequent reads should fail when trying to read during skip
				case > 0 when Position < 20:
					throw new IOException("Cannot read during skip");
				default:
					return await base.ReadAsync(buffer, cancellationToken);
			}
		}
	}

	/// <summary>
	///     Simple IStorage implementation that returns a preconfigured stream for ReadStream.
	///     Only the members used by Mp4FileHasher are implemented; others are minimal stubs.
	/// </summary>
	private sealed class StreamReturningStorage : IStorage
	{
		private readonly Stream _stream;

		public StreamReturningStorage(Stream stream)
		{
			_stream = stream;
		}

		public bool ExistFile(string path)
		{
			return true;
		}

		public bool ExistFolder(string path)
		{
			return false;
		}

		public bool IsFolderEmpty(string path)
		{
			return false;
		}

		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile(string path)
		{
			return FolderOrFileModel.FolderOrFileTypeList.File;
		}

		public void FolderMove(string fromPath, string toPath)
		{
			throw new NotImplementedException();
		}

		public bool FileMove(string fromPath, string toPath)
		{
			throw new NotImplementedException();
		}

		public void FileCopy(string fromPath, string toPath)
		{
			throw new NotImplementedException();
		}

		public bool FileDelete(string path)
		{
			throw new NotImplementedException();
		}

		public bool CreateDirectory(string path)
		{
			throw new NotImplementedException();
		}

		public bool FolderDelete(string path)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> GetAllFilesInDirectory(string path)
		{
			return [];
		}

		public IEnumerable<string> GetAllFilesInDirectoryRecursive(string path)
		{
			return [];
		}

		public IEnumerable<string> GetDirectories(string path)
		{
			return [];
		}

		public IEnumerable<KeyValuePair<string, DateTime>> GetDirectoryRecursive(string path)
		{
			return [];
		}

		public Stream ReadStream(string path, int maxRead = -1)
		{
			if ( _stream.CanSeek )
			{
				_stream.Position = 0;
			}

			return _stream;
		}

		public bool WriteStreamOpenOrCreate(Stream stream, string path)
		{
			throw new NotImplementedException();
		}

		public bool WriteStream(Stream stream, string path)
		{
			throw new NotImplementedException();
		}

		public Task<bool> WriteStreamAsync(Stream stream, string path)
		{
			throw new NotImplementedException();
		}

		public StorageInfo Info(string path)
		{
			throw new NotImplementedException();
		}

		public bool IsFileReady(string path)
		{
			return true;
		}

		public IAsyncEnumerable<string> ReadLinesAsync(string path,
			CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
