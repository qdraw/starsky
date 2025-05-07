using System.Collections.Generic;
using System.Linq;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory;

public static class ErrorGenerationResultModel
{
	internal static GenerationResultModel FailedResult(ThumbnailSize size,
		string subPath, string fileHash,
		bool existsFile,
		string errorMessage)
	{
		return new GenerationResultModel
		{
			SubPath = subPath,
			FileHash = fileHash,
			Success = false,
			IsNotFound = !existsFile,
			ErrorMessage = errorMessage,
			Size = size
		};
	}

	internal static List<GenerationResultModel> FailedResult(List<ThumbnailSize> sizes,
		string subPath, string fileHash,
		bool existsFile,
		string errorMessage)
	{
		return sizes.Select(size =>
			FailedResult(size, subPath, fileHash, existsFile, errorMessage)
		).ToList();
	}
}
