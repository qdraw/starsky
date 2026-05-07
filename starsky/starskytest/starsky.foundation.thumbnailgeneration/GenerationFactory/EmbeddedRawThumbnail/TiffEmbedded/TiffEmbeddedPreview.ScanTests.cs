using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbedded;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.
	TiffEmbedded;

[TestClass]
public class TiffEmbeddedPreviewScanTests
{
	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_BaselineJpeg_StartingWith_FF_D8_FF_C4()
	{
		var bytes = new byte[]
		{
			0x00,
			0xFF, 0xD8,
			0xFF, 0xC4, 0x00, 0x04, 0x00, 0x00,
			0xFF, 0xDB, 0x00, 0x04, 0x00, 0x00,
			0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11,
			0x00
		};
		using var ms = new MemoryStream(bytes);
		// offset points to 0xFF in the sequence (index 1)
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 1);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsTrue_For_LosslessSof3Jpeg()
	{
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xC4, 0x00, 0x04, 0x00, 0x00,
			0xFF, 0xC3, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11,
			0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_RegularJpegHeader()
	{
		var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00 };
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_NonJpegBytes()
	{
		var bytes = new byte[] { 0x11, 0x22, 0x33, 0x44 };
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_WhenOffsetOutOfRange()
	{
		var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xC4 };
		using var ms = new MemoryStream(bytes);
		// offset beyond length
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 10);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_When_NotEnoughBytes()
	{
		var bytes = new byte[] { 0xFF, 0xD8, 0xFF }; // only 3 bytes
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result);
	}
}

[TestClass]
public class TryBuildScanCandidateTests
{
	[TestMethod]
	public void TryBuildScanCandidate_ReturnsFalse_When_IsLosslessJpegAtOffset()
	{
		// Arrange: create a stream with padding, then a lossless JPEG header at soi
		using var ms = new MemoryStream();
		var padding = new byte[20];
		ms.Write(padding, 0, padding.Length);
		var soi = ( uint ) 10;
		ms.Seek(soi, SeekOrigin.Begin);
		ms.Write(new byte[]
			{
				0xFF, 0xD8,
				0xFF, 0xC4, 0x00, 0x04, 0x00, 0x00,
				0xFF, 0xC3, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01,
				0x11, 0x00
			}, 0, 21); // lossless SOF3 marker chain

		// resume position should be different from soi to ensure it's restored
		ms.Seek(5, SeekOrigin.Begin);

		// Act
		var ok = TiffEmbeddedPreviewExtractor.TryBuildScanCandidate(ms, soi, 1000,
			out var candidate);

		// Assert
		Assert.IsFalse(ok, "Expected TryBuildScanCandidate to return false for lossless JPEG");
		Assert.IsNull(candidate, "Candidate should be null for lossless JPEG");
		Assert.AreEqual(5, ms.Position, "Stream position should be restored to resumePosition");
	}

	[TestMethod]
	public void TryBuildScanCandidate_ReturnsFalse_When_LengthLessThanMinJpegSize()
	{
		// Arrange: create a small JPEG (length < MinJpegSize) starting at soi
		using var ms = new MemoryStream();
		var padding = new byte[20];
		ms.Write(padding, 0, padding.Length);
		var soi = ( uint ) 8;
		ms.Seek(soi, SeekOrigin.Begin);

		// Create a small JPEG: SOI (2 bytes) + 6 bytes data + EOI -> total length = 10
		var jpeg = new byte[] { 0xFF, 0xD8, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0xFF, 0xD9 };
		ms.Write(jpeg, 0, jpeg.Length);

		// set resume position to non-soi
		ms.Seek(3, SeekOrigin.Begin);

		// Act
		var ok = TiffEmbeddedPreviewExtractor.TryBuildScanCandidate(ms, soi, 5000,
			out var candidate);

		// Assert
		Assert.IsFalse(ok,
			"Expected TryBuildScanCandidate to return false when JPEG length < MinJpegSize");
		Assert.IsNull(candidate, "Candidate should be null when JPEG too small");
		Assert.AreEqual(3, ms.Position, "Stream position should be restored to resumePosition");
	}
}

