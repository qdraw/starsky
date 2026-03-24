using System;
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
		var relativeOffset = ( uint )( taggedOffset - tiffBase );
		var leadingJpeg = CreateJpeg(18869);
		var taggedJpeg = CreateJpeg(taggedLength);
		var totalLength = ( int ) taggedOffset + taggedLength + 16;
		var bytes = new byte[totalLength];
		Array.Copy(leadingJpeg, 0, bytes, 292, leadingJpeg.Length);

		bytes[tiffBase] = 0x4D; bytes[tiffBase + 1] = 0x4D; bytes[tiffBase + 2] = 0x00; bytes[tiffBase + 3] = 0x2A;
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
		WriteUInt32BigEndian(bytes, ifd1 + 34, ( uint ) taggedLength);
		WriteUInt32BigEndian(bytes, ifd1 + 38, 0);

		Array.Copy(taggedJpeg, 0, bytes, taggedOffset, taggedJpeg.Length);

		var subPathStorage = new FakeIStorage(["/raw"], [RawPathX3F], [bytes]);
		var tempStorage = new FakeIStorage(["/tmp"]);
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(), new FakeIStorage(), tempStorage);
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
		bytes[tiffBase] = 0x4D; bytes[tiffBase + 1] = 0x4D; bytes[tiffBase + 2] = 0x00; bytes[tiffBase + 3] = 0x2A;
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
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(), new FakeIStorage(), tempStorage);
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
		bytes[tiffBase] = 0x4D; bytes[tiffBase + 1] = 0x4D; bytes[tiffBase + 2] = 0x00; bytes[tiffBase + 3] = 0x2A;
		WriteUInt32BigEndian(bytes, tiffBase + 4, 8);
		WriteUInt16BigEndian(bytes, tiffBase + 8, 2000);

		var subPathStorage = new FakeIStorage(["/raw"], [RawPathX3F], [bytes]);
		var tempStorage = new FakeIStorage(["/tmp"]);
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(), new FakeIStorage(), tempStorage);
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
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(), new FakeIStorage(), tempStorage);
		var extractor = new LightweightContainerPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract(subPath, OutputPath);

		Assert.IsTrue(result);
		Assert.IsTrue(tempStorage.ExistFile(OutputPath));
	}
}
