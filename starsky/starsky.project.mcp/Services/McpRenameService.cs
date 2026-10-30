using starsky.feature.rename.Helpers;
using starsky.feature.rename.Services;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.project.mcp.Services;

/// <summary>
///     The rename of `/api/disk/rename`: rename on disk and in the database,
///     then tell the other clients
/// </summary>
[Service(typeof(McpRenameService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class McpRenameService(
	IQuery query,
	ISelectorStorage selectorStorage,
	INotificationQuery notificationQuery,
	IWebSocketConnectionsService connectionsService,
	IWebLogger logger)
{
	/// <returns>the status per file, notifications are only sent when something is renamed</returns>
	public async Task<List<FileIndexItem>> RenameAsync(string source, string target,
		bool collections)
	{
		var renameService = new RenameService(query,
			selectorStorage.Get(SelectorStorage.StorageServices.SubPath), logger);
		var renamed = await renameService.Rename(source, target, collections);
		if ( renamed.Exists(p => p.Status == FileIndexItem.ExifStatus.Ok) )
		{
			await RenameNotification.SendAsync(renamed, notificationQuery, connectionsService);
		}

		return renamed;
	}
}