internal sealed class UnseekableStream(Stream inner) : Stream
{
	public override bool CanRead => inner.CanRead;
	public override bool CanSeek => false;
	public override bool CanWrite => inner.CanWrite;
	public override long Length => inner.Length;

	public override long Position
	{
		get => inner.Position;
		set => throw new NotSupportedException();
	}

	public override void Flush()
	{
		inner.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return inner.Read(buffer, offset, count);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		inner.SetLength(value);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		inner.Write(buffer, offset, count);
	}

	protected override void Dispose(bool disposing)
	{
		if ( disposing )
		{
			inner.Dispose();
		}

		base.Dispose(disposing);
	}
}

// Additional tests for ScanJpegsInRange early exit conditions
[TestClass]
public class TiffEmbeddedPreviewScanEarlyExitTests
{
	[TestMethod]
	public void ScanJpegsInRange_YieldsEmpty_When_MaxScanLessThan4()
	{
		var bytes = new byte[10];
		using var ms = new MemoryStream(bytes);

		var results = TiffEmbeddedPreviewExtractor.ScanJpegsInRange(ms, 0, 3).ToList();
		Assert.IsEmpty(results, "Expected no candidates when rangeLength < 4");
	}

	[TestMethod]
	public void ScanJpegsInRange_YieldsEmpty_When_SeekFails_DueToOffsetOutOfRange()
	{
		var bytes = new byte[10];
		using var ms = new MemoryStream(bytes);

		// rangeOffset beyond stream length should cause TrySeek to fail
		var results = TiffEmbeddedPreviewExtractor.ScanJpegsInRange(ms, 100u, 50u).ToList();
		Assert.IsEmpty(results, "Expected no candidates when rangeOffset > stream length");
	}

	[TestMethod]
	public void ScanJpegsInRange_YieldsEmpty_When_StreamIsNotSeekable()
	{
		var bytes = new byte[64];
		using var inner = new MemoryStream(bytes);
		using var ms = new UnseekableStream(inner);

		var results = TiffEmbeddedPreviewExtractor.ScanJpegsInRange(ms, 0u, 64u).ToList();
		Assert.IsEmpty(results, "Expected no candidates when stream is not seekable");
	}
}

