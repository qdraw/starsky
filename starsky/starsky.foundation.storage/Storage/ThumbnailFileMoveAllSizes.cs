using System.Collections.Generic;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Storage;

public sealed class ThumbnailFileMoveAllSizes
{
	private readonly AppSettings _appSettings;
	private readonly IWebLogger _logger;
	private readonly IStorage _thumbnailStorage;

	public ThumbnailFileMoveAllSizes(IStorage thumbnailStorage, AppSettings appSettings,
		IWebLogger logger)
	{
		_thumbnailStorage = thumbnailStorage;
		_appSettings = appSettings;
		_logger = logger;
	}

	/// <summary>
	///     Rename all thumbnails
	/// </summary>
	/// <param name="oldFileHash">from</param>
	/// <param name="newHashCode">to</param>
	/// <returns>Success, size</returns>
	public List<(bool, ThumbnailSize)> FileMove(string oldFileHash, string newHashCode)
	{
		var status = new List<(bool, ThumbnailSize)>();
		foreach ( var size in ThumbnailNameHelper.AllThumbnailSizes )
		{
			var oldName =
				ThumbnailNameHelper.Combine(oldFileHash, size, _appSettings.ThumbnailImageFormat);
			var newName =
				ThumbnailNameHelper.Combine(newHashCode, size, _appSettings.ThumbnailImageFormat);

			var isSuccess = _thumbnailStorage.FileMove(oldName, newName);

			status.Add(( isSuccess, size ));
		}

		_logger.LogInformation($"[FileMove] O: {oldFileHash} - N: {newHashCode} - " +
		                       $"{string.Join(", ", status)}");
		return status;
	}
}

// // Log
// var messageSuccess = isSuccess ? "File successfully moved" : "File failed to move";
// var message = $"[FileMove] {messageSuccess} from " +
//               $"'{oldName}' to '{newName}' for size {size}.";
//
// if ( !isSuccess )
// {
// 	_logger.LogInformation(message);
// 	continue;
// }
//
// _logger.LogDebug(message);
