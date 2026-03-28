using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class App1PayloadProcessorTests
{
	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task Process_TooShortPayload_ReturnsFalse()
	{
		var result = await App1PayloadProcessor.Process(new byte[5], null, null, 0);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task Process_InvalidMagic_ReturnsFalse()
	{
		var payload = "NOTEXI"u8.ToArray();
		var result = await App1PayloadProcessor.Process(payload, null, null, 0);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task Process_InvalidTiffHeader_ReturnsFalse()
	{
		var payload = "Exif\0\0NOTTIFF"u8.ToArray();
		var result = await App1PayloadProcessor.Process(payload, null, null, 0);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task Process_NoBestPreview_ReturnsFalse()
	{
		// Valid Exif and TIFF header but no previews
		var payload = new byte[]
		{
			( byte ) 'E', ( byte ) 'x', ( byte ) 'i', ( byte ) 'f', 0, 0, ( byte ) 'I',
			( byte ) 'I', 42, 0, 8, 0, 0, 0, // TIFF Header
			0, 0, 0, 0 // 0 entries
		};
		var result = await App1PayloadProcessor.Process(payload, null, null, 0);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task Process_WithOutputLargeNull_ReturnsTrueIfFound()
	{
		// We need a valid TIFF with a JPEG preview
		var preview = new byte[4096];
		preview[0] = 0xFF;
		preview[1] = 0xD8;
		preview[2] = 0xFF;
		preview[^2] = 0xFF;
		preview[^1] = 0xD9;

		using var ms = new MemoryStream();
		ms.Write("Exif\0\0"u8);
		var tiffStart = ms.Position;
		ms.Write("II"u8);
		ms.WriteByte(42);
		ms.WriteByte(0);
		ms.WriteByte(8);
		ms.WriteByte(0);
		ms.WriteByte(0);
		ms.WriteByte(0);

		// IFD0 at tiffStart + 8
		ms.WriteByte(2);
		ms.WriteByte(0); // 2 entries

		// Entry 1: JpegOffset
		ms.WriteByte(0x01);
		ms.WriteByte(0x02); // 0x0201 JpegOffset
		ms.WriteByte(4);
		ms.WriteByte(0); // LONG
		ms.WriteByte(1);
		ms.WriteByte(0);
		ms.WriteByte(0);
		ms.WriteByte(0); // count 1
		var valueOffsetPos = ms.Position;
		ms.WriteByte(0);
		ms.WriteByte(0);
		ms.WriteByte(0);
		ms.WriteByte(0);

		// Entry 2: JpegLength
		ms.WriteByte(0x02);
		ms.WriteByte(0x02); // 0x0202 JpegLength
		ms.WriteByte(4);
		ms.WriteByte(0); // LONG
		ms.WriteByte(1);
		ms.WriteByte(0);
		ms.WriteByte(0);
		ms.WriteByte(0); // count 1
		await ms.WriteAsync(BitConverter.GetBytes(( uint ) preview.Length),
			TestContext.CancellationToken);

		ms.WriteByte(0);
		ms.WriteByte(0);
		ms.WriteByte(0);
		ms.WriteByte(0); // next IFD

		var jpegOffsetRel = ( uint ) ( ms.Position - tiffStart );
		await ms.WriteAsync(preview, TestContext.CancellationToken);

		var currentPos = ms.Position;
		ms.Seek(valueOffsetPos, SeekOrigin.Begin);
		await ms.WriteAsync(BitConverter.GetBytes(jpegOffsetRel), TestContext.CancellationToken);
		ms.Seek(currentPos, SeekOrigin.Begin);

		var result = await App1PayloadProcessor.Process(ms.ToArray(), null, null, 0);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task Process_WithOriginalStream_ScansForJpegs()
	{
		var preview = new byte[4096];
		preview[0] = 0xFF;
		preview[1] = 0xD8;
		preview[2] = 0xFF;
		preview[^2] = 0xFF;
		preview[^1] = 0xD9;

		// TIFF without previews but original stream HAS JPEGs
		using var ms = new MemoryStream();
		ms.Write("Exif\0\0"u8);
		ms.Write("II"u8);
		ms.WriteByte(42);
		ms.WriteByte(0);
		ms.WriteByte(8);
		ms.WriteByte(0);
		ms.WriteByte(0);
		ms.WriteByte(0);
		ms.WriteByte(0);
		ms.WriteByte(0); // 0 entries
		ms.WriteByte(0);
		ms.WriteByte(0);
		ms.WriteByte(0);
		ms.WriteByte(0); // next IFD

		var app1Payload = ms.ToArray();

		using var originalStream = new MemoryStream();
		await originalStream.WriteAsync(new byte[100], TestContext.CancellationToken); // padding
		await originalStream.WriteAsync(preview, TestContext.CancellationToken);

		var result = await App1PayloadProcessor.Process(app1Payload, null, originalStream, 0);
		Assert.IsTrue(result);
	}
}

// Additional tests for AddScannedCandidates
[TestClass]
public class AddScannedCandidatesTests
{
	private const int MinJpegSize = 4096;

	private static MemoryStream CreateStreamWithJpegs(params int[] offsets)
	{
		if ( offsets.Length == 0 )
		{
			return new MemoryStream([]);
		}

		var maxOffset = 0;
		foreach ( var o in offsets )
		{
			maxOffset = Math.Max(maxOffset, o);
		}

		var total = maxOffset + MinJpegSize + 16;
		var buf = new byte[total];

		foreach ( var o in offsets )
		{
			if ( o + 2 >= buf.Length )
			{
				continue;
			}

			buf[o] = 0xFF;
			buf[o + 1] = 0xD8;
			buf[o + 2] = 0xFF; // matches IsJpegStartMarker pattern

			var eoiPos = o + MinJpegSize - 2;
			if ( eoiPos + 1 < buf.Length )
			{
				buf[eoiPos] = 0xFF;
				buf[eoiPos + 1] = 0xD9;
			}
		}

		return new MemoryStream(buf);
	}

	[TestMethod]
	public void AddScannedCandidates_NullStream_NoCandidates()
	{
		var candidates = new List<PreviewCandidate>();
		App1PayloadProcessor.AddScannedCandidates(null, candidates);
		Assert.IsEmpty(candidates);
	}

	[TestMethod]
	public void AddScannedCandidates_SkipsPrimaryAtZero()
	{
		using var ms = CreateStreamWithJpegs(0);
		var candidates = new List<PreviewCandidate>();
		App1PayloadProcessor.AddScannedCandidates(ms, candidates);
		Assert.IsEmpty(candidates);
	}

	[TestMethod]
	public void AddScannedCandidates_AddsNonZeroCandidate()
	{
		using var ms = CreateStreamWithJpegs(0, 6000);
		var candidates = new List<PreviewCandidate>();
		App1PayloadProcessor.AddScannedCandidates(ms, candidates);
		Assert.IsTrue(candidates.Exists(c => c.Offset == 6000));
	}

	[TestMethod]
	public void AddScannedCandidates_CapsAt16()
	{
		var offsets = new int[20];
		for ( var i = 0; i < 20; i++ )
		{
			offsets[i] = 1000 + i * 6000;
		}

		using var ms = CreateStreamWithJpegs(offsets);
		var candidates = new List<PreviewCandidate>();
		App1PayloadProcessor.AddScannedCandidates(ms, candidates);
		Assert.HasCount(16, candidates);
	}

	[TestMethod]
	public void AddScannedCandidates_ExceptionHandled()
	{
		using var bs = new BrokenStream();
		var candidates = new List<PreviewCandidate>();
		App1PayloadProcessor.AddScannedCandidates(bs, candidates);
		Assert.IsEmpty(candidates);
	}

	private sealed class BrokenStream : Stream
	{
		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => false;
		public override long Length => throw new InvalidOperationException("boom");

		[SuppressMessage("Usage", "S3237:value in set")]
		public override long Position
		{
			get => 0;
			set
			{
				// do nothing as we just want to throw on Length access
			}
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new InvalidOperationException("boom");
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new InvalidOperationException("boom");
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
}

[TestClass]
public class TryExtractBestPreviewTests
{
	private static MemoryStream MakeStreamWithJpegAt(long offset, int length)
	{
		var total = ( int ) ( offset + length + 8 );
		var buf = new byte[total];
		if ( offset + 2 < buf.Length )
		{
			buf[offset] = 0xFF;
			buf[offset + 1] = 0xD8;
			buf[offset + 2] = 0xFF;
		}

		var eoi = offset + length - 2;
		if ( eoi + 1 < buf.Length )
		{
			buf[eoi] = 0xFF;
			buf[eoi + 1] = 0xD9;
		}

		// fill with deterministic data so we can assert later
		for ( var i = 0; i < buf.Length; i++ )
		{
			buf[i] = ( byte ) ( i % 251 );
		}

		return new MemoryStream(buf);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_ExtractsFromTiffMs_WhenInsideTiff()
	{
		const long offset = 100L;
		const uint length = 1024u;
		using var tiffMs = MakeStreamWithJpegAt(offset, ( int ) length);
		// originalStream null
		PreviewCandidate best = new() { Offset = ( uint ) offset, Length = length };
		using var outMs = new MemoryStream();

		var ok = await App1PayloadProcessor.TryExtractBestPreview(best, tiffMs, null, 0, outMs);
		Assert.IsTrue(ok);
		Assert.AreEqual(length, ( uint ) outMs.Length);
		// verify first two bytes are JPEG SOI
		outMs.Seek(0, SeekOrigin.Begin);
		var b = outMs.ReadByte();
		var b2 = outMs.ReadByte();
		Assert.AreEqual(0xFF, b);
		Assert.AreEqual(0xD8, b2);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_ReturnsFalse_WhenOriginalNullAndNotInTiff()
	{
		// tiffMs too short so inside-tiff check fails
		using var tiffMs = new MemoryStream(new byte[10]);
		PreviewCandidate best = new() { Offset = 1000, Length = 2000 };
		using var outMs = new MemoryStream();

		var ok = await App1PayloadProcessor.TryExtractBestPreview(best, tiffMs, null, 0, outMs);
		Assert.IsFalse(ok);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_UsesMappedOffset_WhenValid()
	{
		// tiffMs doesn't contain candidate
		using var tiffMs = new MemoryStream(new byte[100]);
		var bestOffset = 50u;
		var length = 1024u;
		// payloadStart such that mappedBest = payloadStart + 6 + bestOffset
		var payloadStart = 500L;
		var mappedBest = payloadStart + 6 + bestOffset;
		using var originalStream = MakeStreamWithJpegAt(mappedBest, ( int ) length);

		PreviewCandidate best = new() { Offset = bestOffset, Length = length };
		using var outMs = new MemoryStream();

		var ok = await App1PayloadProcessor.TryExtractBestPreview(best, tiffMs, originalStream,
			payloadStart, outMs);
		Assert.IsTrue(ok);
		Assert.AreEqual(length, ( uint ) outMs.Length);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_UsesAbsoluteOffset_WhenMappedFailsButAbsoluteValid()
	{
		using var tiffMs = new MemoryStream(new byte[100]);
		var bestOffset = 300u;
		var length = 1024u;
		var payloadStart = 200L;
		var mappedBest = payloadStart + 6 + bestOffset;

		// build originalStream with NO JPEG at mappedBest but WITH JPEG at absolute bestOffset
		var total = ( int ) ( mappedBest + length + 16 );
		var buf = new byte[total];
		// put JPEG at absolute offset
		var abs = ( int ) bestOffset;
		buf[abs] = 0xFF;
		buf[abs + 1] = 0xD8;
		buf[abs + 2] = 0xFF;
		var eoi = abs + ( int ) length - 2;
		buf[eoi] = 0xFF;
		buf[eoi + 1] = 0xD9;
		for ( var i = 0; i < buf.Length; i++ )
		{
			buf[i] = ( byte ) ( ( i + 7 ) % 251 );
		}

		using var originalStream = new MemoryStream(buf);

		PreviewCandidate best = new() { Offset = bestOffset, Length = length };
		using var outMs = new MemoryStream();

		var ok = await App1PayloadProcessor.TryExtractBestPreview(best, tiffMs, originalStream,
			payloadStart, outMs);
		Assert.IsTrue(ok);
		Assert.AreEqual(length, ( uint ) outMs.Length);
	}

	[TestMethod]
	public async Task TryExtractBestPreview_ReturnsFalse_WhenMappedInBoundsButAbsoluteOutOfBounds()
	{
		using var tiffMs = new MemoryStream(new byte[100]);
		const uint bestOffset = 100u;
		const uint length = 1024u;
		const long payloadStart = 200L;
		var mappedBest = payloadStart + 6 + bestOffset;
		// original stream that contains mapped region but absolute offset is out-of-bounds
		var total = ( int ) ( mappedBest + length + 8 );
		var buf = new byte[total];
		// put NO JPEG at mappedBest (so TryValidate will fail)
		for ( var i = 0; i < buf.Length; i++ )
		{
			buf[i] = 0;
		}

		using var originalStream = new MemoryStream(buf);

		// set best such that best.Offset + best.Length > originalStream.Length to trigger early false
		PreviewCandidate best = new()
		{
			Offset = ( uint ) ( originalStream.Length - 10 ), Length = 1024
		};
		using var outMs = new MemoryStream();

		var ok = await App1PayloadProcessor.TryExtractBestPreview(best, tiffMs, originalStream,
			payloadStart, outMs);
		Assert.IsFalse(ok);
	}
}
