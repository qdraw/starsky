using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;
using starsky.foundation.thumbnailgeneration.Models;
using starskytest.FakeMocks;
using VerifyMSTest;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.Shared;

[TestClass]
public class SharedGenerateTest : VerifyBase
{
	[TestMethod]
	[DataRow(true, 1)]
	[DataRow(false, 0)]
	public async Task GenerateThumbnail_LogsConditionalExpectedMessage(
		bool errorLog, int expectedErrorCount)
	{
		// Arrange
		var fakeLogger = new FakeIWebLogger();
		var storage = new FakeIStorage(["/"],
			["/test.jpg"]);
		var fakeSelectorStorage = new FakeSelectorStorage(storage);
		var sharedGenerate = new SharedGenerate(fakeSelectorStorage, fakeLogger);

		const string singleSubPath = "/test.jpg";
		const string fileHash = "test-hash";
		var errorMessage = errorLog ? "Some other error" : "Not supported";
		const ThumbnailImageFormat imageFormat = ThumbnailImageFormat.jpg;

		var largeImageResult = new GenerationResultModel
		{
			Success = false,
			ErrorLog = errorLog,
			ErrorMessage = errorMessage,
			FileHash = "test",
			SubPath = "/test.jpg",
			ToGenerate = false,
			IsNotFound = false,
			Size = ThumbnailSize.Large,
			ImageFormat = ThumbnailImageFormat.jpg
		};
		var thumbnailSizes = new List<ThumbnailSize> { ThumbnailSize.Large };

		// Act
		await sharedGenerate.GenerateThumbnail(
			(_, _, _, _) =>
				Task.FromResult(largeImageResult),
			_ => true,
			singleSubPath, fileHash, imageFormat, thumbnailSizes);

		var errorMessages = fakeLogger.TrackedExceptions.Count(p => p.Item2?.Contains(
			SharedGenerate.PrefixGenerateThumbnailErrorMessage) == true);

		// when the errorLog is true, the error message should be logged
		Assert.AreEqual(expectedErrorCount, errorMessages);
	}

	[TestMethod]
	public async Task GenerateThumbnail_Verify()
	{
		// Arrange
		var fakeLogger = new FakeIWebLogger();
		var storage = new FakeIStorage(["/"],
			["/test.jpg"]);
		var fakeSelectorStorage = new FakeSelectorStorage(storage);
		var sharedGenerate = new SharedGenerate(fakeSelectorStorage, fakeLogger);

		const string singleSubPath = "/test.jpg";
		const string fileHash = "test-hash";
		const string errorMessage = "Some error occurred";
		const ThumbnailImageFormat imageFormat = ThumbnailImageFormat.jpg;

		var largeImageResult = new GenerationResultModel
		{
			Success = false,
			ErrorLog = true,
			ErrorMessage = errorMessage,
			Size = ThumbnailSize.Large,
			FileHash = fileHash,
			SubPath = singleSubPath,
			ToGenerate = false,
			IsNotFound = false,
			ImageFormat = ThumbnailImageFormat.unknown
		};
		var thumbnailSizes = new List<ThumbnailSize> { ThumbnailSize.Large };

		// Act
		var result = await sharedGenerate.GenerateThumbnail(
			(_, _, _, _) =>
				Task.FromResult(largeImageResult),
			_ => true,
			singleSubPath, fileHash, imageFormat, thumbnailSizes);

		await Verify(result);
	}

	[TestMethod]
	public async Task GenerateThumbnail_SomeSizesExist_AttemptsMissingGeneration()
	{
		// Arrange - Simulate scenario where Small and TinyMeta already exist
		// but ExtraLarge and Large don't
		var fakeLogger = new FakeIWebLogger();
		
		var combinedStorage = new FakeIStorage(["/"], 
			["/test.jpg", "/6GNRDDAMA2YQFRFAGRPH7XEOYY@300.jpg", "/6GNRDDAMA2YQFRFAGRPH7XEOYY@meta.jpg"],
			new List<byte[]> { new byte[1000], new byte[100], new byte[100] });
		
		var fakeSelectorStorage = new FakeSelectorStorage(combinedStorage);
		var sharedGenerate = new SharedGenerate(fakeSelectorStorage, fakeLogger);

		const string singleSubPath = "/test.jpg";
		const string fileHash = "6GNRDDAMA2YQFRFAGRPH7XEOYY";
		const ThumbnailImageFormat imageFormat = ThumbnailImageFormat.jpg;
		
		var generationCallCount = 0;
		var generatedSizes = new List<ThumbnailSize>();
		
		Task<GenerationResultModel> MockResizeThumbnailFromSourceImage(
			ThumbnailSize biggestThumbnailSize, string _, string __, ThumbnailImageFormat ___)
		{
			generationCallCount++;
			generatedSizes.Add(biggestThumbnailSize);
			return Task.FromResult(new GenerationResultModel
			{
				Success = true,
				Size = biggestThumbnailSize,
				FileHash = fileHash,
				SubPath = singleSubPath,
				ToGenerate = false,
				IsNotFound = false,
				ImageFormat = imageFormat,
				ErrorLog = false,
				ErrorMessage = string.Empty
			});
		}

		// Request all sizes: ExtraLarge, Large, Small, TinyMeta
		var thumbnailSizes = new List<ThumbnailSize> 
		{ 
			ThumbnailSize.ExtraLarge, 
			ThumbnailSize.Large, 
			ThumbnailSize.Small, 
			ThumbnailSize.TinyMeta 
		};

		// Act - Should attempt to generate missing sizes (ExtraLarge and Large) 
		// even though Small and TinyMeta already exist in storage
		var results = await sharedGenerate.GenerateThumbnail(
			MockResizeThumbnailFromSourceImage,
			_ => true,
			singleSubPath, fileHash, imageFormat, thumbnailSizes);

		// Assert
		var resultList = results.ToList();
		Assert.IsNotNull(resultList);
		
		// Should have results for all 4 sizes
		Assert.HasCount(4, resultList);
		
		// Generation should have been attempted for at least the largest missing size
		// (With the bug fix, it should attempt to generate, whereas before it would skip)
		Assert.IsGreaterThan(0, generationCallCount, 
			"Generator should have been called to create missing sizes");
		
		// Verify that ExtraLarge was in the generated sizes list (it's the largest)
		Assert.Contains(ThumbnailSize.ExtraLarge, generatedSizes,
			"ExtraLarge should have been generated from source image");
	}

	/// <summary>
	///     Validator for the GenerationResultModel
	/// </summary>
	/// <param name="result">List of items</param>
	private static async Task Verify(IEnumerable<GenerationResultModel> result)
	{
		await Verifier.Verify(result).DontScrubDateTimes();
	}
}
