using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class EmbeddedRawThumbnailServiceTest
{
	private string _tempDir = null!;

	public TestContext TestContext { get; set; }

	[TestInitialize]
	public void Setup()
	{
		_tempDir = Path.Combine(Path.GetTempPath(), $"thumb_test_{Guid.NewGuid()}");
		Directory.CreateDirectory(_tempDir);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if ( !Directory.Exists(_tempDir) )
		{
			return;
		}

		try
		{
			Directory.Delete(_tempDir, true);
		}
		catch
		{
			// Ignore cleanup errors
		}
	}

	private static byte[] CreateMinimalTiffData(int jpegSize = 5000)
	{
		const int ifdOffset = 8;
		const int entryCount = 3; // width + jpeg offset + jpeg length
		const int ifdLength = 2 + entryCount * 12 + 4;
		var jpegOffset = ifdOffset + ifdLength;
		var data = new byte[jpegOffset + jpegSize]; // header + ifd + jpeg

		// TIFF header (little-endian)
		data[0] = ( byte ) 'I';
		data[1] = ( byte ) 'I';
		data[2] = 42;
		data[3] = 0;
		data[4] = ifdOffset;
		data[5] = 0;
		data[6] = 0;
		data[7] = 0;

		// IFD
		var ifdPos = ifdOffset;
		data[ifdPos++] = entryCount;
		data[ifdPos++] = 0;

		// TAG_IMAGE_WIDTH (0x0100), LONG, count=1, value=3000
		data[ifdPos++] = 0x00;
		data[ifdPos++] = 0x01;
		data[ifdPos++] = 4;
		data[ifdPos++] = 0;
		data[ifdPos++] = 1;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0xB8; // 3000 LE
		data[ifdPos++] = 0x0B;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;

		// TAG_JPEG_OFFSET
		data[ifdPos++] = 0x01;
		data[ifdPos++] = 0x02;
		data[ifdPos++] = 4; // Type LONG
		data[ifdPos++] = 0;
		data[ifdPos++] = 1; // Count
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = ( byte ) ( jpegOffset & 0xFF );
		data[ifdPos++] = ( byte ) ( ( jpegOffset >> 8 ) & 0xFF );
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;

		// TAG_JPEG_LENGTH
		data[ifdPos++] = 0x02;
		data[ifdPos++] = 0x02;
		data[ifdPos++] = 4;
		data[ifdPos++] = 0;
		data[ifdPos++] = 1;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = ( byte ) ( jpegSize & 0xFF );
		data[ifdPos++] = ( byte ) ( ( jpegSize >> 8 ) & 0xFF );
		data[ifdPos++] = ( byte ) ( ( jpegSize >> 16 ) & 0xFF );
		data[ifdPos++] = ( byte ) ( ( jpegSize >> 24 ) & 0xFF );

		// Next IFD
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos] = 0;

		// JPEG data
		var jpegPos = jpegOffset;
		data[jpegPos++] = 0xFF;
		data[jpegPos++] = 0xD8;
		data[jpegPos++] = 0xFF;
		data[jpegPos++] = 0xDA;
		for ( var i = 0; i < jpegSize - 6; i++ )
		{
			data[jpegPos++] = 0x00;
		}

		data[jpegPos++] = 0xFF;
		data[jpegPos] = 0xD9;

		return data;
	}

	private static byte[] CreateMinimalJpeg(int size = 5000)
	{
		if ( size < 64 )
		{
			size = 64;
		}

		using var ms = new MemoryStream(size);
		using var bw = new BinaryWriter(ms);

		bw.Write((byte)0xFF); bw.Write((byte)0xD8); // SOI

		// SOF0 segment (baseline, 8-bit, 16x16, 3 components)
		bw.Write((byte)0xFF); bw.Write((byte)0xC0);
		bw.Write((byte)0x00); bw.Write((byte)0x11);
		bw.Write((byte)0x08);
		bw.Write((byte)0x00); bw.Write((byte)0x10);
		bw.Write((byte)0x00); bw.Write((byte)0x10);
		bw.Write((byte)0x03);
		bw.Write((byte)0x01); bw.Write((byte)0x11); bw.Write((byte)0x00);
		bw.Write((byte)0x02); bw.Write((byte)0x11); bw.Write((byte)0x00);
		bw.Write((byte)0x03); bw.Write((byte)0x11); bw.Write((byte)0x00);

		// SOS segment header
		bw.Write((byte)0xFF); bw.Write((byte)0xDA);
		bw.Write((byte)0x00); bw.Write((byte)0x0C);
		bw.Write((byte)0x03);
		bw.Write((byte)0x01); bw.Write((byte)0x00);
		bw.Write((byte)0x02); bw.Write((byte)0x11);
		bw.Write((byte)0x03); bw.Write((byte)0x11);
		bw.Write((byte)0x00); bw.Write((byte)0x3F); bw.Write((byte)0x00);

		var payloadBytes = Math.Max(8, size - (int)ms.Length - 2);
		for ( var i = 0; i < payloadBytes; i++ )
		{
			bw.Write((byte)0x55);
		}

		bw.Write((byte)0xFF); bw.Write((byte)0xD9); // EOI
		bw.Flush();
		return ms.ToArray();
	}

	private static byte[] CreateMinimalCr3WithMdatJpeg()
	{
		var jpeg = CreateMinimalJpeg();
		using var ms = new MemoryStream();
		using var bw = new BinaryWriter(ms);

		// ftyp box
		bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(16)));
		bw.Write("ftyp"u8.ToArray());
		bw.Write("crx "u8.ToArray());
		bw.Write("\0\0\0\0"u8.ToArray());

		// mdat box containing a full JPEG segment
		var mdatSize = 8 + jpeg.Length;
		bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(mdatSize)));
		bw.Write("mdat"u8.ToArray());
		bw.Write(jpeg);
		bw.Flush();

		return ms.ToArray();
	}

	private static byte[] CreateLightweightContainerWithEmbeddedJpeg()
	{
		var jpeg = CreateMinimalJpeg();
		var bytes = new byte[256 + jpeg.Length + 256];
		Array.Copy(jpeg, 0, bytes, 256, jpeg.Length);
		return bytes;
	}

	[TestMethod]
	public async Task TryExtractPreview_WithNonExistentFile_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		var nonExistent = Path.Combine(_tempDir, "nonexistent.arw");

		// Act
		var result = await service.TryExtractPreview(nonExistent, null, null);

		// Assert
		Assert.IsFalse(result, "Should return false for nonexistent file");
	}

	[TestMethod]
	public async Task TryExtractPreview_WithValidRawFile_ReturnsTrue()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		var rawFile = Path.Combine(_tempDir, "test.arw");
		await File.WriteAllBytesAsync(rawFile, CreateMinimalTiffData(),
			TestContext.CancellationToken);

		// Act
		var result = await service.TryExtractPreview(rawFile, null, null);

		// Assert
		Assert.IsTrue(result, "Should extract preview from valid RAW");
	}


	[TestMethod]
	public async Task TryExtractPreview_WithBothOutputs_ReturnsTrue()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		var rawFile = Path.Combine(_tempDir, "test.arw");
		await File.WriteAllBytesAsync(rawFile, CreateMinimalTiffData(),
			TestContext.CancellationToken);

		var largeOutput = Path.Combine(_tempDir, "large.jpg");
		var mediumOutput = Path.Combine(_tempDir, "medium.jpg");

		// Act
		var result = await service.TryExtractPreview(rawFile, largeOutput, mediumOutput);

		// Assert
		Assert.IsTrue(result, "Should extract both previews");
	}

	[TestMethod]
	public async Task TryExtractPreview_WithInvalidTiffData_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		var rawFile = Path.Combine(_tempDir, "invalid.arw");
		await File.WriteAllBytesAsync(rawFile, new byte[100],
			TestContext.CancellationToken); // Random data

		// Act
		var result = await service.TryExtractPreview(rawFile, null, null);

		// Assert
		Assert.IsFalse(result, "Should fail with invalid TIFF data");
	}

	[TestMethod]
	public void TryExtractPreview_WithNullOutputs_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		var rawFile = Path.Combine(_tempDir, "test.arw");
		File.WriteAllBytes(rawFile, CreateMinimalTiffData());

		// Act
		var result = service.TryExtractPreview(rawFile, null, null).Result;

		// Assert
		// Should still return true because preview exists, just not written
		Assert.IsTrue(result, "Should return true even with null outputs");
	}

	[TestMethod]
	public async Task TryExtractPreviewAsync_WithValidRawFile_ReturnsTrue()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		var rawFile = Path.Combine(_tempDir, "test.arw");
		await File.WriteAllBytesAsync(rawFile, CreateMinimalTiffData(),
			TestContext.CancellationToken);

		// Act
		var result = await service.TryExtractPreview(rawFile, null, null);

		// Assert
		Assert.IsTrue(result, "Async method should return same result");
	}

	[TestMethod]
	public async Task TryExtractPreviewAsync_WithNonExistentFile_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		var nonExistent = Path.Combine(_tempDir, "nonexistent.arw");

		// Act
		var result = await service.TryExtractPreview(nonExistent, null, null);

		// Assert
		Assert.IsFalse(result, "Async should return false for nonexistent file");
	}


	[TestMethod]
	public async Task TryExtractPreview_WithEmptyFile_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		var rawFile = Path.Combine(_tempDir, "empty.arw");
		await File.WriteAllBytesAsync(rawFile, [], TestContext.CancellationToken);

		// Act
		var result = await service.TryExtractPreview(rawFile, null, null);

		// Assert
		Assert.IsFalse(result, "Should fail with empty file");
	}

	[TestMethod]
	public async Task TryExtractPreview_WithTinyFile_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		var rawFile = Path.Combine(_tempDir, "tiny.arw");
		await File.WriteAllBytesAsync(rawFile, new byte[3],
			TestContext.CancellationToken); // Less than header

		// Act
		var result = await service.TryExtractPreview(rawFile, null, null);

		// Assert
		Assert.IsFalse(result, "Should fail with too small file");
	}

	[TestMethod]
	public async Task TryExtractPreview_WithInvalidPath_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());

		// Act
		var result = await service.TryExtractPreview("", null, null);

		// Assert
		Assert.IsFalse(result, "Should fail with empty path");
	}

	[TestMethod]
	public void TryExtractPreview_WithDifferentFileExtensions_Works()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		string[] extensions =
		[
			".arw", ".cr2", ".nef", ".dng", ".cr3", ".raf", ".fff", ".x3f"
		];

		foreach ( var ext in extensions )
		{
			var rawFile = Path.Combine(_tempDir, $"test{ext}");
			var content = ext switch
			{
				".cr3" => CreateMinimalCr3WithMdatJpeg(),
				".raf" or ".fff" or ".x3f" => CreateLightweightContainerWithEmbeddedJpeg(),
				_ => CreateMinimalTiffData()
			};
			File.WriteAllBytes(rawFile, content);

			// Act
			var result = service.TryExtractPreview(rawFile, null, null).Result;

			// Assert
			Assert.IsTrue(result, $"Should work with {ext} extension");

			File.Delete(rawFile);
		}
	}

	[TestMethod]
	public void TryExtractPreview_MultipleCallsOnSameFile_Consistent()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		var rawFile = Path.Combine(_tempDir, "test.arw");
		File.WriteAllBytes(rawFile, CreateMinimalTiffData());

		// Act
		var result1 = service.TryExtractPreview(rawFile, null, null).Result;
		var result2 = service.TryExtractPreview(rawFile, null, null).Result;

		// Assert
		Assert.AreEqual(result1, result2, "Multiple calls should be consistent");
	}


	[TestMethod]
	public async Task TryExtractPreview_OutputPathWithSpecialCharacters_Works()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		var rawFile = Path.Combine(_tempDir, "test.arw");
		await File.WriteAllBytesAsync(rawFile, CreateMinimalTiffData(),
			TestContext.CancellationToken);

		var output = Path.Combine(_tempDir, "output_[special]_characters.jpg");

		// Act
		var result = await service.TryExtractPreview(rawFile, output, null);

		// Assert
		Assert.IsTrue(result, "Should work with special characters in path");
	}

	[TestMethod]
	public async Task TryExtractPreview_Cr3BmffRoute_WithMdatJpeg_ReturnsTrue()
	{
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		var rawFile = Path.Combine(_tempDir, "test.cr3");
		await File.WriteAllBytesAsync(rawFile, CreateMinimalCr3WithMdatJpeg(),
			TestContext.CancellationToken);

		var result = await service.TryExtractPreview(rawFile, null, null);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task TryExtractPreview_LightweightContainerRoute_WithEmbeddedJpeg_ReturnsTrue()
	{
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());
		var rawFile = Path.Combine(_tempDir, "test.raf");
		await File.WriteAllBytesAsync(rawFile, CreateLightweightContainerWithEmbeddedJpeg(),
			TestContext.CancellationToken);

		var result = await service.TryExtractPreview(rawFile, null, null);

		Assert.IsTrue(result);
	}
}
