using System;
using System.IO;
using starsky.foundation.native.PreviewImageNative.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.NativePreview.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.NativePreview;

public class NativePreviewHelper(
	IPreviewImageNativeService previewService,
	IStorage storage,
	AppSettings appSettings,
	IReadMetaSubPathStorage readMeta
)
{
	public NativePreviewResult NativePreviewImage(ThumbnailSize biggestThumbnailSize,
		string singleSubPath, string fileHash)
	{
		if ( !storage.ExistFile(singleSubPath) )
		{
			return new NativePreviewResult
			{
				IsSuccess = false, ErrorMessage = "File does not exist"
			};
		}

		var outputPath = Path.Combine(appSettings.TempFolder, fileHash + ".jpg");
		var result = previewService.GeneratePreviewImage(singleSubPath, outputPath,
			ThumbnailNameHelper.GetSize(biggestThumbnailSize), 0);
	}

	public void CleanTemporaryFile(string resultResultPath,
		SelectorStorage.StorageServices? resultResultPathType)
	{
		throw new NotImplementedException();
	}
}
