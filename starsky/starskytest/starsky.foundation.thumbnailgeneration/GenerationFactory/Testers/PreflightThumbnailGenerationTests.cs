using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;

[TestClass]
public class PreflightThumbnailGenerationTests
{
	private readonly PreflightThumbnailGeneration _sut;

	public PreflightThumbnailGenerationTests()
	{
		_sut = new PreflightThumbnailGeneration(new FakeSelectorStorage());
	}

	private static bool DelegateMock(string? filename)
	{
		return true;
	}

	[TestMethod]
	public void Preflight_WithNoItems_ReturnsExpectedResults()
	{
		// Arrange
		const string subPath = "test.jpg";
		var results = _sut.Preflight(DelegateMock,
			[],
			subPath, "hash", ThumbnailImageFormat.jpg);

		// Assert
		Assert.IsNotNull(results);
		Assert.AreEqual(2, results.Count);
		Assert.IsFalse(results[0].Success);
		Assert.AreEqual($"{PreflightThumbnailGeneration.NoCountErrorPrefix}{subPath}", results[0].ErrorMessage);
		Assert.IsFalse(results[1].Success);
		Assert.AreEqual($"{PreflightThumbnailGeneration.NoCountErrorPrefix}{subPath}", results[1].ErrorMessage);
	}

	[TestMethod]
	public void Preflight_ThumbnailImageFormatUnknown_ReturnsExpectedResults()
	{
		// Arrange
		const string subPath = "test.jpg";
		var results = _sut.Preflight(DelegateMock,
			[ThumbnailSize.Large],
			subPath, "hash", ThumbnailImageFormat.unknown);

		// Assert
		Assert.IsNotNull(results);
		Assert.AreEqual(2, results.Count);
		Assert.IsFalse(results[0].Success);
		Assert.AreEqual($"{PreflightThumbnailGeneration.FormatUnknownPrefix}{subPath}", results[0].ErrorMessage);
		Assert.IsFalse(results[1].Success);
		Assert.AreEqual($"{PreflightThumbnailGeneration.FormatUnknownPrefix}{subPath}", results[1].ErrorMessage);
	}
}
