using starsky.feature.metaupdate.Interfaces;
using starsky.feature.trash.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.native.Trash.Interfaces;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.worker.Interfaces;

namespace starsky.feature.trash.Services;

[Service(typeof(IMoveToTrashService), InjectionLifetime = InjectionLifetime.Scoped)]
public class MoveToTrashService : IMoveToTrashService
{
	private readonly AppSettings _appSettings;
	private readonly IQuery _query;
	private readonly IMetaPreflight _metaPreflight;
	private readonly IUpdateBackgroundTaskQueue _queue;
	private readonly ITrashService _systemTrashService;
	private readonly IMetaUpdateService _metaUpdateService;
	private readonly ITrashConnectionService _connectionService;


	public MoveToTrashService(AppSettings appSettings, IQuery query,
		IMetaPreflight metaPreflight, 
		IUpdateBackgroundTaskQueue queue, 
		ITrashService systemTrashService, IMetaUpdateService metaUpdateService,
		ITrashConnectionService connectionService
		)
	{
		_appSettings = appSettings;
		_query = query;
		_metaPreflight = metaPreflight;
		_queue = queue;
		_systemTrashService = systemTrashService;
		_metaUpdateService = metaUpdateService;
		_connectionService = connectionService;
	}
	
	public async Task<List<FileIndexItem>> MoveToTrashAsync(
		string[] inputFilePaths, bool collections)
	{
		var inputModel = new FileIndexItem { Tags = TrashKeyword.TrashKeywordString };
		var (fileIndexResultsList, changedFileIndexItemName) =  await _metaPreflight.PreflightAsync(inputModel, inputFilePaths,
			false, collections, 0);

		var moveToTrash =
			fileIndexResultsList.Where(p =>
				p.Status is FileIndexItem.ExifStatus.Ok or FileIndexItem.ExifStatus.Deleted).ToList();

		if ( _appSettings.UseSystemTrash == true &&  _systemTrashService.DetectToUseSystemTrash() )
		{
			await _queue.QueueBackgroundWorkItemAsync(async _ =>
			{
				await SystemTrashInQueue(moveToTrash);
			}, "trash");
			
			return await _connectionService.ConnectionServiceAsync(moveToTrash, FileIndexItem.ExifStatus.NotFoundSourceMissing);
		}
		

		await _queue.QueueBackgroundWorkItemAsync( async _ =>
		{
			await _metaUpdateService.UpdateAsync(changedFileIndexItemName,
				fileIndexResultsList, inputModel, collections, false, 0);
		}, "trash");
		
		return await _connectionService.ConnectionServiceAsync(moveToTrash, FileIndexItem.ExifStatus.Deleted);
	}



	private async Task<List<FileIndexItem>> SystemTrashInQueue(
		List<FileIndexItem> moveToTrash)
	{
		var fullFilePaths = moveToTrash
			.Where(p => p.FilePath != null)
			.Select(p => _appSettings.DatabasePathToFilePath(p.FilePath))
			.ToList();
		_systemTrashService.Trash(fullFilePaths);

		_query.RemoveItemAsync(moveToTrash);
		return moveToTrash;
	}
}

