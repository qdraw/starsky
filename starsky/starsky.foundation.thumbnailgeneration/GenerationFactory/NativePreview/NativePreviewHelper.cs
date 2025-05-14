using System;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.native.PreviewImageNative.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.NativePreview.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.NativePreview;

public class NativePreviewHelper(
	IPreviewImageNativeService previewService,
	IStorage storage,
	AppSettings appSettings,
	IReadMetaSubPathStorage readMeta,
	IFullFilePathExistsService existsService)
{
	public async Task<NativePreviewResult> NativePreviewImage(ThumbnailSize biggestThumbnailSize,
		string singleSubPath, string fileHash)
	{
		if ( !previewService.IsSupported() || !storage.ExistFile(singleSubPath) )
		{
			return new NativePreviewResult
			{
				IsSuccess = false,
				ErrorMessage = !previewService.IsSupported()
					? "Native service not supported"
					: "File does not exist"
			};
		}

		var metaData = await readMeta.ReadExifAndXmpFromFileAsync(singleSubPath);
		var width = ThumbnailNameHelper.GetSize(biggestThumbnailSize);
		var height =
			( int ) Math.Round(( double ) metaData!.ImageHeight / metaData.ImageWidth * width);

		var (_, fullFilePath, useTempStorageForInput, fileHashWithExtension) =
			await existsService.GetFullFilePath(singleSubPath, fileHash);

		var outputFullPath = Path.Combine(appSettings.TempFolder, fileHashWithExtension);
		var result =
			previewService.GeneratePreviewImage(fullFilePath, outputFullPath, width, height);

		existsService.CleanTemporaryFile(fileHashWithExtension, useTempStorageForInput);

		return new NativePreviewResult
		{
			IsSuccess = result,
			ResultPath = fileHashWithExtension,
			ResultPathType = SelectorStorage.StorageServices.Temporary
		};
	}

	public void CleanTemporaryFile(string resultResultPath,
		SelectorStorage.StorageServices? resultResultPathType)
	{
		throw new NotImplementedException();
	}
}
