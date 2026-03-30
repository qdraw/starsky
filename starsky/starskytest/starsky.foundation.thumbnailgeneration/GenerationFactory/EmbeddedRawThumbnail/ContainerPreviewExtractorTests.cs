using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class ContainerPreviewExtractorTests
{
	private const string RawPathFff = "/raw/test.fff";
	private const string RawPathX3F = "/raw/test.x3f";
	private const string RawPathRaf = "/raw/test.raf";
	private const string OutputPath = "/tmp/preview.jpg";

	private static FakeSelectorStorageByType CreateSelectorStorage(byte[] bytes,
		string filePath,
		bool includeInTemp,
		out FakeIStorage tempStorage)
	{
		FakeIStorage subPathStorage;
		subPathStorage = new FakeIStorage(
			["/raw"],
			[filePath],
			[bytes]);

		tempStorage = includeInTemp
			? new FakeIStorage(
				["/tmp", "/raw"],
				[filePath],
				[bytes])
			: new FakeIStorage(["/tmp"]);

		var thumbnailStorage = new FakeIStorage();
		var hostStorage = new FakeIStorage();
		return new FakeSelectorStorageByType(subPathStorage, thumbnailStorage, hostStorage,
			tempStorage);
	}

	private static byte[] CreateJpeg(int totalLength, bool withIptc)
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
			var iptcPayload = "Photoshop 3.0\0"u8.ToArray();
			var segmentLength = iptcPayload.Length + 2;
			ms.WriteByte(0xFF);
			ms.WriteByte(0xED);
			ms.WriteByte(( byte ) ( ( segmentLength >> 8 ) & 0xFF ));
			ms.WriteByte(( byte ) ( segmentLength & 0xFF ));
			ms.Write(iptcPayload);
		}
		else
		{
			// Minimal APP0 segment
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

	private static byte[] CreateContainerWithTwoJpegsPreferIptc()
	{
		var largerNoIptc = CreateJpeg(8200, false);
		var smallerWithIptc = CreateJpeg(5400, true);
		var leading = Enumerable.Repeat(( byte ) 0xAA, 256).ToArray();
		var middle = Enumerable.Repeat(( byte ) 0xBB, 128).ToArray();

		return [.. leading, .. largerNoIptc, .. middle, .. smallerWithIptc];
	}

	private static byte[] CreateContainerWithTwoJpegsPreferIptcWithX3FHeader()
	{
		var baseBytes = CreateContainerWithTwoJpegsPreferIptc();
		// Prepend or overwrite the first four bytes with X3F signature: 'F','O','V','b'
		if ( baseBytes.Length >= 4 )
		{
			baseBytes[0] = ( byte ) 'F';
			baseBytes[1] = ( byte ) 'O';
			baseBytes[2] = ( byte ) 'V';
			baseBytes[3] = ( byte ) 'b';
		}
		return baseBytes;
	}

	private static byte[] CreateRafContainerWithIptcJpeg()
	{
		var jpeg = CreateJpeg(5600, true);
		var fujiHeader = "FUJI"u8.ToArray();
		var padding = Enumerable.Repeat(( byte ) 0x00, 128).ToArray();
		return fujiHeader.Concat(padding).Concat(jpeg).ToArray();
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
		var leadingJpeg = CreateJpeg(18869, false); // earlier JPEG should not be preferred
		var taggedJpeg = CreateJpeg(taggedLength, false);
		var totalLength = ( int ) taggedOffset + taggedLength + 16;
		var bytes = new byte[totalLength];

		// Simulate existing early JPEG in container data.
		Array.Copy(leadingJpeg, 0, bytes, 292, leadingJpeg.Length);

		// Embedded TIFF header at offset 304 (as seen in real sigma sample).
		const int tiffBase = 304;
		bytes[tiffBase] = 0x4D;
		bytes[tiffBase + 1] = 0x4D;
		bytes[tiffBase + 2] = 0x00;
		bytes[tiffBase + 3] = 0x2A;
		WriteUInt32BigEndian(bytes, tiffBase + 4, 8); // first IFD offset

		// IFD0: zero entries + pointer to IFD1 at relative offset 16
		WriteUInt16BigEndian(bytes, tiffBase + 8, 0);
		WriteUInt32BigEndian(bytes, tiffBase + 10, 16);

		// IFD1 at tiffBase + 16: Compression + Thumbnail Offset + Thumbnail Length
		var ifd1 = tiffBase + 16;
		WriteUInt16BigEndian(bytes, ifd1, 3);

		// Tag 0x0103 Compression, type SHORT(3), count 1, value 6
		WriteUInt16BigEndian(bytes, ifd1 + 2, 0x0103);
		WriteUInt16BigEndian(bytes, ifd1 + 4, 3);
		WriteUInt32BigEndian(bytes, ifd1 + 6, 1);
		WriteUInt16BigEndian(bytes, ifd1 + 10, 6);

		// Tag 0x0201 Thumbnail Offset (LONG)
		WriteUInt16BigEndian(bytes, ifd1 + 14, 0x0201);
		WriteUInt16BigEndian(bytes, ifd1 + 16, 4);
		WriteUInt32BigEndian(bytes, ifd1 + 18, 1);
		WriteUInt32BigEndian(bytes, ifd1 + 22, taggedOffset);

		// Tag 0x0202 Thumbnail Length (LONG)
		WriteUInt16BigEndian(bytes, ifd1 + 26, 0x0202);
		WriteUInt16BigEndian(bytes, ifd1 + 28, 4);
		WriteUInt32BigEndian(bytes, ifd1 + 30, 1);
		WriteUInt32BigEndian(bytes, ifd1 + 34, ( uint ) taggedLength);

		// next IFD offset = 0
		WriteUInt32BigEndian(bytes, ifd1 + 38, 0);

		Array.Copy(taggedJpeg, 0, bytes, taggedOffset, taggedJpeg.Length);

		// Ensure X3F header at start so content-detection recognizes the format
		bytes[0] = ( byte ) 'F';
		bytes[1] = ( byte ) 'O';
		bytes[2] = ( byte ) 'V';
		bytes[3] = ( byte ) 'b';
		return bytes;
	}

	private static byte[] CreateX3FWithRelativeTaggedIfd1Preview(uint taggedOffset,
		int taggedLength)
	{
		const int tiffBase = 304;
		var relativeTaggedOffset = taggedOffset - tiffBase;
		var leadingJpeg = CreateJpeg(18869, false); // absolute scanner candidate
		var taggedJpeg = CreateJpeg(taggedLength, false); // tagged candidate
		var totalLength = ( int ) taggedOffset + taggedLength + 16;
		var bytes = new byte[totalLength];

		Array.Copy(leadingJpeg, 0, bytes, 292, leadingJpeg.Length);

		// Embedded TIFF header at offset 304 (big-endian)
		bytes[tiffBase] = 0x4D;
		bytes[tiffBase + 1] = 0x4D;
		bytes[tiffBase + 2] = 0x00;
		bytes[tiffBase + 3] = 0x2A;
		WriteUInt32BigEndian(bytes, tiffBase + 4, 8);

		// IFD0 -> next IFD points to IFD1
		WriteUInt16BigEndian(bytes, tiffBase + 8, 0);
		WriteUInt32BigEndian(bytes, tiffBase + 10, 16);

		var ifd1 = tiffBase + 16;
		WriteUInt16BigEndian(bytes, ifd1, 3);

		// Compression=6
		WriteUInt16BigEndian(bytes, ifd1 + 2, 0x0103);
		WriteUInt16BigEndian(bytes, ifd1 + 4, 3);
		WriteUInt32BigEndian(bytes, ifd1 + 6, 1);
		WriteUInt16BigEndian(bytes, ifd1 + 10, 6);

		// 0x0201 uses TIFF-base-relative offset here (real edge-case)
		WriteUInt16BigEndian(bytes, ifd1 + 14, 0x0201);
		WriteUInt16BigEndian(bytes, ifd1 + 16, 4);
		WriteUInt32BigEndian(bytes, ifd1 + 18, 1);
		WriteUInt32BigEndian(bytes, ifd1 + 22, relativeTaggedOffset);

		WriteUInt16BigEndian(bytes, ifd1 + 26, 0x0202);
		WriteUInt16BigEndian(bytes, ifd1 + 28, 4);
		WriteUInt32BigEndian(bytes, ifd1 + 30, 1);
		WriteUInt32BigEndian(bytes, ifd1 + 34, ( uint ) taggedLength);

		WriteUInt32BigEndian(bytes, ifd1 + 38, 0);
		Array.Copy(taggedJpeg, 0, bytes, taggedOffset, taggedJpeg.Length);

		// Ensure X3F header at start so content-detection recognizes the format
		bytes[0] = ( byte ) 'F';
		bytes[1] = ( byte ) 'O';
		bytes[2] = ( byte ) 'V';
		bytes[3] = ( byte ) 'b';
		return bytes;
	}

	[TestMethod]
	public async Task LightweightContainerPreviewExtractor_PrefersIptcCandidate()
	{
		var bytes = CreateContainerWithTwoJpegsPreferIptc();
		var selectorStorage = CreateSelectorStorage(bytes, RawPathFff, false, out var tempStorage);
		var extractor = new LightweightContainerPreviewExtractor(new FakeIWebLogger(),
			selectorStorage);

		var result = await extractor.TryExtract(RawPathFff, OutputPath);

		Assert.IsTrue(result, "Expected JPEG extraction from FFF container");
		Assert.IsTrue(tempStorage.ExistFile(OutputPath), "Expected output preview file");

		await using var output = tempStorage.ReadStream(OutputPath);
		Assert.IsGreaterThan(0, output.Length, "Output should contain JPEG bytes");
		// Smaller IPTC candidate should be preferred over larger non-IPTC candidate.
		Assert.IsLessThan(8200, output.Length,
			"Expected IPTC-scored candidate to be selected instead of largest-only candidate");
	}

	[TestMethod]
	public async Task EmbeddedRawThumbnailService_RoutesX3fToLightweightExtractor()
	{
		var bytes = CreateContainerWithTwoJpegsPreferIptcWithX3FHeader();
		var selectorStorage = CreateSelectorStorage(bytes, RawPathX3F, false, out var tempStorage);
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger(), selectorStorage);

		var result = await service.TryExtractPreview(RawPathX3F, OutputPath);

		Assert.IsTrue(result, "Expected service route for .x3f via lightweight extractor");
		Assert.IsTrue(tempStorage.ExistFile(OutputPath), "Expected output preview file");
	}

	[TestMethod]
	public async Task EmbeddedRawThumbnailService_X3fWithIfd1Tags_UsesTaggedThumbnail()
	{
		const uint taggedOffset = 3816;
		const int taggedLength = 15345;
		var bytes = CreateX3FWithTaggedIfd1Preview(taggedOffset, taggedLength);
		var selectorStorage = CreateSelectorStorage(bytes, RawPathX3F, false, out var tempStorage);
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger(), selectorStorage);

		var result = await service.TryExtractPreview(RawPathX3F, OutputPath);

		Assert.IsTrue(result, "Expected tagged IFD1 thumbnail extraction for .x3f");
		Assert.IsTrue(tempStorage.ExistFile(OutputPath), "Expected output preview file");

		await using var output = tempStorage.ReadStream(OutputPath);
		Assert.AreEqual(taggedLength, output.Length,
			"Expected output length to match IFD1 tagged thumbnail length");
	}

	[TestMethod]
	public async Task EmbeddedRawThumbnailService_X3fWithRelativeIfd1Offset_UsesTaggedThumbnail()
	{
		const uint taggedOffset = 3816;
		const int taggedLength = 15345;
		var bytes = CreateX3FWithRelativeTaggedIfd1Preview(taggedOffset, taggedLength);
		var selectorStorage = CreateSelectorStorage(bytes, RawPathX3F, false, out var tempStorage);
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger(), selectorStorage);

		var result = await service.TryExtractPreview(RawPathX3F, OutputPath);

		Assert.IsTrue(result, "Expected tagged IFD1 relative-offset thumbnail extraction");
		Assert.IsTrue(tempStorage.ExistFile(OutputPath), "Expected output preview file");

		await using var output = tempStorage.ReadStream(OutputPath);
		Assert.AreEqual(taggedLength, output.Length,
			"Expected output length to match relative IFD1 tagged thumbnail length");
	}

	[TestMethod]
	public async Task EmbeddedRawThumbnailService_RoutesRafToRafExtractor()
	{
		var bytes = CreateRafContainerWithIptcJpeg();
		var selectorStorage = CreateSelectorStorage(bytes, RawPathRaf, true, out var tempStorage);
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger(), selectorStorage);

		var result = await service.TryExtractPreview(RawPathRaf, OutputPath);

		Assert.IsTrue(result, "Expected service route for .raf via RAF extractor");
		Assert.IsTrue(tempStorage.ExistFile(OutputPath), "Expected output preview file");
	}
}
