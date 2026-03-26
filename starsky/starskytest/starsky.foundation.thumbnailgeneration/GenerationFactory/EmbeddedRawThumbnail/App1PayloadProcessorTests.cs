using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

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
			return new MemoryStream(new byte[0]);
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
		var candidates = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();
		App1PayloadProcessor.AddScannedCandidates(null, candidates);
		Assert.IsEmpty(candidates);
	}

	[TestMethod]
	public void AddScannedCandidates_SkipsPrimaryAtZero()
	{
		using var ms = CreateStreamWithJpegs(0);
		var candidates = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();
		App1PayloadProcessor.AddScannedCandidates(ms, candidates);
		Assert.IsEmpty(candidates);
	}

	[TestMethod]
	public void AddScannedCandidates_AddsNonZeroCandidate()
	{
		using var ms = CreateStreamWithJpegs(0, 6000);
		var candidates = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();
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
		var candidates = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();
		App1PayloadProcessor.AddScannedCandidates(ms, candidates);
		Assert.HasCount(16, candidates);
	}

	[TestMethod]
	public void AddScannedCandidates_ExceptionHandled()
	{
		using var bs = new BrokenStream();
		var candidates = new List<TiffEmbeddedPreviewExtractor.PreviewCandidate>();
		App1PayloadProcessor.AddScannedCandidates(bs, candidates);
		Assert.IsEmpty(candidates);
	}

	private class BrokenStream : Stream
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
