using System.Text.Json;
using starsky.feature.trash.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.native.Trash.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;

namespace starsky.feature.trash.Services;

[Service(typeof(IBackgroundJobHandler), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class MoveToTrashJobHandler(
	ITrashConnectionService connectionService,
	ITrashService systemTrashService,
	IMetaUpdateService metaUpdateService,
	IQuery query,
	AppSettings appSettings) : IBackgroundJobHandler
{
	public string JobType => MoveToTrashService.JobType;

	public async Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
	{
		if ( string.IsNullOrWhiteSpace(payloadJson) )
		{
			throw new ArgumentException("Missing payload", nameof(payloadJson));
		}

		var payload = JsonSerializer.Deserialize<MoveToTrashPayload>(payloadJson);
		if ( payload == null )
		{
			throw new ArgumentException("Invalid payload", nameof(payloadJson));
		}

		await connectionService.ConnectionServiceAsync(payload.MoveToTrashList,
			payload.IsSystemTrashEnabled);

		if ( payload.IsSystemTrashEnabled )
		{
			var fullFilePaths = payload.MoveToTrashList
				.Where(p => p.FilePath != null)
				.Select(p => appSettings.DatabasePathToFilePath(p.FilePath!))
				.ToList();

			systemTrashService.Trash(fullFilePaths);
			await query.RemoveItemAsync(payload.MoveToTrashList);
			return;
		}

		await metaUpdateService.UpdateAsync(payload.ChangedFileIndexItemName,
			payload.FileIndexResultsList,
			payload.InputModel,
			payload.Collections,
			false,
			0);
	}
}
