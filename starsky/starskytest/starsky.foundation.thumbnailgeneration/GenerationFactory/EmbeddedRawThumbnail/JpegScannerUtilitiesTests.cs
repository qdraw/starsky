using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class JpegScannerUtilitiesTests
{
	private static byte[] BuildJpeg(int totalSize)
	{
		if ( totalSize < 4 )
		{
			totalSize = 4;
		}

		var buf = new byte[totalSize];
		// SOI
		buf[0] = 0xFF;
		buf[1] = 0xD8;

		// Fill middle with non-marker bytes (0x00)
		for ( var i = 2; i < totalSize - 2; i++ )
		{
			buf[i] = 0x00;
		}

		// EOI
		buf[totalSize - 2] = 0xFF;
		buf[totalSize - 1] = 0xD9;
		return buf;
	}

	[TestMethod]
	public void DetectJpegLengthFromStart_FindsEoi_WhenPresent()
	{
		var length = 8192;
		var jpeg = BuildJpeg(length);
		using var ms = new MemoryStream(jpeg, false);

		var found = JpegScannerUtilities.DetectJpegLengthFromStart(ms, 0, length);

		Assert.AreEqual(( uint ) length, found);
	}

	[TestMethod]
	public void DetectJpegLengthFromSoi_FindsEoi_WhenSoiOffsetProvided()
	{
		var length = 4096;
		var prefix = new byte[32];
		var jpeg = BuildJpeg(length);

		using var ms = new MemoryStream();
		ms.Write(prefix);
		ms.Write(jpeg);
		ms.Seek(0, SeekOrigin.Begin);

		var soiOffset = ( uint ) prefix.Length;
		var found = JpegScannerUtilities.DetectJpegLengthFromSoi(ms, soiOffset, length);

		Assert.AreEqual(( uint ) length, found);
	}

	[TestMethod]
	public void DetectJpegLength_ReturnsZero_WhenEoiNotWithinMaxScan()
	{
		var length = 10000;
		var jpeg = BuildJpeg(length);
		using var ms = new MemoryStream(jpeg, false);

		// maxScanBytes smaller than distance to EOI
		var found = JpegScannerUtilities.DetectJpegLengthFromStart(ms, 0, 5000);

		Assert.AreEqual(0u, found);
	}

	[TestMethod]
	public void DetectJpegLength_ReturnsZero_WhenStreamNotSeekable()
	{
		var length = 4096;
		var jpeg = BuildJpeg(length);
		using var ms = new NonSeekableStream(jpeg);

		var found = JpegScannerUtilities.DetectJpegLengthFromStart(ms, 0, length);

		Assert.AreEqual(0u, found);
	}

	[TestMethod]
	public void DetectJpegLengthFromStart_ReturnsZero_WhenMaxScanTooSmall()
	{
		const int length = 1024;
		var jpeg = BuildJpeg(length);
		using var ms = new MemoryStream(jpeg, false);

		// DetectJpegLengthFromStart requires at least 2
		var found = JpegScannerUtilities.DetectJpegLengthFromStart(ms, 0, 1);

		Assert.AreEqual(0u, found);
	}

	[TestMethod]
	public void DetectJpegLengthFromSoi_ReturnsZero_WhenMaxScanTooSmall()
	{
		const int length = 1024;
		var jpeg = BuildJpeg(length);
		using var ms = new MemoryStream(jpeg, false);

		// DetectJpegLengthFromSoi requires at least 4
		var found = JpegScannerUtilities.DetectJpegLengthFromSoi(ms, 0, 3);

		Assert.AreEqual(0u, found);
	}

	private sealed class NonSeekableStream(byte[] data) : Stream
	{
		private int _pos;
		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => _pos;
			set => throw new NotSupportedException();
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var available = Math.Min(count, data.Length - _pos);
			if ( available <= 0 )
			{
				return 0;
			}

			Array.Copy(data, _pos, buffer, offset, available);
			_pos += available;
			return available;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
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
