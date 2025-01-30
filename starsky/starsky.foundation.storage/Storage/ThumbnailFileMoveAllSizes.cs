using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Storage;

public sealed class ThumbnailFileMoveAllSizes
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _thumbnailStorage;

	public ThumbnailFileMoveAllSizes(IStorage thumbnailStorage, AppSettings appSettings)
	{
		_thumbnailStorage = thumbnailStorage;
		_appSettings = appSettings;
	}

	public void FileMove(string oldFileHash, string newHashCode)
	{
		_thumbnailStorage.FileMove(
			ThumbnailNameHelper.Combine(oldFileHash, ThumbnailSize.Large,
				_appSettings.ThumbnailImageFormat),
			ThumbnailNameHelper.Combine(newHashCode, ThumbnailSize.Large,
				_appSettings.ThumbnailImageFormat));
		_thumbnailStorage.FileMove(
			ThumbnailNameHelper.Combine(oldFileHash, ThumbnailSize.Small,
				_appSettings.ThumbnailImageFormat),
			ThumbnailNameHelper.Combine(newHashCode, ThumbnailSize.Small,
				_appSettings.ThumbnailImageFormat));
		_thumbnailStorage.FileMove(
			ThumbnailNameHelper.Combine(oldFileHash, ThumbnailSize.ExtraLarge,
				_appSettings.ThumbnailImageFormat),
			ThumbnailNameHelper.Combine(newHashCode, ThumbnailSize.ExtraLarge,
				_appSettings.ThumbnailImageFormat));
		_thumbnailStorage.FileMove(
			ThumbnailNameHelper.Combine(oldFileHash, ThumbnailSize.TinyMeta,
				_appSettings.ThumbnailImageFormat),
			ThumbnailNameHelper.Combine(newHashCode, ThumbnailSize.TinyMeta,
				_appSettings.ThumbnailImageFormat));
	}
}
