using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using starsky.foundation.native.PreviewImageNative.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.NativePreview.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.NativePreview;

public class NativePreviewHelper(
	IPreviewImageNativeService previewService,
	IStorage storage,
	IStorage tempStorage,
	AppSettings appSettings,
	IFullFilePathExistsService existsService,
	IWebLogger logger)
{
	internal const string ErrorFileDoesNotExist = "File does not exist";
	internal const string ErrorNativeServiceNotSupported = "Native service not supported";

	public async Task<NativePreviewResult> NativePreviewImage(ThumbnailSize biggestThumbnailSize,
		string singleSubPath, string fileHash)
	{
		var width = ThumbnailNameHelper.GetSize(biggestThumbnailSize);

		if ( !previewService.IsSupported(width) || !storage.ExistFile(singleSubPath) )
		{
			return new NativePreviewResult
			{
				IsSuccess = false,
				ErrorMessage = !previewService.IsSupported()
					? ErrorNativeServiceNotSupported
					: ErrorFileDoesNotExist
			};
		}

		var (_, fullFilePath, useTempStorageForInput, fileHashWithExtension) =
			await existsService.GetFullFilePath(singleSubPath, fileHash);

		var previewImageName = GetPreviewImageName(fileHash);
		var outputFullPath = Path.Combine(appSettings.TempFolder, previewImageName);
		var height = await GetImageHeight(fullFilePath);
		var result = previewService.GeneratePreviewImage(fullFilePath,
			outputFullPath, width, height);

		existsService.CleanTemporaryFile(fileHashWithExtension, useTempStorageForInput);

		return new NativePreviewResult
		{
			IsSuccess = result,
			ResultPath = previewImageName,
			ResultPathType = SelectorStorage.StorageServices.Temporary
		};
	}

	private static async Task<int> GetImageHeight(string filePath)
	{
		try
		{
			using var image = await Image.LoadAsync(filePath);
			return image.Height;
		}
		catch ( Exception )
		{
			return 0;
		}
	}

	private string GetPreviewImageName(string fileHash)
	{
		return $"{fileHash}.preview.{previewService.FileExtension()}";
	}

	public bool CleanTemporaryFile(string resultResultPath,
		SelectorStorage.StorageServices? resultResultPathType)
	{
		switch ( resultResultPathType )
		{
			case SelectorStorage.StorageServices.Temporary:
				return tempStorage.FileDelete(resultResultPath);
			default:
				logger.LogError(
					$"[NativePreviewHelper] CleanTemporaryFile: {resultResultPath} not deleted");
				break;
		}

		return false;
	}
}
