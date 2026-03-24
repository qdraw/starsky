using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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

internal class UnseekableStream : Stream
{
    private readonly Stream _inner;
    public UnseekableStream(Stream inner) => _inner = inner;
    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => _inner.CanWrite;
    public override long Length => _inner.Length;
    public override long Position { get => _inner.Position; set => throw new NotSupportedException(); }
    public override void Flush() => _inner.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => _inner.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
    protected override void Dispose(bool disposing)
    {
        if ( disposing )
        {
            _inner.Dispose();
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
        Assert.IsFalse(results.Any(), "Expected no candidates when rangeLength < 4");
    }

    [TestMethod]
    public void ScanJpegsInRange_YieldsEmpty_When_SeekFails_DueToOffsetOutOfRange()
    {
        var bytes = new byte[10];
        using var ms = new MemoryStream(bytes);

        // rangeOffset beyond stream length should cause TrySeek to fail
        var results = TiffEmbeddedPreviewExtractor.ScanJpegsInRange(ms, 100u, 50u).ToList();
        Assert.IsFalse(results.Any(), "Expected no candidates when rangeOffset > stream length");
    }

    [TestMethod]
    public void ScanJpegsInRange_YieldsEmpty_When_StreamIsNotSeekable()
    {
        var bytes = new byte[64];
        using var inner = new MemoryStream(bytes);
        using var ms = new UnseekableStream(inner);

        var results = TiffEmbeddedPreviewExtractor.ScanJpegsInRange(ms, 0u, 64u).ToList();
        Assert.IsFalse(results.Any(), "Expected no candidates when stream is not seekable");
    }
}

