using System.Collections.Generic;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;

public class ThumbnailExistsBySize(IStorage thumbnailStorage)
{
	public List<GenerationResultModel> CheckIfExists(string fileHash, string subPathReference,
		List<ThumbnailSize> thumbnailSizes,
		ThumbnailImageFormat imageFormat)
	{
		var generationResults = new List<GenerationResultModel>();
		foreach ( var thumbnailSize in thumbnailSizes )
		{
			ForEachAdd(generationResults, fileHash, thumbnailSize, imageFormat, subPathReference);
		}

		return generationResults;
	}

	private void ForEachAdd(List<GenerationResultModel> generationResults, string fileHash,
		ThumbnailSize thumbnailSize, ThumbnailImageFormat imageFormat, string subPathReference)
	{
		// add to field: toGenerate when not exists
		var toGenerate = !thumbnailStorage.ExistFile(
			ThumbnailNameHelper.Combine(fileHash, thumbnailSize, imageFormat));
		generationResults.Add(AddResult(fileHash, toGenerate, thumbnailSize, subPathReference,
			imageFormat));
	}

	private static GenerationResultModel AddResult(string fileHash, bool toGenerate,
		ThumbnailSize thumbnailSize,
		string subPathReference, ThumbnailImageFormat imageFormat)
	{
		return new GenerationResultModel
		{
			Success = true,
			FileHash = fileHash,
			Size = thumbnailSize,
			SubPath = subPathReference,
			ToGenerate = toGenerate,
			IsNotFound = false,
			ImageFormat = imageFormat,
			ErrorMessage = string.Empty
		};
	}
}
