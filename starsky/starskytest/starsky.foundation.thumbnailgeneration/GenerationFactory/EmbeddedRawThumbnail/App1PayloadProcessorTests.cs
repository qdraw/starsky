using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class App1PayloadProcessorTests
{
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

	public TestContext TestContext { get; set; }
}
