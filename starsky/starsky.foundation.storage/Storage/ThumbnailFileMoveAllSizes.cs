using System.Collections.Generic;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;

namespace starsky.foundation.storage.Storage;

public sealed class ThumbnailFileMoveAllSizes(
	IStorage thumbnailStorage,
	AppSettings appSettings,
	IWebLogger logger)
{
	/// <summary>
	///     Rename all thumbnails
	/// </summary>
	/// <param name="oldFileHash">from</param>
	/// <param name="newHashCode">to</param>
	/// <returns>Success, size</returns>
	public List<(bool, ThumbnailSize)> FileMove(string oldFileHash, string newHashCode,
		string? reference)
	{
		var status = new List<(bool, ThumbnailSize)>();
		foreach ( var size in ThumbnailNameHelper.AllThumbnailSizes )
		{
			var oldName =
				ThumbnailNameHelper.Combine(oldFileHash, size, appSettings.ThumbnailImageFormat);
			var newName =
				ThumbnailNameHelper.Combine(newHashCode, size, appSettings.ThumbnailImageFormat);

			var isSuccess = thumbnailStorage.FileMove(oldName, newName);

			status.Add(( isSuccess, size ));
		}

		logger.LogInformation($"[FileMove] {reference} \nO: {oldFileHash} - N: {newHashCode} - " +
		                      $"{string.Join(", ", status)}");
		return status;
	}
}
