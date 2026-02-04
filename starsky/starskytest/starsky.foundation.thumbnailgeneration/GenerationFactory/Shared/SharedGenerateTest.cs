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

	/// <summary>
	///     Validator for the GenerationResultModel
	/// </summary>
	/// <param name="result">List of items</param>
	private static async Task Verify(IEnumerable<GenerationResultModel> result)
	{
		await Verifier.Verify(result).DontScrubDateTimes();
	}
}
