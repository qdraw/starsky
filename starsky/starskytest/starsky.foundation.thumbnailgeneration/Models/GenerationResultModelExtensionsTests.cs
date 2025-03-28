using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.Models;

namespace starskytest.starsky.foundation.thumbnailgeneration.Models;

[TestClass]
public class GenerationResultModelExtensionsTests
{
	[TestMethod]
	public void AddOrUpdateRange_BothNull_ReturnsEmptyList()
	{
		List<GenerationResultModel>? compositeResults = null;
		List<GenerationResultModel>? result = null;

		var updatedResults = compositeResults.AddOrUpdateRange(result);

		Assert.IsNotNull(updatedResults);
		Assert.AreEqual(0, updatedResults.Count);
	}

	[TestMethod]
	public void AddOrUpdateRange_CompositeResultsNull_ReturnsResult()
	{
		List<GenerationResultModel>? compositeResults = null;
		var result = new List<GenerationResultModel>
		{
			new() { FileHash = "hash1", Size = ThumbnailSize.TinyIcon }
		};

		var updatedResults = compositeResults.AddOrUpdateRange(result);

		Assert.AreEqual(1, updatedResults.Count);
		Assert.AreEqual("hash1", updatedResults[0].FileHash);
	}

	[TestMethod]
	public void AddOrUpdateRange_ResultNull_ReturnsCompositeResults()
	{
		var compositeResults = new List<GenerationResultModel>
		{
			new() { FileHash = "hash1", Size = ThumbnailSize.TinyIcon }
		};
		List<GenerationResultModel>? result = null;

		var updatedResults = compositeResults.AddOrUpdateRange(result);

		Assert.AreEqual(1, updatedResults.Count);
		Assert.AreEqual("hash1", updatedResults[0].FileHash);
	}

	[TestMethod]
	public void AddOrUpdateRange_UpdatesExistingItem()
	{
		var compositeResults = new List<GenerationResultModel>
		{
			new() { FileHash = "hash1", Size = ThumbnailSize.TinyIcon, Success = false }
		};
		var result = new List<GenerationResultModel>
		{
			new() { FileHash = "hash1", Size = ThumbnailSize.TinyIcon, Success = true }
		};

		var updatedResults = compositeResults.AddOrUpdateRange(result);

		Assert.AreEqual(1, updatedResults.Count);
		Assert.AreEqual("hash1", updatedResults[0].FileHash);
		Assert.IsTrue(updatedResults[0].Success);
	}

	[TestMethod]
	public void AddOrUpdateRange_AddsNewItem()
	{
		var compositeResults = new List<GenerationResultModel>
		{
			new() { FileHash = "hash1", Size = ThumbnailSize.TinyIcon }
		};
		var result = new List<GenerationResultModel>
		{
			new() { FileHash = "hash2", Size = ThumbnailSize.TinyMeta }
		};

		var updatedResults = compositeResults.AddOrUpdateRange(result);

		Assert.AreEqual(2, updatedResults.Count);
		Assert.IsTrue(updatedResults.Exists(x => x.FileHash == "hash1"));
		Assert.IsTrue(updatedResults.Exists(x => x.FileHash == "hash2"));
	}

	[TestMethod]
	public void AddOrUpdateRange_NullableCompositeResults_ReturnsResult()
	{
		IEnumerable<GenerationResultModel>? compositeResults = null;
		var result = new List<GenerationResultModel>
		{
			new() { FileHash = "hash1", Size = ThumbnailSize.TinyIcon }
		};

		var updatedResults = compositeResults.AddOrUpdateRange(result);

		Assert.AreEqual(1, updatedResults.Count);
		Assert.AreEqual("hash1", updatedResults[0].FileHash);
	}

	[TestMethod]
	public void AddOrUpdateRange_NullableResult_ReturnsCompositeResults()
	{
		var compositeResults = new List<GenerationResultModel>
		{
			new() { FileHash = "hash1", Size = ThumbnailSize.TinyIcon }
		};
		IEnumerable<GenerationResultModel>? result = null;

		var updatedResults = compositeResults.AddOrUpdateRange(result);

		Assert.AreEqual(1, updatedResults.Count);
		Assert.AreEqual("hash1", updatedResults[0].FileHash);
	}
}
