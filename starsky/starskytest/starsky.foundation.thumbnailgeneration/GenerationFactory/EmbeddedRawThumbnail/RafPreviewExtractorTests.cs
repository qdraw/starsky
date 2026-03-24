using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class RafPreviewExtractorTests
{
	private const string InputSubPath = "/raw/test.raf";
	private const string OutputSubPath = "/tmp/output.jpg";

	public TestContext TestContext { get; set; } = null!;

	private static FakeSelectorStorageByType CreateSelectorStorage(byte[]? inputBytes,
		out FakeIStorage tempStorage)
	{
		var subPathStorage = inputBytes != null
			? new FakeIStorage(["/raw"], [InputSubPath], [inputBytes])
			: new FakeIStorage(["/raw"]);

		tempStorage = new FakeIStorage(["/tmp"]);
		var thumbnailStorage = new FakeIStorage();
		var hostStorage = new FakeIStorage();
		return new FakeSelectorStorageByType(subPathStorage, thumbnailStorage, hostStorage,
			tempStorage);
	}

	private static byte[] CreateMinimalJpeg(int size = 6000)
	{
		var jpeg = new byte[size];
		jpeg[0] = 0xFF;
		jpeg[1] = 0xD8;
		jpeg[2] = 0xFF;
		for ( var i = 3; i < size - 2; i++ )
		{
			jpeg[i] = 0x11;
		}

		jpeg[size - 2] = 0xFF;
		jpeg[size - 1] = 0xD9;
		return jpeg;
	}

	private static void WriteUInt32BigEndian(byte[] target, int offset, uint value)
	{
		target[offset] = ( byte ) ( value >> 24 );
		target[offset + 1] = ( byte ) ( value >> 16 );
		target[offset + 2] = ( byte ) ( value >> 8 );
		target[offset + 3] = ( byte ) value;
	}

	private static byte[] CreateRafWithHeaderPreview(uint previewOffset, byte[] previewBytes)
	{
		const int headerSize = 0x5C;
		var totalLength = ( int ) previewOffset + previewBytes.Length;
		var bytes = new byte[Math.Max(totalLength, headerSize)];

		var signature = "FUJIFILMCCD-RAW "u8.ToArray();
		Array.Copy(signature, 0, bytes, 0, signature.Length);

		WriteUInt32BigEndian(bytes, 0x54, previewOffset);
		WriteUInt32BigEndian(bytes, 0x58, ( uint ) previewBytes.Length);

		Array.Copy(previewBytes, 0, bytes, previewOffset, previewBytes.Length);
		return bytes;
	}

	[TestMethod]
	public async Task TryExtract_WithValidRafHeaderPreview_ExtractsFromHeader()
	{
		var jpeg = CreateMinimalJpeg(7000);
		var raf = CreateRafWithHeaderPreview(148, jpeg);
		var selectorStorage = CreateSelectorStorage(raf, out var tempStorage);
		var extractor = new RafPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		Assert.IsTrue(result, "RAF header preview should be extracted");
		Assert.IsTrue(tempStorage.ExistFile(OutputSubPath), "Preview should be written to temp");

		using var stream = tempStorage.ReadStream(OutputSubPath);
		using var ms = new MemoryStream();
		await stream.CopyToAsync(ms, TestContext.CancellationToken);
		var written = ms.ToArray();
		Assert.HasCount(jpeg.Length,
			written,
			"Should write exact RAF header preview range");
		Assert.AreEqual(0xFF, written[0]);
		Assert.AreEqual(0xD8, written[1]);
	}

	[TestMethod]
	public async Task TryExtract_WithInvalidHeaderRange_FallsBackToScanner()
	{
		var jpeg = CreateMinimalJpeg(6500);
		var bytes = new byte[200 + jpeg.Length];
		var signature = Encoding.ASCII.GetBytes("FUJIFILMCCD-RAW ");
		Array.Copy(signature, 0, bytes, 0, signature.Length);

		// Invalid header metadata (range outside file) to force fallback path.
		WriteUInt32BigEndian(bytes, 0x54, 999_999);
		WriteUInt32BigEndian(bytes, 0x58, 20_000);

		Array.Copy(jpeg, 0, bytes, 200, jpeg.Length);

		var selectorStorage = CreateSelectorStorage(bytes, out var tempStorage);
		var extractor = new RafPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		Assert.IsTrue(result, "Fallback scanner should still find embedded JPEG");
		Assert.IsTrue(tempStorage.ExistFile(OutputSubPath));
	}

	[TestMethod]
	public async Task TryExtract_WithMissingFile_ReturnsFalse()
	{
		var selectorStorage = CreateSelectorStorage(null, out _);
		var extractor = new RafPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		Assert.IsFalse(result);
	}
}
