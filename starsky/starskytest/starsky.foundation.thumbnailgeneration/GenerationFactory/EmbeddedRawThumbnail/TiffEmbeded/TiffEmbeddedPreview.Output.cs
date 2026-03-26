using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;
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
	public async Task ExtractPreview_OffsetPlusLengthGreaterThanStream_ShouldReturnFalse()
	{
		var ms = MakeStream(new byte[10]);
		var preview =
			new PreviewCandidate { Offset = 8, Length = 4, Width = 0, Height = 0 };
		var res = await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ms, preview, null);
		Assert.IsFalse(res);
	}

	[TestMethod]
	public async Task ExtractPreview_OutputNullWithValidJpeg_ShouldReturnTrue()
	{
		var smallJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x01, 0x02, 0xFF, 0xD9 };
		var data = new byte[20];
		Array.Copy(smallJpeg, 0, data, 5, smallJpeg.Length);
		var ms = MakeStream(data);
		var preview = new PreviewCandidate
		{
			Offset = 5, Length = ( uint ) smallJpeg.Length, Width = 0, Height = 0
		};
		var res = await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ms, preview, null);
		Assert.IsTrue(res);
	}

	[TestMethod]
	public async Task ExtractPreview_WriteToOutput_WritesCorrectBytes()
	{
		var smallJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x01, 0x02, 0xFF, 0xD9 };
		var data = new byte[50];
		Array.Copy(smallJpeg, 0, data, 10, smallJpeg.Length);
		var ms = MakeStream(data);
		var outMs = new MemoryStream();
		var preview = new PreviewCandidate
		{
			Offset = 10, Length = ( uint ) smallJpeg.Length, Width = 0, Height = 0
		};
		var res = await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ms, preview, outMs);
		Assert.IsTrue(res);
		var written = outMs.ToArray();
		Assert.HasCount(smallJpeg.Length, written);
		for ( var i = 0; i < written.Length; i++ )
		{
			Assert.AreEqual(smallJpeg[i], written[i]);
		}
	}

	[TestMethod]
	public async Task ExtractPreview_TruncatedRead_ShouldReturnFalse()
	{
		var smallJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x01, 0x02, 0xFF, 0xD9 };
		var data = new byte[12];
		Array.Copy(smallJpeg, 0, data, 8, Math.Max(0, data.Length - 8));
		var ms = MakeStream(data);
		var outMs = new MemoryStream();
		var preview = new PreviewCandidate
		{
			Offset = 8, Length = ( uint ) ( smallJpeg.Length + 10 ), Width = 0, Height = 0
		};
		var res = await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ms, preview, outMs);
		Assert.IsFalse(res);
	}

	[TestMethod]
	public async Task ExtractPreview_NonSeekableStream_ShouldReturnFalse()
	{
		var smallJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x01, 0x02, 0xFF, 0xD9 };
		var data = new byte[30];
		Array.Copy(smallJpeg, 0, data, 4, smallJpeg.Length);
		var inner = new MemoryStream(data);
		var ns = new NonSeekableStream(inner);
		var outMs = new MemoryStream();
		var preview = new PreviewCandidate
		{
			Offset = 4, Length = ( uint ) smallJpeg.Length, Width = 0, Height = 0
		};
		var res = await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ns, preview, outMs);
		Assert.IsFalse(res);
	}

	[TestMethod]
	public async Task ExtractPreview_InsufficientMarkerBytes_ShouldReturnFalse()
	{
		var data = new byte[4];
		data[3] = 0xFF;
		var ms = MakeStream(data);
		var preview =
			new PreviewCandidate { Offset = 3, Length = 1, Width = 0, Height = 0 };
		var res = await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ms, preview, null);

		Assert.IsFalse(res);
	}

	[TestMethod]
	public async Task ExtractPreview_SeekFailsInsideExtraction_ShouldReturnFalse()
	{
		var smallJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x01, 0x02, 0xFF, 0xD9 };
		var data = new byte[20];
		Array.Copy(smallJpeg, 0, data, 0, smallJpeg.Length);
		var ms = MakeStream(data);

		// This stream will allow the first seek (in TryValidateJpegOffset)
		// but fail the second seek (inside ExtractPreviewToStream)
		var ss = new SequenceSeekFailingStream(ms, 1);

		var outMs = new MemoryStream();
		var preview = new PreviewCandidate
		{
			Offset = 0, Length = ( uint ) smallJpeg.Length, Width = 0, Height = 0
		};
		var res = await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ss, preview, outMs);
		Assert.IsFalse(res);
	}

	private sealed class SequenceSeekFailingStream(Stream inner, int succeedCount) : Stream
	{
		private int _seekCount;
		public override bool CanRead => inner.CanRead;
		public override bool CanSeek => true;
		public override bool CanWrite => inner.CanWrite;
		public override long Length => inner.Length;

		public override long Position
		{
			get => inner.Position;
			set => inner.Position = value;
		}

		public override void Flush()
		{
			inner.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return inner.Read(buffer, offset, count);
		}

		public override void SetLength(long value)
		{
			inner.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			inner.Write(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if ( _seekCount >= succeedCount )
			{
				throw new IOException("Mock Seek Exception");
			}

			_seekCount++;
			return inner.Seek(offset, origin);
		}

		public override ValueTask<int> ReadAsync(Memory<byte> buffer,
			CancellationToken cancellationToken = default)
		{
			return inner.ReadAsync(buffer, cancellationToken);
		}
	}

	[TestMethod]
	public async Task ExtractPreview_InvalidJpegSOI_ShouldReturnFalse()
	{
		var data = new byte[30];
		// Not 0xFF, 0xD8, 0xFF
		data[0] = 0x00;
		data[1] = 0x01;
		data[2] = 0x02;
		var ms = MakeStream(data);
		var preview = new PreviewCandidate { Offset = 0, Length = 10, Width = 0, Height = 0 };
		var res = await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ms, preview, null);
		Assert.IsFalse(res);
	}

	[TestMethod]
	public async Task ExtractPreview_LargeContent_ShouldLoopBuffer()
	{
		const int size = 70000; // > 65536
		var data = new byte[size];
		data[0] = 0xFF;
		data[1] = 0xD8;
		data[2] = 0xFF; // SOI
		for ( var i = 3; i < size; i++ )
		{
			data[i] = ( byte ) ( i % 256 );
		}

		var ms = MakeStream(data);
		var outMs = new MemoryStream();
		var preview = new PreviewCandidate { Offset = 0, Length = size, Width = 0, Height = 0 };
		var res = await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ms, preview, outMs);
		Assert.IsTrue(res);
		Assert.AreEqual(size, outMs.Length);
		var written = outMs.ToArray();
		for ( var i = 0; i < size; i++ )
		{
			Assert.AreEqual(data[i], written[i]);
		}
	}

	[TestMethod]
	public async Task ExtractPreview_ReadAsyncThrows_ShouldReturnFalse()
	{
		var smallJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x01, 0x02, 0xFF, 0xD9 };
		var data = new byte[20];
		Array.Copy(smallJpeg, 0, data, 0, smallJpeg.Length);
		var inner = new MemoryStream(data);
		var ts = new ThrowingStream(inner, true, false);
		var outMs = new MemoryStream();
		var preview = new PreviewCandidate
		{
			Offset = 0, Length = ( uint ) smallJpeg.Length, Width = 0, Height = 0
		};
		var res = await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ts, preview, outMs);
		Assert.IsFalse(res);
	}

	[TestMethod]
	public async Task ExtractPreview_WriteAsyncThrows_ShouldReturnFalse()
	{
		var smallJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x01, 0x02, 0xFF, 0xD9 };
		var data = new byte[20];
		Array.Copy(smallJpeg, 0, data, 0, smallJpeg.Length);
		var ms = MakeStream(data);
		var ts = new ThrowingStream(new MemoryStream(), false, true);
		var preview = new PreviewCandidate
		{
			Offset = 0, Length = ( uint ) smallJpeg.Length, Width = 0, Height = 0
		};
		var res = await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(ms, preview, ts);
		Assert.IsFalse(res);
	}

	[TestMethod]
	public async Task ExtractPreview_ReadZeroButStillRemaining_ShouldReturnFalse()
	{
		var smallJpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x01, 0x02, 0xFF, 0xD9 };
		var data = new byte[20];
		Array.Copy(smallJpeg, 0, data, 0, smallJpeg.Length);
		var inner = new MemoryStream(data);
		// ZeroReadStream will return 0 bytes read
		var zr = new ZeroReadStream(inner);
		var outMs = new MemoryStream();
		var preview = new PreviewCandidate
		{
			Offset = 0, Length = ( uint ) smallJpeg.Length, Width = 0, Height = 0
		};
		var res = await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(zr, preview, outMs);
		Assert.IsFalse(res);
	}

	private sealed class ZeroReadStream(Stream inner) : Stream
	{
		public override bool CanRead => inner.CanRead;
		public override bool CanSeek => inner.CanSeek;
		public override bool CanWrite => inner.CanWrite;
		public override long Length => inner.Length;

		public override long Position
		{
			get => inner.Position;
			set => inner.Position = value;
		}

		public override void Flush()
		{
			inner.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return inner.Seek(offset, origin);
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
			return new ValueTask<int>(0);
		}
	}

	private sealed class ThrowingStream(Stream inner, bool throwOnRead, bool throwOnWrite) : Stream
	{
		public override bool CanRead => inner.CanRead;
		public override bool CanSeek => inner.CanSeek;
		public override bool CanWrite => inner.CanWrite;
		public override long Length => inner.Length;

		public override long Position
		{
			get => inner.Position;
			set => inner.Position = value;
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
			return inner.Seek(offset, origin);
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
			return throwOnRead
				? throw new IOException("Mock Read Exception")
				: inner.ReadAsync(buffer, cancellationToken);
		}

		public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
			CancellationToken cancellationToken = default)
		{
			if ( throwOnWrite )
			{
				throw new IOException("Mock Write Exception");
			}

			return inner.WriteAsync(buffer, cancellationToken);
		}
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
