using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.foundation.writemeta.Services;

[Service(typeof(IExifToolHostStorage), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ExifToolHostStorageService : IExifToolHostStorage
{
	private readonly ExifTool _exifTool;

	public ExifToolHostStorageService(ISelectorStorage selectorStorage,
		AppSettings appSettings, IWebLogger webLogger)
	{
		var iStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		var thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_exifTool = new ExifTool(iStorage, thumbnailStorage, appSettings, webLogger);
	}

	public async Task<bool> WriteTagsAsync(string subPath, string command)
	{
		return await _exifTool.WriteTagsAsync(subPath, command);
	}

	public async Task<KeyValuePair<bool, string>> WriteTagsAndRenameThumbnailAsync(
		string subPath, string? beforeFileHash,
		string command, CancellationToken cancellationToken = default)
	{
		return await _exifTool.WriteTagsAndRenameThumbnailAsync(subPath, beforeFileHash,
			command, cancellationToken);
	}

	public async Task<bool> WriteTagsThumbnailAsync(string fileHash, string command)
	{
		return await _exifTool.WriteTagsThumbnailAsync(fileHash, command);
	}
}
