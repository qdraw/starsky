using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.imageclassification.Helpers;

namespace starskytest.starsky.foundation.imageclassification.Helpers;

[TestClass]
public sealed class ImageClassificationTagMergeHelperTests
{
	[TestMethod]
	public void MergeSuggestedTags_ShouldFilterTagsRejectedAndExistingSuggested_CaseInsensitive()
	{
		var fileIndexItem = new FileIndexItem
		{
			Tags = "Cat, sky",
			RejectedTags = "car",
			SuggestedTags = "tree"
		};

		var candidates = new[] { "cat", "Car", "tree", "mountain", "Sky", "night" };
		var (merged, addedCount) = ImageClassificationTagMergeHelper.MergeSuggestedTags(fileIndexItem,
			candidates);

		Assert.AreEqual(2, addedCount);
		Assert.Contains("mountain", merged);
		Assert.Contains("night", merged);
		Assert.DoesNotContain(merged, "cat");
		Assert.DoesNotContain(merged, "car");
	}
}

