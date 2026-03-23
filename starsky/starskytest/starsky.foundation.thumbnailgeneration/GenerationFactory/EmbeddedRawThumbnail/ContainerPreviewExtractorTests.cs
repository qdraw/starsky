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
public class ContainerPreviewExtractorTests
{
	private const string RawPathFff = "/raw/test.fff";
	private const string RawPathX3f = "/raw/test.x3f";
	private const string RawPathRaf = "/raw/test.raf";
	private const string OutputPath = "/tmp/preview.jpg";

	private static FakeSelectorStorageByType CreateSelectorStorage(byte[] bytes,
		string filePath,
		bool includeInTemp,
		out FakeIStorage tempStorage)
	{
		FakeIStorage subPathStorage;
		subPathStorage = new FakeIStorage(
			outputSubPathFolders: ["/raw"],
			outputSubPathFiles: [filePath],
			byteListSource: [bytes]);

		tempStorage = includeInTemp
			? new FakeIStorage(
				outputSubPathFolders: ["/tmp", "/raw"],
				outputSubPathFiles: [filePath],
				byteListSource: [bytes])
			: new FakeIStorage(outputSubPathFolders: ["/tmp"]);

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

		return leading
			.Concat(largerNoIptc)
			.Concat(middle)
			.Concat(smallerWithIptc)
			.ToArray();
	}

	private static byte[] CreateRafContainerWithIptcJpeg()
	{
		var jpeg = CreateJpeg(5600, true);
		var fujiHeader = "FUJI"u8.ToArray();
		var padding = Enumerable.Repeat(( byte ) 0x00, 128).ToArray();
		return fujiHeader.Concat(padding).Concat(jpeg).ToArray();
	}

	[TestMethod]
	public async Task LightweightContainerPreviewExtractor_PrefersIptcCandidate()
	{
		var bytes = CreateContainerWithTwoJpegsPreferIptc();
		var selectorStorage = CreateSelectorStorage(bytes, RawPathFff, includeInTemp: false, out var tempStorage);
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
		var bytes = CreateContainerWithTwoJpegsPreferIptc();
		var selectorStorage = CreateSelectorStorage(bytes, RawPathX3f, includeInTemp: false, out var tempStorage);
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger(), selectorStorage);

		var result = await service.TryExtractPreview(RawPathX3f, OutputPath);

		Assert.IsTrue(result, "Expected service route for .x3f via lightweight extractor");
		Assert.IsTrue(tempStorage.ExistFile(OutputPath), "Expected output preview file");
	}

	[TestMethod]
	public async Task EmbeddedRawThumbnailService_RoutesRafToRafExtractor()
	{
		var bytes = CreateRafContainerWithIptcJpeg();
		var selectorStorage = CreateSelectorStorage(bytes, RawPathRaf, includeInTemp: true, out var tempStorage);
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger(), selectorStorage);

		var result = await service.TryExtractPreview(RawPathRaf, OutputPath);

		Assert.IsTrue(result, "Expected service route for .raf via RAF extractor");
		Assert.IsTrue(tempStorage.ExistFile(OutputPath), "Expected output preview file");
	}
}

