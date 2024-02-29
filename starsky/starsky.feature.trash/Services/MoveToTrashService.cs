using System.Runtime.CompilerServices;
using System.Text;
using starsky.feature.metaupdate.Interfaces;
using starsky.feature.trash.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.native.Trash.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]

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

	/// <summary>
	/// Is supported and enabled in the feature toggle
	/// </summary>
	/// <returns>Should you use it?</returns>
	public bool IsEnabled()
	{
		return _appSettings.UseSystemTrash == true &&
		       _systemTrashService.DetectToUseSystemTrash();
	}

	/// <summary>
	/// Move a file to the internal trash or system trash
	/// Depends on the feature toggle
	/// </summary>
	/// <param name="inputFilePaths">list of paths</param>
	/// <param name="collections">is stack collections enabled</param>
	/// <returns>list of files</returns>
	public async Task<List<FileIndexItem>> MoveToTrashAsync(
		List<string> inputFilePaths, bool collections)
	{
		var inputModel = new FileIndexItem { Tags = TrashKeyword.TrashKeywordString };
		var (fileIndexResultsList, changedFileIndexItemName) =
			await _metaPreflight.PreflightAsync(inputModel, inputFilePaths,
				false, collections, 0);

		( fileIndexResultsList, changedFileIndexItemName ) =
			await AppendChildItemsToTrashList(fileIndexResultsList, changedFileIndexItemName);

		var moveToTrashList =
			fileIndexResultsList.Where(p =>
					p.Status is FileIndexItem.ExifStatus.Ok or FileIndexItem.ExifStatus.Deleted)
				.ToList();

		var isSystemTrashEnabled = IsEnabled();

		await _queue.QueueBackgroundWorkItemAsync(async _ =>
		{
			await _connectionService.ConnectionServiceAsync(moveToTrashList, isSystemTrashEnabled);

			if ( isSystemTrashEnabled )
			{
				await SystemTrashInQueue(moveToTrashList);
				return;
			}

			await MetaTrashInQueue(changedFileIndexItemName,
				fileIndexResultsList, inputModel, collections);
		}, "trash");

		return TrashConnectionService.StatusUpdate(moveToTrashList, isSystemTrashEnabled);
	}

	private async Task MetaTrashInQueue(Dictionary<string, List<string>> changedFileIndexItemName,
		List<FileIndexItem> fileIndexResultsList, FileIndexItem inputModel, bool collections)
	{
		await _metaUpdateService.UpdateAsync(changedFileIndexItemName,
			fileIndexResultsList, inputModel, collections, false, 0);
	}

	/// <summary>
	/// For directories add all sub files
	/// </summary>
	/// <param name="moveToTrash"></param>
	/// <param name="changedFileIndexItemName"></param>
	internal async Task<(List<FileIndexItem>, Dictionary<string, List<string>>)>
		AppendChildItemsToTrashList(List<FileIndexItem> moveToTrash,
			Dictionary<string, List<string>> changedFileIndexItemName)
	{
		var parentSubPaths = moveToTrash
			.Where(p => !string.IsNullOrEmpty(p.FilePath) && p.IsDirectory == true)
			.Select(p => p.FilePath).Cast<string>()
			.ToList();

		if ( parentSubPaths.Count == 0 )
		{
			return ( moveToTrash, changedFileIndexItemName );
		}

		var childItems = ( await _query.GetAllObjectsAsync(parentSubPaths) )
			.Where(p => p.FilePath != null).ToList();

		moveToTrash.AddRange(childItems);
		foreach ( var childItem in childItems )
		{
			var builder = new StringBuilder(childItem.Tags);
			builder.Append(", ");
			builder.Append(TrashKeyword.TrashKeywordString);
			childItem.Tags = builder.ToString();

			changedFileIndexItemName.TryAdd(childItem.FilePath!, new List<string> { "tags" });
		}

		return ( moveToTrash, changedFileIndexItemName );
	}

	private async Task SystemTrashInQueue(List<FileIndexItem> moveToTrash)
	{
		var fullFilePaths = moveToTrash
			.Where(p => p.FilePath != null)
			.Select(p => _appSettings.DatabasePathToFilePath(p.FilePath!))
			.ToList();

		_systemTrashService.Trash(fullFilePaths);

		await _query.RemoveItemAsync(moveToTrash);
	}
	
	/// <summary>
	/// Is it supported to use the system trash
	/// But it does NOT check if the feature toggle is enabled
	/// Used for end2end test to check if it an option to enable / disable the system trash
	/// </summary>
	/// <returns>true if supported</returns>
	public bool DetectToUseSystemTrash()
	{
		return _systemTrashService.DetectToUseSystemTrash();
	}
}
