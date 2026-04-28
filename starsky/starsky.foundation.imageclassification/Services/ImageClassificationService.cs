using starsky.foundation.database.Models;
using starsky.foundation.imageclassification.Helpers;
using starsky.foundation.imageclassification.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.imageclassification.Services;

[Service(typeof(IImageClassificationService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ImageClassificationService(
	IOllamaService ollamaService,
	IMetaUpdateService metaUpdateService,
	AppSettings appSettings) : IImageClassificationService
{
	public async Task<OllamaCommandResult> ClassifyAndUpdateAsync(FileIndexItem fileIndexItem,
		CancellationToken cancellationToken = default)
	{
		if ( fileIndexItem.IsDirectory == true || string.IsNullOrWhiteSpace(fileIndexItem.FilePath) )
		{
			return OllamaCommandResult.Failed("Only files with valid file paths can be classified");
		}

		var fullFilePath = appSettings.DatabasePathToFilePath(fileIndexItem.FilePath);
		var inferenceResult = await ollamaService.InferTagsAsync(fullFilePath, cancellationToken);
		if ( !inferenceResult.Success )
		{
			return inferenceResult;
		}

		var candidateTags = ParseTags(inferenceResult.Output);
		var (mergedSuggestedTags, _) = ImageClassificationTagMergeHelper.MergeSuggestedTags(fileIndexItem,
			candidateTags);

		fileIndexItem.SuggestedTags = mergedSuggestedTags;
		fileIndexItem.ImageClassificationModel = appSettings.OllamaModel;
		fileIndexItem.ImageClassificationGeneratedAt = DateTime.UtcNow;

		var changedNames = new Dictionary<string, List<string>>
		{
			[fileIndexItem.FilePath] =
			[
				nameof(FileIndexItem.SuggestedTags).ToLowerInvariant(),
				nameof(FileIndexItem.ImageClassificationModel).ToLowerInvariant(),
				nameof(FileIndexItem.ImageClassificationGeneratedAt).ToLowerInvariant()
			]
		};

		await metaUpdateService.UpdateAsync(changedNames,
			[fileIndexItem], null, false, false, 0);

		return inferenceResult;
	}

	internal static IEnumerable<string> ParseTags(string? output)
	{
		if ( string.IsNullOrWhiteSpace(output) )
		{
			return [];
		}

		return output.Split([',', '\n', '\r', ';'], StringSplitOptions.RemoveEmptyEntries)
			.Select(p => p.Trim())
			.Where(p => !string.IsNullOrWhiteSpace(p));
	}
}

