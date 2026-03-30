using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Helpers;
using starskytest.FakeCreateAn.CreateAnImageA6700PreviewRawJpeg;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Helpers;

[TestClass]
public class JpegExtractPreviewHelperTests
{
	[TestMethod]
	public async Task TryExtractFromStream_SeekToStart_IfSeekable()
	{
		var data = new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 };
		using var ms = new MemoryStream(data);
		ms.Position = 2; // Not at start

		var result = await JpegExtractPreviewHelper.TryExtractFromStream(ms, null);
		
		Assert.IsFalse(result); // Should fail because it finds EOI immediately after SOI if it seeks back
		Assert.AreEqual(4, ms.Position); // It should have read the whole stream
	}

	[TestMethod]
	public async Task TryExtractFromStream_InvalidSoi_ReturnsFalse()
	{
		var data = new byte[] { 0x00, 0x00 };
		using var ms = new MemoryStream(data);
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(ms, null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtractFromStream_TooShort_ReturnsFalse()
	{
		var data = new byte[] { 0xFF };
		using var ms = new MemoryStream(data);
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(ms, null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task ProcessJpegMarkersAsync_EofInMarkers_ReturnsFalse()
	{
		var data = new byte[] { 0xFF, 0xD8, 0xFF }; // Truncated after marker prefix
		using var ms = new MemoryStream(data);
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(ms, null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task ProcessJpegMarkersAsync_StandaloneMarkers_SkipCorrectly()
	{
		var data = new byte[]
		{
			0xFF, 0xD8, // SOI
			0xFF, 0x01, // TEM (standalone)
			0xFF, 0xD0, // RST0 (standalone)
			0xFF, 0xD9  // EOI
		};
		using var ms = new MemoryStream(data);
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(ms, null);
		Assert.IsFalse(result); // No APP1 found
	}

	[TestMethod]
	public async Task ProcessNonStandaloneMarkerAsync_ReadLengthFail_ReturnsError()
	{
		var data = new byte[]
		{
			0xFF, 0xD8, // SOI
			0xFF, 0xE0, // APP0
			0x00        // Only 1 byte of length
		};
		using var ms = new MemoryStream(data);
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(ms, null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task ProcessNonStandaloneMarkerAsync_NonApp1_SkipPayload()
	{
		var data = new byte[]
		{
			0xFF, 0xD8, // SOI
			0xFF, 0xE0, // APP0
			0x00, 0x03, // Length 3
			0x01,       // 1 byte payload
			0xFF, 0xD9  // EOI
		};
		using var ms = new MemoryStream(data);
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(ms, null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task ProcessNonStandaloneMarkerAsync_SkipPayloadFail_ReturnsFalse()
	{
		var data = new byte[]
		{
			0xFF, 0xD8, // SOI
			0xFF, 0xE0, // APP0
			0x00, 0x05, // Length 5 (payload 3)
			0x01, 0x02  // Only 2 bytes payload
		};
		using var ms = new MemoryStream(data);
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(ms, null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task ReadSegmentPayloadAsync_ReadFail_ReturnsError()
	{
		var data = new byte[]
		{
			0xFF, 0xD8, // SOI
			0xFF, 0xE1, // APP1
			0x00, 0x05, // Length 5 (payload 3)
			0x01, 0x02  // Only 2 bytes payload (premature EOF)
		};
		using var ms = new MemoryStream(data);
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(ms, null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task SkipSegmentAsync_NonSeekable_Works()
	{
		var data = new byte[]
		{
			0xFF, 0xD8, // SOI
			0xFF, 0xE0, // APP0
			0x00, 0x05, // Length 5 (payload 3)
			0x01, 0x02, 0x03, // payload
			0xFF, 0xD9  // EOI
		};
		var nonSeekable = new NonSeekableStream(new MemoryStream(data));
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(nonSeekable, null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task SkipSegmentAsync_NonSeekable_FailOnRead_ReturnsFalse()
	{
		var data = new byte[]
		{
			0xFF, 0xD8, // SOI
			0xFF, 0xE0, // APP0
			0x00, 0x05, // Length 5 (payload 3)
			0x01, 0x02  // only 2 bytes payload
		};
		var nonSeekable = new NonSeekableStream(new MemoryStream(data));
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(nonSeekable, null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task ReadNextMarker_HandlesMultipleFFs()
	{
		var data = new byte[]
		{
			0xFF, 0xD8,       // SOI
			0xFF, 0xFF, 0xFF, 0xD9 // Multiple FFs before EOI
		};
		using var ms = new MemoryStream(data);
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(ms, null);
		Assert.IsFalse(result);
	}
	
	[TestMethod]
	public async Task ReadSegmentPayloadAsync_PayloadSizeZero_ReturnsEmptyArray()
	{
		var data = new byte[]
		{
			0xFF, 0xD8, // SOI
			0xFF, 0xE1, // APP1
			0x00, 0x02, // Length 2 (payload 0)
			0xFF, 0xD9  // EOI
		};
		using var ms = new MemoryStream(data);
		// App1PayloadProcessor.Process will return false because empty payload is not valid Exif
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(ms, null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtractFromStream_Success()
	{
		var bytes = new CreateAnImageA6700PreviewRawJpeg().BytesJpeg;
		using var ms = new MemoryStream([.. bytes]);
		using var output = new MemoryStream();
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(ms, output);
		Assert.IsTrue(result);
		Assert.IsGreaterThan(0, output.Length);
	}

	[TestMethod]
	public async Task ProcessJpegMarkersAsync_MarkerFound_ReturnsTrue()
	{
		// This is covered by TryExtractFromStream_Success, but we can also test it via APP1 with valid payload
		var bytes = new CreateAnImageA6700PreviewRawJpeg().BytesJpeg;
		using var ms = new MemoryStream([.. bytes]);
		// Skip SOI
		ms.ReadByte();
		ms.ReadByte();
		var result = await JpegExtractPreviewHelper.ProcessJpegMarkersAsync(ms, new MemoryStream());
		Assert.IsTrue(result);
	}

	[TestMethod]
	[DataRow(0xD0)]
	[DataRow(0xD1)]
	[DataRow(0xD2)]
	[DataRow(0xD3)]
	[DataRow(0xD4)]
	[DataRow(0xD5)]
	[DataRow(0xD6)]
	[DataRow(0xD7)]
	[DataRow(0x01)]
	public async Task ProcessJpegMarkersAsync_AllStandaloneMarkers_SkipCorrectly(int markerByte)
	{
		var data = new byte[]
		{
			0xFF, 0xD8, // SOI
			0xFF, (byte)markerByte, // Standalone
			0xFF, 0xD9  // EOI
		};
		using var ms = new MemoryStream(data);
		var result = await JpegExtractPreviewHelper.TryExtractFromStream(ms, null);
		Assert.IsFalse(result); // No APP1 found
	}

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

		public override void Flush() => inner.Flush();

		public override int Read(byte[] buffer, int offset, int count) => 
			inner.Read(buffer, offset, count);

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count,
			CancellationToken cancellationToken)
			=> inner.ReadAsync(buffer, offset, count, cancellationToken);

		public override ValueTask<int> ReadAsync(Memory<byte> buffer, 
			CancellationToken cancellationToken = default)
			=> inner.ReadAsync(buffer, cancellationToken);

		public override long Seek(long offset, SeekOrigin origin) => 
			throw new NotSupportedException();

		public override void SetLength(long value) => throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count) => 
			inner.Write(buffer, offset, count);
	}
}
