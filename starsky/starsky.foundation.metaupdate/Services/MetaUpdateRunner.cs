using System.Diagnostics;
using System.Text.Json;
using starsky.foundation.database.JsonConverters;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;

namespace starsky.foundation.metaupdate.Services;

/// <summary>
///     The update flow behind `/api/update`, shared with other entry points (for example MCP)
/// </summary>
[Service(typeof(IMetaUpdateRunner), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class MetaUpdateRunner : IMetaUpdateRunner
{
	private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
	private readonly IMetaUpdateConnectionService _connectionService;
	private readonly IWebLogger _logger;
	private readonly IMetaPreflight _metaPreflight;
	private readonly IMetaUpdateService _metaUpdateService;

	public MetaUpdateRunner(IMetaPreflight metaPreflight,
		IMetaUpdateService metaUpdateService,
		IUpdateBackgroundTaskQueue queue,
		IWebLogger logger,
		IMetaUpdateConnectionService connectionService)
	{
		_metaPreflight = metaPreflight;
		_metaUpdateService = metaUpdateService;
		_bgTaskQueue = queue;
		_logger = logger;
		_connectionService = connectionService;
	}

	public async Task<MetaUpdateRunResult> UpdateAsync(FileIndexItem inputModel,
		string[] inputFilePaths, bool append, bool collections, int rotateClock)
	{
		var stopwatch = StopWatchLogger.StartUpdateReplaceStopWatch();

		var (fileIndexResultsList, changedFileIndexItemName) =
			await _metaPreflight.PreflightAsync(inputModel,
				[.. inputFilePaths], append, collections, rotateClock);

		// Update >
		await _bgTaskQueue.QueueJobAsync(new BackgroundTaskQueueJob
		{
			MetaData = "MetaUpdate",
			TraceParentId = Activity.Current?.Id,
			PriorityLane = ProcessTaskQueue.PriorityLaneUpdate,
			JobType = MetaUpdateBackgroundJobHandler.MetaUpdate,
			PayloadJson = JsonSerializer.Serialize(
				new MetaUpdateBackgroundPayload
				{
					ChangedFileIndexItemName = changedFileIndexItemName,
					FileIndexResultsList = fileIndexResultsList,
					Collections = collections,
					Append = append,
					RotateClock = rotateClock
				}, DefaultJsonFileIndexJsonSerializer.WithIdConverter)
		});

		// before sending not founds
		new StopWatchLogger(_logger).StopUpdateReplaceStopWatch("update",
			string.Join(';', inputFilePaths), collections, stopwatch);

		// When all items are not found
		if ( fileIndexResultsList.TrueForAll(p =>
			    p.Status != FileIndexItem.ExifStatus.Ok
			    && p.Status != FileIndexItem.ExifStatus.Deleted) )
		{
			return new MetaUpdateRunResult(fileIndexResultsList, null);
		}

		// Clone a new item in the list to display
		var returnNewResultList = fileIndexResultsList.Select(item => item.Clone()).ToList();

		// when switching very fast between images the background task has not run yet
		_metaUpdateService.UpdateReadMetaCache(returnNewResultList);

		// Push direct to socket when update or replace to avoid undo after a second
		_logger.LogInformation(
			$"[UpdateController] send to socket {inputFilePaths[0]}");

		await Task.Run(async () =>
			await _connectionService.UpdateWebSocketTaskRun(fileIndexResultsList));

		return new MetaUpdateRunResult(fileIndexResultsList, returnNewResultList);
	}
}
