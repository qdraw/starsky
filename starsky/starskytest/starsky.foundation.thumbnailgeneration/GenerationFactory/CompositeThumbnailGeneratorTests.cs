using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory;

[TestClass]
public class CompositeThumbnailGeneratorTests
{
	private readonly FakeIWebLogger _logger;
	private readonly CompositeThumbnailGenerator _compositeGenerator;
	private readonly List<FakeThumbnailGenerator> _generatorMocks;

	public CompositeThumbnailGeneratorTests()
	{
		_logger = new FakeIWebLogger();
		_generatorMocks =
		[
			new FakeThumbnailGenerator(),
			new FakeThumbnailGenerator()
		];
		_compositeGenerator = new CompositeThumbnailGenerator(
			_generatorMocks.Cast<IThumbnailGenerator>().ToList(), _logger);
	}

	[TestMethod]
	[DataRow(true, true, true)]
	[DataRow(false, true, true)]
	[DataRow(false, false, false)]
	public async Task GenerateThumbnail_VariousScenarios_ReturnsExpectedResults(bool firstSuccess,
		bool secondSuccess, bool expectedSuccess)
	{
		// Arrange
		const string singleSubPath = "test.jpg";
		const string fileHash = "hash";
		const ThumbnailImageFormat imageFormat = ThumbnailImageFormat.jpg;
		var thumbnailSizes = new List<ThumbnailSize> { ThumbnailSize.Small };

		var firstResults = new List<GenerationResultModel> { new() { Success = firstSuccess } };

		var secondResults = new List<GenerationResultModel> { new() { Success = secondSuccess } };

		_generatorMocks[0].SetResults(firstResults);
		_generatorMocks[1].SetResults(secondResults);

		// Act
		var results =
			await _compositeGenerator.GenerateThumbnail(singleSubPath, fileHash, imageFormat,
				thumbnailSizes);

		// Assert
		Assert.AreEqual(expectedSuccess, results.All(r => r.Success));
	}

	[TestMethod]
	[DataRow(true)]
	[DataRow(false)]
	public async Task GenerateThumbnail_GeneratorThrowsException_LogsError(bool secondSuccess)
	{
		// Arrange
		const string singleSubPath = "test.jpg";
		const string fileHash = "hash";
		const ThumbnailImageFormat imageFormat = ThumbnailImageFormat.jpg;
		var thumbnailSizes = new List<ThumbnailSize> { ThumbnailSize.Small };

		_generatorMocks[0].SetException(new Exception("Test exception"));

		var secondResults = new List<GenerationResultModel> { new() { Success = secondSuccess } };

		_generatorMocks[1].SetResults(secondResults);

		// Act
		var results =
			await _compositeGenerator.GenerateThumbnail(singleSubPath, fileHash, imageFormat,
				thumbnailSizes);

		// Assert
		Assert.IsTrue(
			_logger.TrackedExceptions.Any(log => log.Item2?.Contains("Test exception") == true));
		Assert.AreEqual(secondSuccess, results.All(r => r.Success));
	}
}
