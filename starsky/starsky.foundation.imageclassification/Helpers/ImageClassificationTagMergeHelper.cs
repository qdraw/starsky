using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.imageclassification.Helpers;

public static class ImageClassificationTagMergeHelper
{
	public static (string mergedSuggestedTags, int addedCount) MergeSuggestedTags(
		FileIndexItem fileIndexItem,
		IEnumerable<string> candidateTags)
	{
		var existingSuggested = HashSetHelper.StringToHashSet(fileIndexItem.SuggestedTags ?? string.Empty);
		var combinedKnown = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach ( var tag in HashSetHelper.StringToHashSet(fileIndexItem.Tags ?? string.Empty) )
		{
			combinedKnown.Add(tag);
		}

		foreach ( var tag in HashSetHelper.StringToHashSet(fileIndexItem.RejectedTags ?? string.Empty) )
		{
			combinedKnown.Add(tag);
		}

		foreach ( var tag in existingSuggested )
		{
			combinedKnown.Add(tag);
		}

		var addedCount = 0;
		foreach ( var candidateTag in candidateTags )
		{
			if ( string.IsNullOrWhiteSpace(candidateTag) )
			{
				continue;
			}

			var trimmed = candidateTag.Trim();
			if ( combinedKnown.Contains(trimmed) )
			{
				continue;
			}

			existingSuggested.Add(trimmed);
			combinedKnown.Add(trimmed);
			addedCount++;
		}

		return (HashSetHelper.HashSetToString(existingSuggested.OrderBy(p => p).ToHashSet()), addedCount);
	}
}



