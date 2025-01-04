using System.Collections.Generic;
using System.Linq;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;

public class Preflight(IStorage storage)
{
	public List<GenerationResultModel>? Test(List<ThumbnailSize> thumbnailSizes, string subPath,
		string fileHash)
	{
		if ( thumbnailSizes.Count < ThumbnailSizes.GetSizes(true).Count )
		{
			return ErrorGenerationResultModel.FailedResult(
				ThumbnailSizes.GetSizes(true),
				subPath, fileHash, false,
				$"thumbnailSizes.Count <= {ThumbnailSizes.GetSizes(true).Count}");
		}

		var extensionSupported =
			ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(subPath);
		var existsFile = storage.ExistFile(subPath);
		if ( !extensionSupported || !existsFile )
		{
			return thumbnailSizes.Select(size =>
				new GenerationResultModel
				{
					SubPath = subPath,
					FileHash = fileHash,
					Success = false,
					IsNotFound = !existsFile,
					ErrorMessage = !extensionSupported ? "not supported" : "File is not found",
					Size = size
				}).ToList();
		}

		// File is already tested
		if ( storage.ExistFile(ErrorLogItemFullPath.GetErrorLogItemFullPath(subPath)) )
		{
			return thumbnailSizes.Select(size =>
				new GenerationResultModel
				{
					SubPath = subPath,
					FileHash = fileHash,
					Success = false,
					IsNotFound = false,
					ErrorMessage = "File already failed before",
					Size = size
				}).ToList();
		}

		return null;
	}
}
