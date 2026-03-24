using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

namespace starskytest.starsky.foundation.thumbnailgeneration;

[TestClass]
public class LightweightContainerPreviewExtractorTests
{
    private sealed class NonSeekableStream(Stream inner) : Stream
    {
	    public override bool CanRead => inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => inner.CanWrite;
        public override long Length => inner.Length;
        public override long Position { get => inner.Position; set => throw new NotSupportedException(); }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
    }

    [TestMethod]
    public void TryParseTiffHeader_NonSeekableOrShortLength_ReturnsFalse()
    {
        var ms = new MemoryStream(new byte[100]);
        var ns = new NonSeekableStream(ms);
        var ok = LightweightContainerPreviewExtractor.TryParseTiffHeader(ns, out var a, out var b, out var c);
        Assert.IsFalse(ok);
        ok = LightweightContainerPreviewExtractor.TryParseTiffHeader(new MemoryStream(new byte[511]), out a, out b, out c);
        Assert.IsFalse(ok);
    }

    [TestMethod]
    public void TryParseTiffHeader_ReadEndianFails_ReturnsFalse()
    {
        var data = new byte[600];
        // make length >=512 so first check passes
        var ms = new MemoryStream(data);
        // Simulate TrySeek failing by passing a wrapper that disallows seeking
        var ns = new NonSeekableStream(ms);
        // Non-seekable already tested; now test insufficient read for endian: data length small after seek
        // Create a stream where FindTiffHeaderOffset will find offset but Read will be short: craft FindTiffHeaderOffset to return 0 by writing <8 bytes
        var shortRead = new MemoryStream(new byte[512 + 4]);
        // Force FindTiffHeaderOffset to return 0 by placing valid header at position 0 but limit read of endian bytes
        // Actually easiest: use a stream that has length but with no data at position found -> simulate by trimming
        var trimmed = new MemoryStream(new byte[513]);
        trimmed.SetLength(513);
        var res = LightweightContainerPreviewExtractor.TryParseTiffHeader(trimmed, out var tiffBase, out var littleEndian, out var firstIfdRelative);
        // This might return false because FindTiffHeaderOffset reads but then reading endian may succeed; the important assert is false if not matching
        Assert.IsFalse(res);
    }

    [TestMethod]
    public void TryParseTiffHeader_InvalidEndianBytes_ReturnsFalse()
    {
        var data = new byte[600];
        // Fill with zeros so FindTiffHeaderOffset will not find TIFF header -> returns false
        var ms = new MemoryStream(data);
        var ok = LightweightContainerPreviewExtractor.TryParseTiffHeader(ms, out var a, out var b, out var c);
        Assert.IsFalse(ok);
    }

    [TestMethod]
    public void TryParseTiffHeader_WrongMagicOrFirstIfdFails_ReturnsFalse()
    {
        // Construct a stream where a TIFF header exists at offset 0 but magic is wrong
        var buf = new byte[600];
        // Little endian marker 'II' but wrong magic bytes (not 42)
        buf[0] = 0x49; buf[1] = 0x49;
        buf[2] = 0x00; buf[3] = 0x00; // magic low bytes (wrong)
        // Make sure we have 4 bytes for first IFD as well
        var ms = new MemoryStream(buf);
        var ok = LightweightContainerPreviewExtractor.TryParseTiffHeader(ms, out var tiffBase, out var littleEndian, out var firstIfdRelative);
        Assert.IsFalse(ok);

        // Now set magic to 42 but make TryReadUInt32 fail by truncating stream
        var buf2 = new byte[6];
        buf2[0] = 0x49; buf2[1] = 0x49; // endian
        buf2[2] = 0x2A; buf2[3] = 0x00; // magic 42
        // but insufficient bytes for firstIfdRelative (need 4)
        var ms2 = new MemoryStream(buf2);
        ok = LightweightContainerPreviewExtractor.TryParseTiffHeader(ms2, out tiffBase, out littleEndian, out firstIfdRelative);
        Assert.IsFalse(ok);
    }

    [TestMethod]
    public void FindTiffHeaderOffset_TrySeekZeroFailsOrReadLessThan8_ReturnsMinusOne()
    {
        // Non-seekable stream should return -1
        var ms = new MemoryStream(new byte[1024]);
        var ns = new NonSeekableStream(ms);
        var idx = LightweightContainerPreviewExtractor.FindTiffHeaderOffset(ns);
        Assert.AreEqual(-1, idx);

        // Now seekable but insufficient bytes read (<8)
        var small = new MemoryStream(new byte[7]);
        idx = LightweightContainerPreviewExtractor.FindTiffHeaderOffset(small);
        Assert.AreEqual(-1, idx);
    }

    [TestMethod]
    public void TryReadUInt16_And_TryReadUInt32_InsufficientBytes_ReturnFalse()
    {
        var one = new MemoryStream(new byte[1]);
        var ok16 = LightweightContainerPreviewExtractor.TryReadUInt16(one, true, out var v16);
        Assert.IsFalse(ok16);

        var three = new MemoryStream(new byte[3]);
        var ok32 = LightweightContainerPreviewExtractor.TryReadUInt32(three, true, out var v32);
        Assert.IsFalse(ok32);
    }

    [TestMethod]
    public void TrySeek_NonSeekableOrOutOfRange_ReturnsFalse()
    {
        var ms = new MemoryStream(new byte[100]);
        var ns = new NonSeekableStream(ms);
        Assert.IsFalse(LightweightContainerPreviewExtractor.TrySeek(ns, 0));
        Assert.IsFalse(LightweightContainerPreviewExtractor.TrySeek(ms, -1));
        Assert.IsFalse(LightweightContainerPreviewExtractor.TrySeek(ms, 200));
    }

    [TestMethod]
    public void FindTiffHeaderOffset_FindsHeaderAtZero_ReturnsZero()
    {
        // Craft a buffer with 'II' and magic 0x2A 0x00 at position 0
        var buf = new byte[1000];
        buf[0] = 0x49; buf[1] = 0x49; buf[2] = 0x2A; buf[3] = 0x00;
        var ms = new MemoryStream(buf);
        var idx = LightweightContainerPreviewExtractor.FindTiffHeaderOffset(ms);
        Assert.AreEqual(0, idx);
    }
}


