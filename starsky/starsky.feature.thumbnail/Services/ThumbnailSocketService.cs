using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.feature.thumbnail.Services;

[Service(typeof(IThumbnailSocketService),
	InjectionLifetime = InjectionLifetime.Scoped)]
public class ThumbnailSocketService(
	IQuery query,
	IWebSocketConnectionsService connectionsService,
	IWebLogger logger,
	INotificationQuery notificationQuery) : IThumbnailSocketService
{
	/// <summary>
	///     Create a notification for all clients
	/// </summary>
	/// <param name="subPath">Can be both file or folder</param>
	/// <param name="generateThumbnailResults">the results</param>
	public async Task NotificationSocketUpdate(string subPath,
		List<GenerationResultModel> generateThumbnailResults)
	{
		var fileIndexItems = await query.GetObjectsByFilePathAsync(subPath, false);

		if ( fileIndexItems.Count == 1 && fileIndexItems[0].FilePath == subPath
		                               && fileIndexItems[0].IsDirectory == true )
		{
			fileIndexItems = await query.GetAllFilesAsync(subPath);
		}

		var result = WhichFilesNeedToBePushedForUpdates(
			generateThumbnailResults, fileIndexItems);

		if ( result.Count == 0 )
		{
			logger.LogInformation(
				$"[ThumbnailSocketService] done - no results {subPath}");
			return;
		}

		var webSocketResponse =
			new ApiNotificationResponseModel<List<FileIndexItem>>(result,
				ApiNotificationType.ThumbnailGeneration);
		await connectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);
		await notificationQuery.AddNotification(webSocketResponse);
	}

	internal static List<FileIndexItem> WhichFilesNeedToBePushedForUpdates(
		List<GenerationResultModel> thumbs, IEnumerable<FileIndexItem> getAllFilesAsync)
	{
		var result = new List<FileIndexItem>();
		var searchFor = getAllFilesAsync.Where(item =>
			thumbs.Find(p => p.SubPath == item.FilePath && item.Tags != null)
				?.Success == true).DistinctBy(p => p.FilePath);
		foreach ( var item in searchFor )
		{
			if ( item.Tags!.Contains(TrashKeyword.TrashKeywordString) )
			{
				continue;
			}

			item.FileHash = thumbs.FirstOrDefault(p => p.SubPath == item.FilePath)
				?.FileHash;
			item.LastChanged = ["LastEdited", "FileHash", "Src"];
			item.Status = FileIndexItem.ExifStatus.Ok;
			item.LastEdited = DateTime.UtcNow;
			result.Add(item);
		}

		return result;
	}
}
