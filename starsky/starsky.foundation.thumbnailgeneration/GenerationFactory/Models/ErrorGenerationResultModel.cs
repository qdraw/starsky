using System.Collections.Generic;
using System.Linq;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Models;

public static class ErrorGenerationResultModel
{
	internal static GenerationResultModel FailedResult(ThumbnailSize size,
		string subPath, string fileHash,
		bool existsFile,
		ThumbnailImageFormat imageFormat,
		bool errorLog,
		string errorMessage)
	{
		return new GenerationResultModel
		{
			SubPath = subPath,
			FileHash = fileHash,
			ToGenerate = false,
			Success = false,
			IsNotFound = !existsFile,
			ErrorLog = errorLog,
			ErrorMessage = errorMessage,
			Size = size,
			ImageFormat = imageFormat
		};
	}

	internal static List<GenerationResultModel> FailedResult(List<ThumbnailSize> sizes,
		string subPath, string fileHash,
		bool existsFile,
		ThumbnailImageFormat imageFormat,
		bool errorLog,
		string errorMessage)
	{
		return sizes.Select(size =>
			FailedResult(size, subPath, fileHash, existsFile, imageFormat, errorLog, errorMessage)
		).ToList();
	}
}
