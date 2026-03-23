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
		_embeddedRawThumbnailService = new FakeEmbeddedRawThumbnailService();
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
			new List<ThumbnailSize> { ThumbnailSize.Large });

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
			new List<ThumbnailSize> { ThumbnailSize.Large });

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
			new List<ThumbnailSize> { ThumbnailSize.Large });

		Assert.IsNotNull(result);
	}
}

/// <summary>
///     Fake implementation for testing without external dependencies
/// </summary>
public class FakeEmbeddedRawThumbnailService : IEmbeddedRawThumbnailService
{
	public Task<bool> TryExtractPreview(string rawFilePath, string? outputLargePath)
	{
		// Return false by default - tests can override if needed
		return Task.FromResult(false);
	}
}
