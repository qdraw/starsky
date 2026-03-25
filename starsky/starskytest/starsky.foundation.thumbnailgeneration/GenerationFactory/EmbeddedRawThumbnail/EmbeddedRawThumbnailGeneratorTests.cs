using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class EmbeddedRawThumbnailGeneratorTests
{
	private IEmbeddedRawThumbnailService _embeddedRawThumbnailService = null!;
	private EmbeddedRawThumbnailGenerator _generator = null!;
	private IWebLogger _logger = null!;
	private ISelectorStorage _selectorStorage = null!;

	[TestInitialize]
	public void Initialize()
	{
		_logger = new FakeIWebLogger();
		_selectorStorage = new FakeSelectorStorage();
		_embeddedRawThumbnailService =
			new FakeEmbeddedRawThumbnailService(new FakeSelectorStorage());
		_generator = new EmbeddedRawThumbnailGenerator(_selectorStorage,
			_embeddedRawThumbnailService,
			_logger);
	}

	[TestMethod]
	public async Task GenerateThumbnail_WithDngFile_ReturnsResults()
	{
		// This is an integration test that requires actual RAW files
		// For unit testing, we use the FakeEmbeddedRawThumbnailService
		var result = await _generator.GenerateThumbnail(
			"/test/image.dng",
			"test_hash",
			ThumbnailImageFormat.jpg,
			[ThumbnailSize.Large]);

		// Result should be enumerable
		Assert.IsNotNull(result);
	}

	[TestMethod]
	public async Task GenerateThumbnail_WithUnsupportedFormat_ReturnsEmpty()
	{
		var result = await _generator.GenerateThumbnail(
			"/test/image.jpg",
			"test_hash",
			ThumbnailImageFormat.jpg,
			[ThumbnailSize.Large]);

		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	[DataRow("arw")]
	[DataRow("cr2")]
	[DataRow("cr3")]
	[DataRow("dng")]
	[DataRow("nef")]
	[DataRow("raf")]
	[DataRow("fff")]
	[DataRow("x3f")]
	public async Task GenerateThumbnail_WithSupportedFormat_ProcessesFile(string extension)
	{
		var result = await _generator.GenerateThumbnail(
			$"/test/image.{extension}",
			"test_hash",
			ThumbnailImageFormat.jpg,
			[ThumbnailSize.Large]);

		Assert.IsNotNull(result);
	}

	[TestMethod]
	[DataRow(true)]
	[DataRow(false)]
	public async Task GenerateThumbnail_WhenPreviewExtracted_ProducesResult(bool tryExtractStatus)
	{
		var selectorStorage = new FakeSelectorStorage(
			new FakeIStorage(["/"], ["/test/image.dng"])
		);
		var mockService = new FakeEmbeddedRawThumbnailService(selectorStorage,
			[], tryExtractStatus);
		var logger = new FakeIWebLogger();

		var generator = new EmbeddedRawThumbnailGenerator(selectorStorage, mockService, logger);

		var results = ( await generator.GenerateThumbnail(
			"/test/image.dng",
			"hash1",
			ThumbnailImageFormat.jpg,
			[ThumbnailSize.Large]) ).ToList();

		Assert.IsNotNull(results);
		Assert.IsNotEmpty(results);
		Assert.IsTrue(results[0].ErrorLog);
		Assert.Contains(
			tryExtractStatus
				? "Failed to extract preview files"
				: "No embedded preview found in RAW file", results[0].ErrorMessage!);
	}

	[TestMethod]
	public async Task GenerateThumbnail_WhenPreviewExtracted_Exception()
	{
		var selectorStorage = new FakeSelectorStorage(
			new FakeIStorage(["/"], ["/test/image.dng"])
		);
		var mockService = new FakeEmbeddedRawThumbnailService(selectorStorage,
			[], true, new Exception("EXCEPTION"));
		var logger = new FakeIWebLogger();

		var generator = new EmbeddedRawThumbnailGenerator(selectorStorage, mockService, logger);

		var results = ( await generator.GenerateThumbnail(
			"/test/image.dng",
			"hash1",
			ThumbnailImageFormat.jpg,
			[ThumbnailSize.Large]) ).ToList();

		Assert.IsNotNull(results);
		Assert.IsNotEmpty(results);
		Assert.IsTrue(results[0].ErrorLog);
		Assert.Contains(
			"EXCEPTION", results[0].ErrorMessage!);
	}
}
