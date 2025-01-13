using System.Collections.Generic;
using System.Linq;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Testers;

public class PreflightThumbnailGeneration(ISelectorStorage selectorStorage)
{
	public delegate bool IsExtensionSupportedDelegate(string? filename);

	private readonly IStorage
		_storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

	private readonly IStorage
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

	public static List<ThumbnailSize> MapThumbnailSizes(
		List<GenerationResultModel> generationResults)
	{
		return generationResults.Where(p => p.ToGenerate).Select(p => p.Size).ToList();
	}

	public List<GenerationResultModel> Preflight(
		IsExtensionSupportedDelegate isExtensionSupportedDelegate,
		List<ThumbnailSize> thumbnailSizes, string subPath,
		string fileHash, ThumbnailImageFormat imageFormat)
	{
		if ( thumbnailSizes.Count < ThumbnailSizes.GetSizes(true).Count )
		{
			return ErrorGenerationResultModel.FailedResult(
				ThumbnailSizes.GetSizes(true),
				subPath, fileHash, false,
				$"thumbnailSizes.Count <= {ThumbnailSizes.GetSizes(true).Count}");
		}

		// eg. ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported
		var extensionSupported = isExtensionSupportedDelegate(subPath);
		var existsFile = _storage.ExistFile(subPath);
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
					Size = size,
					ImageFormat = imageFormat
				}).ToList();
		}

		// File is already tested
		if ( _storage.ExistFile(ErrorLogItemFullPath.GetErrorLogItemFullPath(subPath)) )
		{
			return thumbnailSizes.Select(size =>
				new GenerationResultModel
				{
					SubPath = subPath,
					FileHash = fileHash,
					Success = false,
					IsNotFound = false,
					ErrorMessage = "File already failed before",
					Size = size,
					ImageFormat = imageFormat
				}).ToList();
		}

		// check if sizes are already generated
		var existsGenerationResults =
			new ThumbnailExistsBySize(_thumbnailStorage).CheckIfExists(fileHash, subPath,
				thumbnailSizes, imageFormat);

		return existsGenerationResults;
	}
}
