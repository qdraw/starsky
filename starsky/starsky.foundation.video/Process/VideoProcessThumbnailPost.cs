using starsky.foundation.database.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.Process.Interfaces;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.writemeta.Services;

namespace starsky.foundation.video.Process;

[Service(typeof(IVideoProcessThumbnailPost), InjectionLifetime = InjectionLifetime.Scoped)]
public class VideoProcessThumbnailPost : IVideoProcessThumbnailPost
{
	private readonly ExifCopy _exifCopy;
	private readonly IStorage _storage;

	public VideoProcessThumbnailPost(ISelectorStorage selectorStorage,
		AppSettings appSettings, IExifTool exifTool, IWebLogger logger,
		IThumbnailQuery thumbnailQuery)
	{
		_storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		var thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		var readMeta = new ReadMeta(_storage,
			appSettings, null!, logger);
		_exifCopy = new ExifCopy(_storage,
			thumbnailStorage, exifTool, readMeta, thumbnailQuery, logger);
	}

	public async Task<VideoResult> PostPrepThumbnail(VideoResult runResult,
		Stream stream,
		string subPath)
	{
		if ( !runResult.IsSuccess )
		{
			return runResult;
		}

		var jpegInFolderSubPath = GetJpegInFolderSubPath(subPath);
		await WriteStreamInFolderSubPathAsync(stream, subPath, jpegInFolderSubPath);

		return new VideoResult(true, jpegInFolderSubPath);
	}

	private static string GetJpegInFolderSubPath(string subPath)
	{
		return $"{FilenamesHelper.GetParentPath(subPath)}/" +
		       $"{FilenamesHelper.GetFileNameWithoutExtension(subPath)}.jpg";
	}

	private async Task WriteStreamInFolderSubPathAsync(Stream stream, string subPath,
		string jpegInFolderSubPath)
	{
		await _storage.WriteStreamAsync(stream, jpegInFolderSubPath);

		await _exifCopy.CopyExifPublish(subPath, jpegInFolderSubPath);
	}
}