// Test edge cases and camera manufacturer-specific patterns
[TestClass]
public class TiffEmbeddedPreviewCameraQuirksTests
{
	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_ArithmeticEncodingSOF9()
	{
		// Arithmetic-encoded baseline from some NIKON/Canon cameras
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xC9, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11,
			0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result, "SOF9 (arithmetic baseline) should not be detected as lossless");
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_ProgressiveSOF2()
	{
		// Progressive JPEG (SOF2) - still lossy, just multi-pass
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xC2, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11,
			0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result, "SOF2 (progressive) should not be detected as lossless");
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsTrue_For_DifferentialLosslessSOF7()
	{
		// SOF7 - lossless with arithmetic (rare but valid)
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xC7, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11,
			0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsTrue(result, "SOF7 (lossless arithmetic) should be detected as lossless");
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_DifferentialBaselineSOF5()
	{
		// SOF5 - differential sequential DCT (non-standard variant)
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xC5, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11,
			0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result, "SOF5 (differential sequential) should not be detected as lossless");
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_MultipleConsecutiveAPPMarkers()
	{
		// Canon EOS typical pattern: APP0 + APP1 + APP13 + baseline SOF0
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xE0, 0x00, 0x04, 0x00, 0x00, // APP0
			0xFF, 0xE1, 0x00, 0x04, 0x00, 0x00, // APP1
			0xFF, 0xED, 0x00, 0x04, 0x00, 0x00, // APP13
			0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result, "SOF0 after multiple APP markers should not be detected as lossless");
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_ExtendedSequentialSOF1()
	{
		// SOF1 - extended sequential DCT (used by some cameras)
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xC1, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11,
			0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result, "SOF1 (extended sequential) should not be detected as lossless");
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_APP1BeforeSOF()
	{
		// EXIF APP1 marker before SOF (common in TIFF-embedded JPEG with thumbnails)
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xE1, 0x00, 0x10, 0x45, 0x78, 0x69, 0x66, 0x00, // APP1 + "Exif"
			0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result, "Baseline SOF0 after EXIF APP1 should not be detected as lossless");
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_DQTQuantizationTableBeforeSOF()
	{
		// DQT (quantization table) with data before SOF - common in high-quality JPEGs
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xDB, 0x00, 0x43, 0x00,
			0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
			0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13,
			0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D,
			0x1E, 0x1F, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27,
			0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31,
			0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B,
			0x3C, 0x3D, 0x3E, 0x3F,
			0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result, "Baseline SOF0 after DQT table should not be detected as lossless");
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_RestartMarkersBeforeSOF()
	{
		// RST markers (D0-D7) before SOF - some firmware uses these for data alignment
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xD0, 0xFF, 0xD1, 0xFF, 0xD2, 0xFF, 0xD3,
			0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result, "Baseline SOF0 after restart markers should not be detected as lossless");
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_CommentMarkerBeforeSOF()
	{
		// COM (comment) marker before SOF - metadata added by some Canon firmware
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xFE, 0x00, 0x08, 0x54, 0x65, 0x73, 0x74, // COM + "Test"
			0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result, "Baseline SOF0 after COM marker should not be detected as lossless");
	}

	[TestMethod]
	public void CombinedQuirks_ProgressiveWithMultipleAPPMarkers()
	{
		// Realistic pattern: Progressive SOF2 from Sony with multiple APP markers
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, // APP0 JFIF
			0xFF, 0xE1, 0x00, 0x10, 0x45, 0x78, 0x69, 0x66, 0x00, // APP1 EXIF
			0xFF, 0xC2, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00  // SOF2 (progressive)
		};
		using var ms = new MemoryStream(bytes);
		// Progressive JPEG should NOT be detected as lossless
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result, "Progressive JPEG (SOF2) with APP markers should not be lossless");
	}

	[TestMethod]
	public void CombinedQuirks_LosslessWithCommentAndDHT()
	{
		// Lossless JPEG (SOF3) with DHT and comment markers (rare but valid)
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xFE, 0x00, 0x08, 0x54, 0x45, 0x53, 0x54, // COM marker + "TEST"
			0xFF, 0xC4, 0x00, 0x04, 0x00, 0x00, // DHT
			0xFF, 0xC3, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00  // SOF3 (lossless)
		};
		using var ms = new MemoryStream(bytes);
		// Despite comment and DHT, SOF3 makes it lossless
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsTrue(result, "SOF3 (lossless) after comment and DHT should still be lossless");
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_ZeroLengthDHTSegment()
	{
		// DHT with zero length (malformed, from old firmware)
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xC4, 0x00, 0x00, // DHT: zero-length (invalid)
			0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result, "Baseline SOF0 after zero-length DHT should not be detected as lossless");
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsFalse_For_ArithmeticProgressiveSOF10()
	{
		// SOF10 - arithmetic-encoded progressive (rare in cameras)
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xCA, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsFalse(result, "SOF10 (arithmetic progressive) should not be detected as lossless");
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsTrue_For_HierarchicalArithmeticLosslessSOF11()
	{
		// SOF11 - hierarchical/lossless arithmetic (used in some Olympus/Panasonic)
		var bytes = new byte[]
		{
			0xFF, 0xD8,
			0xFF, 0xCB, 0x00, 0x0B, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00
		};
		using var ms = new MemoryStream(bytes);
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 0);
		Assert.IsTrue(result, "SOF11 (hierarchical lossless arithmetic) should be detected as lossless");
	}
}

