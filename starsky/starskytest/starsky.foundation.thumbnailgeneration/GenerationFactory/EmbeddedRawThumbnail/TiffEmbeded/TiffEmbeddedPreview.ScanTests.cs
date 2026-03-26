using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class TiffEmbeddedPreviewScanTests
{
	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsTrue_For_FF_D8_FF_C4()
	{
		var bytes = new byte[] { 0x00, 0xFF, 0xD8, 0xFF, 0xC4, 0x00 };
		using var ms = new MemoryStream(bytes);
		// offset points to 0xFF in the sequence (index 1)
		var result = TiffEmbeddedPreviewExtractor.IsLosslessJpegAtOffset(ms, 1);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsLosslessJpegAtOffset_ReturnsTrue_For_FF_D8_FF_C3()
	{
		var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xC3 };
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
		ms.Write(new byte[] { 0xFF, 0xD8, 0xFF, 0xC4 }, 0, 4); // lossless marker

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
