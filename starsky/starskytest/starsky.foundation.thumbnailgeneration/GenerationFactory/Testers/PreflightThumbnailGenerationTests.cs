using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;

[TestClass]
public class PreflightThumbnailGenerationTests
{
	private readonly string _nonValidImageSubPath;
	private readonly PreflightThumbnailGeneration _sut;

	public PreflightThumbnailGenerationTests()
	{
		_nonValidImageSubPath = "/non-valid.jpg";
		var fakeStorage = new FakeIStorage(["/"],
			["/test.jpg", _nonValidImageSubPath],
			new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray(), Array.Empty<byte>() });

		_sut = new PreflightThumbnailGeneration(new FakeSelectorStorage(fakeStorage));
	}

	private static bool DelegateMockReturnsFalse(string? filename)
	{
		return false;
	}

	private static bool DelegateMockReturnsTrue(string? filename)
	{
		return true;
	}

	[TestMethod]
	public void Preflight_WithNoItems_ReturnsExpectedResults()
	{
		// Arrange
		const string subPath = "test.jpg";
		var results = _sut.Preflight(DelegateMockReturnsFalse,
			[],
			subPath, "hash", ThumbnailImageFormat.jpg);

		// Assert
		Assert.IsNotNull(results);
		Assert.AreEqual(2, results.Count);
		Assert.IsFalse(results[0].Success);
		Assert.AreEqual($"{PreflightThumbnailGeneration.NoCountErrorPrefix}{subPath}",
			results[0].ErrorMessage);
		Assert.IsFalse(results[1].Success);
		Assert.AreEqual($"{PreflightThumbnailGeneration.NoCountErrorPrefix}{subPath}",
			results[1].ErrorMessage);
	}

	[TestMethod]
	public void Preflight_ThumbnailImageFormatUnknown_ReturnsExpectedResults()
	{
		// Arrange
		const string subPath = "test.jpg";
		var results = _sut.Preflight(DelegateMockReturnsFalse,
			[ThumbnailSize.Large],
			subPath, "hash", ThumbnailImageFormat.unknown);

		// Assert
		Assert.IsNotNull(results);
		Assert.AreEqual(2, results.Count);
		Assert.IsFalse(results[0].Success);
		Assert.AreEqual($"{PreflightThumbnailGeneration.FormatUnknownPrefix}{subPath}",
			results[0].ErrorMessage);
		Assert.IsFalse(results[1].Success);
		Assert.AreEqual($"{PreflightThumbnailGeneration.FormatUnknownPrefix}{subPath}",
			results[1].ErrorMessage);
	}

	[TestMethod]
	public void Preflight_NonValid_Image()
	{
		// Arrange
		var results = _sut.Preflight(DelegateMockReturnsFalse,
			[ThumbnailSize.Large],
			// does not actually check if the image is valid
			_nonValidImageSubPath, "hash", ThumbnailImageFormat.jpg);

		// Assert
		Assert.IsNotNull(results);
		Assert.AreEqual(1, results.Count);
		Assert.IsTrue(results.All(p => !p.Success));
		Assert.AreEqual("not supported", results[0].ErrorMessage);
	}

	[TestMethod]
	public void Preflight_NotFound_Image()
	{
		// Arrange
		var results = _sut.Preflight(DelegateMockReturnsTrue,
			[ThumbnailSize.Large],
			"/not-found.jpg", "hash", ThumbnailImageFormat.jpg);

		// Assert
		Assert.IsNotNull(results);
		Assert.AreEqual(1, results.Count);
		Assert.IsTrue(results.All(p => !p.Success));
		Assert.AreEqual("File is not found", results[0].ErrorMessage);
	}

	[TestMethod]
	public void Preflight_HappyFlow()
	{
		// Arrange
		var results = _sut.Preflight(DelegateMockReturnsTrue,
			[ThumbnailSize.Large],
			_nonValidImageSubPath, "hash", ThumbnailImageFormat.jpg);

		// Assert
		Assert.IsNotNull(results);
		Assert.AreEqual(1, results.Count);
		Assert.IsTrue(results.All(p => p.Success));
	}

	[TestMethod]
	public void Preflight_ErrorLock()
	{
		// Arrange

		var fakeStorage = new FakeIStorage(["/"],
			["/test.jpg", ErrorLogItemFullPath.GetErrorLogItemFullPath("/test.jpg")],
			new List<byte[]> { CreateAnImageNoExif.Bytes.ToArray(), Array.Empty<byte>() });

		var sut = new PreflightThumbnailGeneration(new FakeSelectorStorage(fakeStorage));

		var results = sut.Preflight(DelegateMockReturnsTrue,
			[ThumbnailSize.Large],
			"/test.jpg", "hash", ThumbnailImageFormat.jpg);

		// Assert
		Assert.IsNotNull(results);
		Assert.AreEqual(1, results.Count);
		Assert.IsTrue(results.All(p => !p.Success));
		Assert.AreEqual("File already failed before", results[0].ErrorMessage);
	}
}
