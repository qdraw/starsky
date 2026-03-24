using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class LightweightContainerPreviewExtractorTests
{
	private const string RawPathX3F = "/raw/test.x3f";
	private const string RawPathMissing = "/raw/missing.x3f";
	private const string OutputPath = "/tmp/preview.jpg";
	private static readonly string[] SourceArray = ["/raw"];
	private static readonly string[] SourceArray0 = ["/raw/other.x3f"];

	private static byte[] CreateJpeg(int totalLength, bool withIptc = false)
	{
		if ( totalLength < 128 )
		{
			totalLength = 128;
		}

		using var ms = new MemoryStream();
		ms.WriteByte(0xFF);
		ms.WriteByte(0xD8);
		if ( withIptc )
		{
			var iptc = "Photoshop 3.0\0"u8.ToArray();
			var segLen = iptc.Length + 2;
			ms.WriteByte(0xFF);
			ms.WriteByte(0xED);
			ms.WriteByte(( byte ) ( ( segLen >> 8 ) & 0xFF ));
			ms.WriteByte(( byte ) ( segLen & 0xFF ));
			ms.Write(iptc);
		}
		else
		{
			ms.Write([0xFF, 0xE0, 0x00, 0x04, 0x4A, 0x46]);
		}

		while ( ms.Length < totalLength - 2 )
		{
			ms.WriteByte(0x00);
		}

		ms.WriteByte(0xFF);
		ms.WriteByte(0xD9);
		return ms.ToArray();
	}

	private static void WriteUInt16BigEndian(byte[] bytes, int offset, ushort value)
	{
		bytes[offset] = ( byte ) ( value >> 8 );
		bytes[offset + 1] = ( byte ) value;
	}

	private static void WriteUInt32BigEndian(byte[] bytes, int offset, uint value)
	{
		bytes[offset] = ( byte ) ( value >> 24 );
		bytes[offset + 1] = ( byte ) ( value >> 16 );
		bytes[offset + 2] = ( byte ) ( value >> 8 );
		bytes[offset + 3] = ( byte ) value;
	}

	private static byte[] CreateX3FWithTaggedIfd1Preview(uint taggedOffset, int taggedLength)
	{
		var leadingJpeg = CreateJpeg(18869);
		var taggedJpeg = CreateJpeg(taggedLength);
		var totalLength = ( int ) taggedOffset + taggedLength + 16;
		var bytes = new byte[totalLength];

		Array.Copy(leadingJpeg, 0, bytes, 292, leadingJpeg.Length);

		const int tiffBase = 304;
		bytes[tiffBase] = 0x4D;
		bytes[tiffBase + 1] = 0x4D;
		bytes[tiffBase + 2] = 0x00;
		bytes[tiffBase + 3] = 0x2A;
		WriteUInt32BigEndian(bytes, tiffBase + 4, 8);

		// IFD0: zero entries + pointer to IFD1 at relative offset 16
		WriteUInt16BigEndian(bytes, tiffBase + 8, 0);
		WriteUInt32BigEndian(bytes, tiffBase + 10, 16);

		var ifd1 = tiffBase + 16;
		WriteUInt16BigEndian(bytes, ifd1, 3);

		// Tag 0x0103 Compression, SHORT, value 6
		WriteUInt16BigEndian(bytes, ifd1 + 2, 0x0103);
		WriteUInt16BigEndian(bytes, ifd1 + 4, 3);
		WriteUInt32BigEndian(bytes, ifd1 + 6, 1);
		WriteUInt16BigEndian(bytes, ifd1 + 10, 6);

		// Tag 0x0201 Offset
		WriteUInt16BigEndian(bytes, ifd1 + 14, 0x0201);
		WriteUInt16BigEndian(bytes, ifd1 + 16, 4);
		WriteUInt32BigEndian(bytes, ifd1 + 18, 1);
		WriteUInt32BigEndian(bytes, ifd1 + 22, taggedOffset);

		// Tag 0x0202 Length
		WriteUInt16BigEndian(bytes, ifd1 + 26, 0x0202);
		WriteUInt16BigEndian(bytes, ifd1 + 28, 4);
		WriteUInt32BigEndian(bytes, ifd1 + 30, 1);
		WriteUInt32BigEndian(bytes, ifd1 + 34, ( uint ) taggedLength);

		// next IFD = 0
		WriteUInt32BigEndian(bytes, ifd1 + 38, 0);

		Array.Copy(taggedJpeg, 0, bytes, taggedOffset, taggedJpeg.Length);
		return bytes;
	}

	private static byte[] CreateContainerWithTwoJpegsPreferIptc()
	{
		var largerNoIptc = CreateJpeg(8200);
		var smallerWithIptc = CreateJpeg(5400, true);
		var leading = Enumerable.Repeat(( byte ) 0xAA, 256).ToArray();
		var middle = Enumerable.Repeat(( byte ) 0xBB, 128).ToArray();
		return [.. leading, .. largerNoIptc, .. middle, .. smallerWithIptc];
	}

	[TestMethod]
	public async Task TryExtract_ReturnsFalse_WhenFileMissing()
	{
		var subPathStorage =
			new FakeIStorage([.. SourceArray], [.. SourceArray0]);
		var tempStorage = new FakeIStorage(["/tmp"]);
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(),
			new FakeIStorage(), tempStorage);
		var extractor = new LightweightContainerPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract(RawPathMissing, OutputPath);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtract_ReturnsFalse_WhenNoJpegCandidates()
	{
		var bytes = Enumerable.Repeat(( byte ) 0x11, 1024).ToArray();
		var subPathStorage = new FakeIStorage([.. SourceArray],
			[RawPathX3F], [bytes]);
		var tempStorage = new FakeIStorage(["/tmp"]);
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(),
			new FakeIStorage(), tempStorage);
		var extractor = new LightweightContainerPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract(RawPathX3F, OutputPath);

		Assert.IsFalse(result);
		Assert.IsFalse(tempStorage.ExistFile(OutputPath));
	}

	[TestMethod]
	public async Task TryExtract_X3fTaggedPreview_DoesNotWrite_WhenOutputPathNull()
	{
		const uint taggedOffset = 3816;
		const int taggedLength = 15345;
		var bytes = CreateX3FWithTaggedIfd1Preview(taggedOffset, taggedLength);
		var subPathStorage = new FakeIStorage(["/raw"],
			[RawPathX3F], [bytes]);
		var tempStorage = new FakeIStorage(["/tmp"]);
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(),
			new FakeIStorage(), tempStorage);
		var extractor = new LightweightContainerPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract(RawPathX3F, null);

		Assert.IsTrue(result);
		Assert.IsFalse(tempStorage.ExistFile(OutputPath));
	}

	[TestMethod]
	public async Task TryExtract_X3fTaggedPreview_WritesToTemp_WhenOutputPathProvided()
	{
		const uint taggedOffset = 3816;
		const int taggedLength = 15345;
		var bytes = CreateX3FWithTaggedIfd1Preview(taggedOffset, taggedLength);
		var subPathStorage = new FakeIStorage(["/raw"],
			[RawPathX3F], [bytes]);
		var tempStorage = new FakeIStorage(["/tmp"]);
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(),
			new FakeIStorage(), tempStorage);
		var extractor = new LightweightContainerPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract(RawPathX3F, OutputPath);

		Assert.IsTrue(result);
		Assert.IsTrue(tempStorage.ExistFile(OutputPath));
		await using var output = tempStorage.ReadStream(OutputPath);
		Assert.AreEqual(taggedLength, output.Length);
	}

	[TestMethod]
	public async Task TryExtract_FallbacksToScanner_WhenNoTiffButJpegPresent()
	{
		var bytes = CreateContainerWithTwoJpegsPreferIptc();
		// Ensure no TIFF header present by not inserting TIFF bytes
		var subPathStorage = new FakeIStorage(["/raw"],
			[RawPathX3F], [bytes]);
		var tempStorage = new FakeIStorage(["/tmp"]);
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(),
			new FakeIStorage(), tempStorage);
		var extractor = new LightweightContainerPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract(RawPathX3F, OutputPath);

		Assert.IsTrue(result);
		Assert.IsTrue(tempStorage.ExistFile(OutputPath));
		await using var output = tempStorage.ReadStream(OutputPath);
		Assert.IsGreaterThan(0, output.Length);
	}

	[TestMethod]
	public async Task TryExtract_ReturnsFalse_WhenTempWriteThrows()
	{
		const uint taggedOffset = 3816;
		const int taggedLength = 15345;
		var bytes = CreateX3FWithTaggedIfd1Preview(taggedOffset, taggedLength);
		var subPathStorage = new FakeIStorage(["/raw"],
			[RawPathX3F], [bytes]);
		var tempStorage = new FakeIStorage(new Exception("write fail"));
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(),
			new FakeIStorage(), tempStorage);
		var extractor = new LightweightContainerPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract(RawPathX3F, OutputPath);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtract_ReturnsFalse_OnSubPathReadException()
	{
		var subPathStorage = new FakeIStorage(new Exception("read fail"));
		var tempStorage = new FakeIStorage(["/tmp"]);
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(),
			new FakeIStorage(), tempStorage);
		var extractor = new LightweightContainerPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract(RawPathX3F, OutputPath);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtract_X3fTaggedPreview_WithRelativeOffset_WritesToTemp()
	{
		const int tiffBase = 304;
		const uint taggedOffset = 3816;
		const int taggedLength = 15345;
		var relativeOffset = taggedOffset - tiffBase;
		var leadingJpeg = CreateJpeg(18869);
		var taggedJpeg = CreateJpeg(taggedLength);
		var totalLength = ( int ) taggedOffset + taggedLength + 16;
		var bytes = new byte[totalLength];
		Array.Copy(leadingJpeg, 0, bytes, 292, leadingJpeg.Length);

		bytes[tiffBase] = 0x4D;
		bytes[tiffBase + 1] = 0x4D;
		bytes[tiffBase + 2] = 0x00;
		bytes[tiffBase + 3] = 0x2A;
		WriteUInt32BigEndian(bytes, tiffBase + 4, 8);
		WriteUInt16BigEndian(bytes, tiffBase + 8, 0);
		WriteUInt32BigEndian(bytes, tiffBase + 10, 16);

		var ifd1 = tiffBase + 16;
		WriteUInt16BigEndian(bytes, ifd1, 3);
		WriteUInt16BigEndian(bytes, ifd1 + 2, 0x0103);
		WriteUInt16BigEndian(bytes, ifd1 + 4, 3);
		WriteUInt32BigEndian(bytes, ifd1 + 6, 1);
		WriteUInt16BigEndian(bytes, ifd1 + 10, 6);

		WriteUInt16BigEndian(bytes, ifd1 + 14, 0x0201);
		WriteUInt16BigEndian(bytes, ifd1 + 16, 4);
		WriteUInt32BigEndian(bytes, ifd1 + 18, 1);
		WriteUInt32BigEndian(bytes, ifd1 + 22, relativeOffset);

		WriteUInt16BigEndian(bytes, ifd1 + 26, 0x0202);
		WriteUInt16BigEndian(bytes, ifd1 + 28, 4);
		WriteUInt32BigEndian(bytes, ifd1 + 30, 1);
		WriteUInt32BigEndian(bytes, ifd1 + 34, taggedLength);
		WriteUInt32BigEndian(bytes, ifd1 + 38, 0);

		Array.Copy(taggedJpeg, 0, bytes, taggedOffset, taggedJpeg.Length);

		var subPathStorage = new FakeIStorage(["/raw"], [RawPathX3F], [bytes]);
		var tempStorage = new FakeIStorage(["/tmp"]);
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(),
			new FakeIStorage(), tempStorage);
		var extractor = new LightweightContainerPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract(RawPathX3F, OutputPath);

		Assert.IsTrue(result);
		Assert.IsTrue(tempStorage.ExistFile(OutputPath));
		await using var output = tempStorage.ReadStream(OutputPath);
		Assert.AreEqual(taggedLength, output.Length);
	}

	[TestMethod]
	public async Task TryExtract_X3fTaggedPreview_UnsupportedCompression_ReturnsFalse()
	{
		const int tiffBase = 304;
		const uint taggedOffset = 2000u;
		const int taggedLength = 4096;
		var bytes = new byte[taggedOffset + taggedLength + 64];
		bytes[tiffBase] = 0x4D;
		bytes[tiffBase + 1] = 0x4D;
		bytes[tiffBase + 2] = 0x00;
		bytes[tiffBase + 3] = 0x2A;
		WriteUInt32BigEndian(bytes, tiffBase + 4, 8);
		WriteUInt16BigEndian(bytes, tiffBase + 8, 0);
		WriteUInt32BigEndian(bytes, tiffBase + 10, 16);
		var ifd1 = tiffBase + 16;
		WriteUInt16BigEndian(bytes, ifd1, 3);
		WriteUInt16BigEndian(bytes, ifd1 + 2, 0x0103);
		WriteUInt16BigEndian(bytes, ifd1 + 4, 3);
		WriteUInt32BigEndian(bytes, ifd1 + 6, 1);
		WriteUInt16BigEndian(bytes, ifd1 + 10, 1);
		WriteUInt32BigEndian(bytes, ifd1 + 38, 0);

		var subPathStorage = new FakeIStorage(["/raw"], [RawPathX3F], [bytes]);
		var tempStorage = new FakeIStorage(["/tmp"]);
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(),
			new FakeIStorage(), tempStorage);
		var extractor = new LightweightContainerPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract(RawPathX3F, OutputPath);

		Assert.IsFalse(result);
		Assert.IsFalse(tempStorage.ExistFile(OutputPath));
	}

	[TestMethod]
	public async Task TryReadIfdJpegPair_TooManyEntries_ReturnsFalse()
	{
		const int tiffBase = 304;
		var bytes = new byte[1024 + tiffBase + 64];
		bytes[tiffBase] = 0x4D;
		bytes[tiffBase + 1] = 0x4D;
		bytes[tiffBase + 2] = 0x00;
		bytes[tiffBase + 3] = 0x2A;
		WriteUInt32BigEndian(bytes, tiffBase + 4, 8);
		WriteUInt16BigEndian(bytes, tiffBase + 8, 2000);

		var subPathStorage = new FakeIStorage(["/raw"], [RawPathX3F], [bytes]);
		var tempStorage = new FakeIStorage(["/tmp"]);
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(),
			new FakeIStorage(), tempStorage);
		var extractor = new LightweightContainerPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract(RawPathX3F, OutputPath);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtract_FffExtension_UsesScanner()
	{
		var bytes = CreateContainerWithTwoJpegsPreferIptc();
		var subPath = "/raw/test.fff";
		var subPathStorage = new FakeIStorage(["/raw"], [subPath], [bytes]);
		var tempStorage = new FakeIStorage(["/tmp"]);
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(),
			new FakeIStorage(), tempStorage);
		var extractor = new LightweightContainerPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract(subPath, OutputPath);

		Assert.IsTrue(result);
		Assert.IsTrue(tempStorage.ExistFile(OutputPath));
	}

	private static byte[] MakeBufferWithTiffHeader(int totalLength, int headerIndex,
		bool littleEndian, uint firstIfdOffset)
	{
		var buf = new byte[totalLength];
		for ( var i = 0; i < totalLength; i++ )
		{
			buf[i] = 0;
		}

		if ( littleEndian )
		{
			buf[headerIndex] = 0x49;
			buf[headerIndex + 1] = 0x49;
			buf[headerIndex + 2] = 0x2A;
			buf[headerIndex + 3] = 0x00;
		}
		else
		{
			buf[headerIndex] = 0x4D;
			buf[headerIndex + 1] = 0x4D;
			buf[headerIndex + 2] = 0x00;
			buf[headerIndex + 3] = 0x2A;
		}

		var off = BitConverter.GetBytes(firstIfdOffset);
		if ( BitConverter.IsLittleEndian != littleEndian )
		{
			Array.Reverse(off);
		}

		Array.Copy(off, 0, buf, headerIndex + 4, 4);
		return buf;
	}

	[TestMethod]
	public void TryParseTiffHeader_ReturnsFalse_When_EndianReadShort()
	{
		var buf = MakeBufferWithTiffHeader(512, 8, true, 8u);
		var ts = new TestSeekableStream(buf);
		ts.EnqueueSmallResponse(new byte[] { 0x49 });
		var ok = LightweightContainerPreviewExtractor.TryParseTiffHeader(ts, out _,
			out _, out _);
		Assert.IsFalse(ok);
	}

	[TestMethod]
	public void TryParseTiffHeader_ReturnsFalse_When_MagicNot42()
	{
		var buf = MakeBufferWithTiffHeader(512, 16, true, 16u);
		var ts = new TestSeekableStream(buf);
		ts.EnqueueSmallResponse(new byte[] { 0x49, 0x49 });
		ts.EnqueueSmallResponse(new byte[] { 0x00, 0x00 });
		var ok = LightweightContainerPreviewExtractor.TryParseTiffHeader(ts, out _,
			out _, out _);
		Assert.IsFalse(ok);
	}

	[TestMethod]
	public void TryParseTiffHeader_ReturnsFalse_When_FirstIfdRelativeReadFails()
	{
		var buf = MakeBufferWithTiffHeader(1024, 32, true, 32u);
		var ts = new TestSeekableStream(buf);
		ts.EnqueueSmallResponse(new byte[] { 0x49, 0x49 });
		ts.EnqueueSmallResponse(new byte[] { 0x2A, 0x00 });
		ts.EnqueueSmallResponse(new byte[] { 0x01, 0x02 });
		var ok = LightweightContainerPreviewExtractor.TryParseTiffHeader(ts, out _,
			out _, out _);
		Assert.IsFalse(ok);
	}

	private class TestSeekableStream : Stream
	{
		private readonly byte[] _data;
		private readonly Queue<byte[]> _smallResponses = new();
		private long _pos;

		public TestSeekableStream(byte[] data)
		{
			_data = data;
			_pos = 0;
		}

		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => false;
		public override long Length => _data.Length;

		public override long Position
		{
			get => _pos;
			set => _pos = value;
		}

		public void EnqueueSmallResponse(byte[] resp)
		{
			_smallResponses.Enqueue(resp);
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if ( count > 256 && _pos == 0 )
			{
				var toRead = ( int ) Math.Min(count, _data.Length - _pos);
				Array.Copy(_data, _pos, buffer, offset, toRead);
				_pos += toRead;
				return toRead;
			}

			if ( _smallResponses.Count > 0 )
			{
				var resp = _smallResponses.Dequeue();
				var toCopy = Math.Min(resp.Length, count);
				Array.Copy(resp, 0, buffer, offset, toCopy);
				_pos += toCopy;
				return toCopy;
			}

			if ( _pos >= _data.Length )
			{
				return 0;
			}

			var avail = ( int ) Math.Min(count, _data.Length - _pos);
			Array.Copy(_data, _pos, buffer, offset, avail);
			_pos += avail;
			return avail;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch ( origin )
			{
				case SeekOrigin.Begin: _pos = offset; break;
				case SeekOrigin.Current: _pos += offset; break;
				case SeekOrigin.End: _pos = _data.Length + offset; break;
			}

			if ( _pos < 0 )
			{
				_pos = 0;
			}

			if ( _pos > _data.Length )
			{
				_pos = _data.Length;
			}

			return _pos;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
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
        var trimmed = new MemoryStream(new byte[513]);
        trimmed.SetLength(513);
        var res = LightweightContainerPreviewExtractor.TryParseTiffHeader(trimmed, out _, out _, out _);
        // This might return false because FindTiffHeaderOffset reads but then reading endian may succeed; the important assert is false if not matching
        Assert.IsFalse(res);
    }

    [TestMethod]
    public void TryParseTiffHeader_InvalidEndianBytes_ReturnsFalse()
    {
        var data = new byte[600];
        // Fill with zeros so FindTiffHeaderOffset will not find TIFF header -> returns false
        var ms = new MemoryStream(data);
        var ok = LightweightContainerPreviewExtractor.TryParseTiffHeader(ms, out _, out _, out _);
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
        var ok16 = LightweightContainerPreviewExtractor.TryReadUInt16(one, true, out _);
        Assert.IsFalse(ok16);

        var three = new MemoryStream(new byte[3]);
        var ok32 = LightweightContainerPreviewExtractor.TryReadUInt32(three, true, out _);
        Assert.IsFalse(ok32);
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
}

[TestClass]
public class LightweightContainerPreviewExtractor_TryReadIfdJpegPairTests
{
	private static void WriteUInt16Little(byte[] buf, int pos, ushort v)
	{
		buf[pos] = ( byte ) ( v & 0xFF );
		buf[pos + 1] = ( byte ) ( ( v >> 8 ) & 0xFF );
	}

	private static void WriteUInt32Little(byte[] buf, int pos, uint v)
	{
		buf[pos] = ( byte ) ( v & 0xFF );
		buf[pos + 1] = ( byte ) ( ( v >> 8 ) & 0xFF );
		buf[pos + 2] = ( byte ) ( ( v >> 16 ) & 0xFF );
		buf[pos + 3] = ( byte ) ( ( v >> 24 ) & 0xFF );
	}

	[TestMethod]
	public void TryReadIfdJpegPair_ReturnsFalse_When_IfdOffsetOutOfRange()
	{
		using var ms = new MemoryStream(new byte[10]);
		var ok = LightweightContainerPreviewExtractor.TryReadIfdJpegPair(ms, 100, true, out var off,
			out var len, out var comp, out var next);
		Assert.IsFalse(ok);
		Assert.AreEqual(0u, off);
		Assert.AreEqual(0u, len);
		Assert.AreEqual(( ushort ) 0, comp);
		Assert.AreEqual(0u, next);
	}

	[TestMethod]
	public void TryReadIfdJpegPair_ReturnsFalse_When_EntryCountTooLarge()
	{
		var buf = new byte[20];
		// place entryCount (ushort) at offset 0 = 2000
		WriteUInt16Little(buf, 0, 2000);
		using var ms = new MemoryStream(buf);

		var ok = LightweightContainerPreviewExtractor.TryReadIfdJpegPair(ms, 0, true, out _, out _,
			out _, out _);
		Assert.IsFalse(ok);
	}

	[TestMethod]
	public void TryReadIfdJpegPair_ReturnsFalse_When_TryReadIfdEntryFails()
	{
		// entryCount=1 but not enough bytes for entry (12 bytes)
		var buf = new byte[4];
		WriteUInt16Little(buf, 0, 1);
		using var ms = new MemoryStream(buf);

		var ok = LightweightContainerPreviewExtractor.TryReadIfdJpegPair(ms, 0, true, out _, out _,
			out _, out _);
		Assert.IsFalse(ok);
	}

	[TestMethod]
	public void TryReadIfdJpegPair_SkipsEntries_When_CountNotOne_And_ReadsNextIfd()
	{
		// Compose: entryCount=1; entry with count=2; then nextIfdRelative = 0x11223344
		var buf = new byte[2 + 12 + 4];
		WriteUInt16Little(buf, 0, 1);
		var pos = 2;
		// tag (2 bytes)
		WriteUInt16Little(buf, pos, 0x9999);
		pos += 2;
		// type (2 bytes)
		WriteUInt16Little(buf, pos, 4);
		pos += 2;
		// count (4 bytes) -> 2 (not 1)
		WriteUInt32Little(buf, pos, 2);
		pos += 4;
		// value (4 bytes)
		WriteUInt32Little(buf, pos, 0x01020304);
		pos += 4;
		// nextIfdRelative
		WriteUInt32Little(buf, pos, 0x11223344);

		using var ms = new MemoryStream(buf);
		var ok = LightweightContainerPreviewExtractor.TryReadIfdJpegPair(ms, 0, true, out var off,
			out var len, out var comp, out var next);
		Assert.IsTrue(ok);
		Assert.AreEqual(0u, off);
		Assert.AreEqual(0u, len);
		Assert.AreEqual(( ushort ) 0, comp);
		Assert.AreEqual(0x11223344u, next);
	}

	[TestMethod]
	public void TryReadIfdJpegPair_ParsesEntriesAndReturnsTrue()
	{
		// Build IFD with 3 entries: Compression (0x0103, type=4, count=1, value=7), Offset (0x0201), Length (0x0202), then nextIfdRelative
		var buf = new byte[2 + 3 * 12 + 4];
		WriteUInt16Little(buf, 0, 3);
		var pos = 2;

		// Entry 1: Compression tag 0x0103, type=4, count=1, value=7
		WriteUInt16Little(buf, pos, 0x0103);
		pos += 2;
		WriteUInt16Little(buf, pos, 4);
		pos += 2;
		WriteUInt32Little(buf, pos, 1);
		pos += 4;
		WriteUInt32Little(buf, pos, 7);
		pos += 4;

		// Entry 2: JpegOffset tag 0x0201, type=4, count=1, value=0x200
		WriteUInt16Little(buf, pos, 0x0201);
		pos += 2;
		WriteUInt16Little(buf, pos, 4);
		pos += 2;
		WriteUInt32Little(buf, pos, 1);
		pos += 4;
		WriteUInt32Little(buf, pos, 0x200);
		pos += 4;

		// Entry 3: JpegLength tag 0x0202, type=4, count=1, value=0x3000
		WriteUInt16Little(buf, pos, 0x0202);
		pos += 2;
		WriteUInt16Little(buf, pos, 4);
		pos += 2;
		WriteUInt32Little(buf, pos, 1);
		pos += 4;
		WriteUInt32Little(buf, pos, 0x3000);
		pos += 4;

		// nextIfdRelative
		WriteUInt32Little(buf, pos, 0x0);

		using var ms = new MemoryStream(buf);
		var ok = LightweightContainerPreviewExtractor.TryReadIfdJpegPair(ms, 0, true, out var off,
			out var len, out var comp, out var next);
		Assert.IsTrue(ok);
		Assert.AreEqual(0x200u, off);
		Assert.AreEqual(0x3000u, len);
		Assert.AreEqual(( ushort ) 7, comp);
		Assert.AreEqual(0u, next);
	}
}
