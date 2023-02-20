using starsky.feature.metaupdate.Interfaces;
using starsky.feature.trash.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.native.Trash.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;

namespace starsky.feature.trash.Services;

public class MoveToTrashService : IMoveToTrashService
{
	private readonly AppSettings _appSettings;
	private readonly IMetaPreflight _metaPreflight;
	private readonly IUpdateBackgroundTaskQueue _queue;
	private readonly ITrashService _systemTrashService;
	private readonly IMetaUpdateService _metaUpdateService;

	public MoveToTrashService(AppSettings appSettings,
		IMetaPreflight metaPreflight, 
		IUpdateBackgroundTaskQueue queue, 
		ITrashService systemTrashService, IMetaUpdateService metaUpdateService)
	{
		_appSettings = appSettings;
		_metaPreflight = metaPreflight;
		_queue = queue;
		_systemTrashService = systemTrashService;
		_metaUpdateService = metaUpdateService;
	}
	
	public async Task MoveToTrashAsync(string[] inputFilePaths, bool collections)
	{
		var inputModel = new FileIndexItem { Tags = TrashKeyword.TrashKeywordString };
		var (fileIndexResultsList, changedFileIndexItemName) =  await _metaPreflight.PreflightAsync(inputModel, inputFilePaths,
			false, collections, 0);

		var moveToTrash =
			fileIndexResultsList.Where(p =>
				p.Status == FileIndexItem.ExifStatus.Ok);

		if ( _appSettings.UseSystemTrash == true &&  _systemTrashService.DetectToUseSystemTrash() )
		{
			await _queue.QueueBackgroundWorkItemAsync(_ =>
			{
				var paths = moveToTrash
					.Where(p => p.FilePath != null)
					.Select(p => p.FilePath)
					.Cast<string>().ToList();
				_systemTrashService.Trash(paths);
				return ValueTask.CompletedTask;
			}, "trash");
			return;
		}
		
		await _queue.QueueBackgroundWorkItemAsync( async _ =>
		{
			await _metaUpdateService.UpdateAsync(changedFileIndexItemName,
				fileIndexResultsList, inputModel, collections, false, 0);
		}, "trash");

	}
}

