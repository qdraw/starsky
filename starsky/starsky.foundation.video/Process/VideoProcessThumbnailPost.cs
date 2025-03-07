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
	private readonly IStorage _tempStorage;

	public VideoProcessThumbnailPost(ISelectorStorage selectorStorage,
		AppSettings appSettings, IExifTool exifTool, IWebLogger logger,
		IThumbnailQuery thumbnailQuery)
	{
		_tempStorage = selectorStorage.Get(SelectorStorage.StorageServices.Temporary);
		var thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		var readMeta = new ReadMeta(_tempStorage,
			appSettings, null!, logger);
		_exifCopy = new ExifCopy(_tempStorage,
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
		const string extension = "jpg";
		var parentPath = FilenamesHelper.GetParentPath(subPath);
		if ( parentPath == "/" )
		{
			return $"/{FilenamesHelper.GetFileNameWithoutExtension(subPath)}.{extension}";
		}

		return $"{parentPath}/" +
		       $"{FilenamesHelper.GetFileNameWithoutExtension(subPath)}.{extension}";
	}

	private async Task WriteStreamInFolderSubPathAsync(Stream stream, string subPath,
		string jpegInFolderSubPath)
	{
		await _tempStorage.WriteStreamAsync(stream, jpegInFolderSubPath);

		await _exifCopy.CopyExifPublish(subPath, jpegInFolderSubPath);
	}
}
