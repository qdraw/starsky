using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.writemeta.Models;

namespace starsky.foundation.writemeta.Services;

[Service(typeof(IExifTool), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ExifToolService : IExifTool
{
	private readonly AppSettings _appSettings;
	private readonly IExifToolDownload _exifToolDownload;
	private readonly ExifTool _exifTool;
	private readonly IWebLogger _logger;

	public ExifToolService(ISelectorStorage selectorStorage,
		AppSettings appSettings, IWebLogger logger,
		IExifToolDownload exifToolDownload)
	{
		_appSettings = appSettings;
		_exifToolDownload = exifToolDownload;
		_logger = logger;
		var iStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		var thumbnailStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_exifTool = new ExifTool(iStorage, thumbnailStorage, appSettings,
			logger);
	}

	public async Task<bool> WriteTagsAsync(string subPath, string command)
	{
		try
		{
			return await _exifTool.WriteTagsAsync(subPath, command);
		}
		catch ( ArgumentException )
		{
			await RunSetupAsync();
			return await _exifTool.WriteTagsAsync(subPath, command);
		}
	}

	public async Task<ExifToolWriteTagsAndRenameThumbnailModel>
		WriteTagsAndRenameThumbnailAsync(string subPath,
			string? beforeFileHash, string command,
			CancellationToken cancellationToken = default)
	{
		try
		{
			return await _exifTool.WriteTagsAndRenameThumbnailAsync(subPath,
				beforeFileHash, command, cancellationToken);
		}
		catch ( ArgumentException )
		{
			await RunSetupAsync();
			return await _exifTool.WriteTagsAndRenameThumbnailAsync(subPath,
				beforeFileHash, command, cancellationToken);
		}
	}

	public async Task<bool> WriteTagsThumbnailAsync(string fileHash,
		string command)
	{
		try
		{
			return await _exifTool.WriteTagsThumbnailAsync(fileHash, command);
		}
		catch ( ArgumentException )
		{
			await RunSetupAsync();
			return await _exifTool.WriteTagsThumbnailAsync(fileHash, command);
		}
	}

	private async Task RunSetupAsync()
	{
		_logger.LogInformation("[ExifToolService] ExifTool binary missing — re-running setup");
		await _exifToolDownload.DownloadExifTool(_appSettings.IsWindows);
	}
}
