using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

namespace starskytest.starsky.foundation.thumbnailgeneration.Services;

/// <summary>
///     Tests for EmbeddedPreviewExtractor behavior with oversized IFD entry counts.
///     Verifies that malformed RAW files with pathologically large IFD entry counts
///     are handled gracefully without hanging or causing OOM.
/// </summary>
[TestClass]
public class EmbeddedPreviewExtractorOversizedIfdTest
{
	/// <summary>
	///     Helper to create a TIFF file with a valid IFD that has the expected structure.
	/// </summary>
	private static byte[] CreateValidTiffWithJpeg()
	{
		using var ms = new MemoryStream();
		using var bw = new BinaryWriter(ms, Encoding.Default, leaveOpen: true);

		const int jpegSize = 10 * 1024; // 10 KB, well above 4 KB minimum

		// TIFF header (little-endian)
		bw.Write((byte)'I');
		bw.Write((byte)'I');
		bw.Write((ushort)42);
		bw.Write((uint)8); // First IFD offset

		// IFD with 2 tags: JPEG offset and JPEG length
		bw.Write((ushort)2); // 2 entries

		// TAG_JPEG_OFFSET (0x0201)
		bw.Write((ushort)0x0201);
		bw.Write((ushort)4); // Type LONG
		bw.Write((uint)1); // Count
		bw.Write((uint)100); // Offset to JPEG

		// TAG_JPEG_LENGTH (0x0202)
		bw.Write((ushort)0x0202);
		bw.Write((ushort)4); // Type LONG
		bw.Write((uint)1); // Count
		bw.Write((uint)jpegSize); // JPEG length

		// Next IFD
		bw.Write((uint)0);

		// Write minimal JPEG data at offset 100
		ms.Seek(100, SeekOrigin.Begin);
		bw.Write((byte)0xFF);
		bw.Write((byte)0xD8);
		for ( var i = 0; i < jpegSize - 4; i++ )
		{
			bw.Write((byte)0x00);
		}

		bw.Write((byte)0xFF);
		bw.Write((byte)0xD9);

		return ms.ToArray();
	}

	/// <summary>
	///     Helper to create a TIFF with an oversized IFD entry count at the start.
	///     The entry count field claims way more entries than actually follow.
	/// </summary>
	private static byte[] CreateTiffWithOversizedIfdCount(int fakeEntryCount = 23913)
	{
		using var ms = new MemoryStream();
		using var bw = new BinaryWriter(ms, Encoding.Default, leaveOpen: true);

		// TIFF header (little-endian)
		bw.Write((byte)'I');
		bw.Write((byte)'I');
		bw.Write((ushort)42);
		bw.Write((uint)8); // First IFD offset

		// First IFD with fake oversized entry count (but we write 0 actual entries)
		bw.Write(fakeEntryCount); // Claim this many entries (but we won't write them)

		// Write next IFD pointer immediately (skip entries)
		bw.Write((uint)0); // No next IFD

		return ms.ToArray();
	}


	[TestMethod]
	public void TryExtract_WithValidTiff_ReturnsTrue()
	{
		// Arrange
		using var ms = new MemoryStream(CreateValidTiffWithJpeg());
		ms.Seek(0, SeekOrigin.Begin);

		// Act
		var result = EmbeddedPreviewExtractor.TryExtract(ms, null, null);

		// Assert
		Assert.IsTrue(result, "Should successfully extract preview from valid TIFF with JPEG offset and length");
	}

	[TestMethod]
	public void TryExtract_WithOversizedIfdEntryCount_ReturnsFalseButDoesNotHang()
	{
		// Arrange
		using var ms = new MemoryStream(CreateTiffWithOversizedIfdCount(fakeEntryCount: 23913));

		// Act
		var startTime = DateTime.UtcNow;
		var result = EmbeddedPreviewExtractor.TryExtract(ms, null, null);
		var elapsed = DateTime.UtcNow - startTime;

		// Assert
		// Oversized entry count triggers sanity guard; extractor skips and returns false
		Assert.IsFalse(result, "Should gracefully handle oversized IFD without crashing");
		Assert.IsLessThan(1, elapsed.TotalSeconds, $"Should complete quickly (not hang); took {elapsed.TotalSeconds}s");
	}

	[TestMethod]
	public void TryExtract_WithExtremeOversizedCount_DoesNotHang()
	{
		// Arrange
		using var ms = new MemoryStream(CreateTiffWithOversizedIfdCount(100000));

		// Act
		var startTime = DateTime.UtcNow;
		var result = EmbeddedPreviewExtractor.TryExtract(ms, null, null);
		var elapsed = DateTime.UtcNow - startTime;

		// Assert
		Assert.IsFalse(result, "Should skip extreme entry count");
		Assert.IsLessThan(1, elapsed.TotalSeconds,
			$"Should complete instantly without hanging; took {elapsed.TotalSeconds}s");
	}

	[TestMethod]
	public void TryExtract_WithZeroEntryCount_ReturnsFalse()
	{
		// Arrange
		using var ms = new MemoryStream();
		using var bw = new BinaryWriter(ms, Encoding.Default, leaveOpen: true);

		// TIFF header
		bw.Write((byte)'I');
		bw.Write((byte)'I');
		bw.Write((ushort)42);
		bw.Write((uint)8);

		// IFD with 0 entries
		bw.Write((ushort)0); // 0 entries
		bw.Write((uint)0); // next IFD

		ms.Seek(0, SeekOrigin.Begin);

		// Act
		var result = EmbeddedPreviewExtractor.TryExtract(ms, null, null);

		// Assert
		// Empty IFD means no previews found
		Assert.IsFalse(result, "Should handle empty IFD gracefully");
	}
}

