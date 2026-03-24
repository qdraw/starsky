using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.
	TiffEmbeded;

[TestClass]
public class TiffEmbeddedPreviewOutputTests
{
	private static MemoryStream MakeStream(byte[] data)
	{
		return new MemoryStream(data);
	}

	[TestMethod]
	public void ExtractPreview_OffsetPlusLengthGreaterThanStream_ShouldReturnFalse()
	{
		var ms = MakeStream(new byte[10]);
		var preview =
			new TiffEmbeddedPreviewExtractor.PreviewCandidate
			{
				Offset = 8, Length = 4, Width = 0, Height = 0
			};
		var res = TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ms, preview, null)
			.GetAwaiter().GetResult();
		Assert.IsFalse(res);
	}

	[TestMethod]
	public void ExtractPreview_OutputNullWithValidJpeg_ShouldReturnTrue()
	{
		var smallJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x01, 0x02, 0xFF, 0xD9 };
		var data = new byte[20];
		Array.Copy(smallJpeg, 0, data, 5, smallJpeg.Length);
		var ms = MakeStream(data);
		var preview = new TiffEmbeddedPreviewExtractor.PreviewCandidate
		{
			Offset = 5, Length = ( uint ) smallJpeg.Length, Width = 0, Height = 0
		};
		var res = TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ms, preview, null)
			.GetAwaiter().GetResult();
		Assert.IsTrue(res);
	}

	[TestMethod]
	public void ExtractPreview_WriteToOutput_WritesCorrectBytes()
	{
		var smallJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x01, 0x02, 0xFF, 0xD9 };
		var data = new byte[50];
		Array.Copy(smallJpeg, 0, data, 10, smallJpeg.Length);
		var ms = MakeStream(data);
		var outMs = new MemoryStream();
		var preview = new TiffEmbeddedPreviewExtractor.PreviewCandidate
		{
			Offset = 10, Length = ( uint ) smallJpeg.Length, Width = 0, Height = 0
		};
		var res = TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ms, preview, outMs)
			.GetAwaiter().GetResult();
		Assert.IsTrue(res);
		var written = outMs.ToArray();
		Assert.AreEqual(smallJpeg.Length, written.Length);
		for ( var i = 0; i < written.Length; i++ )
		{
			Assert.AreEqual(smallJpeg[i], written[i]);
		}
	}

	[TestMethod]
	public void ExtractPreview_TruncatedRead_ShouldReturnFalse()
	{
		var smallJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x01, 0x02, 0xFF, 0xD9 };
		var data = new byte[12];
		Array.Copy(smallJpeg, 0, data, 8, Math.Max(0, data.Length - 8));
		var ms = MakeStream(data);
		var outMs = new MemoryStream();
		var preview = new TiffEmbeddedPreviewExtractor.PreviewCandidate
		{
			Offset = 8, Length = ( uint ) ( smallJpeg.Length + 10 ), Width = 0, Height = 0
		};
		var res = TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ms, preview, outMs)
			.GetAwaiter().GetResult();
		Assert.IsFalse(res);
	}

	[TestMethod]
	public void ExtractPreview_NonSeekableStream_ShouldReturnFalse()
	{
		var smallJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x01, 0x02, 0xFF, 0xD9 };
		var data = new byte[30];
		Array.Copy(smallJpeg, 0, data, 4, smallJpeg.Length);
		var inner = new MemoryStream(data);
		var ns = new NonSeekableStream(inner);
		var outMs = new MemoryStream();
		var preview = new TiffEmbeddedPreviewExtractor.PreviewCandidate
		{
			Offset = 4, Length = ( uint ) smallJpeg.Length, Width = 0, Height = 0
		};
		var res = TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ns, preview, outMs)
			.GetAwaiter().GetResult();
		Assert.IsFalse(res);
	}

	[TestMethod]
	public void ExtractPreview_InsufficientMarkerBytes_ShouldReturnFalse()
	{
		var data = new byte[4];
		data[3] = 0xFF;
		var ms = MakeStream(data);
		var preview =
			new TiffEmbeddedPreviewExtractor.PreviewCandidate
			{
				Offset = 3, Length = 1, Width = 0, Height = 0
			};
		var res = TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ms, preview, null)
			.GetAwaiter().GetResult();
		Assert.IsFalse(res);
	}

	// Minimal non-seekable wrapper to simulate streams that don't support Seek
	private sealed class NonSeekableStream(Stream inner) : Stream
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

		public override ValueTask<int> ReadAsync(Memory<byte> buffer,
			CancellationToken cancellationToken = default)
		{
			return inner.ReadAsync(buffer, cancellationToken);
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count,
			CancellationToken cancellationToken)
		{
			return inner.ReadAsync(buffer, offset, count, cancellationToken);
		}
	}
}
