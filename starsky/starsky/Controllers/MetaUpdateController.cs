using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.metaupdate.Interfaces;
using starsky.feature.realtime.Interface;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.webtelemetry.Helpers;
using starsky.foundation.worker.Interfaces;

namespace starsky.Controllers
{
	[Authorize]
	public class MetaUpdateController : Controller
	{
		private readonly IMetaPreflight _metaPreflight;
		private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
		private readonly IWebLogger _logger;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IMetaUpdateService _metaUpdateService;

		public MetaUpdateController(IMetaPreflight metaPreflight, 
			IMetaUpdateService metaUpdateService,
			IUpdateBackgroundTaskQueue queue, 
			IWebLogger logger, 
			IServiceScopeFactory scopeFactory)
		{
			_metaPreflight = metaPreflight;
			_scopeFactory = scopeFactory;
			_metaUpdateService = metaUpdateService;
			_bgTaskQueue = queue;
			_logger = logger;
		}
	    
		/// <summary>
		/// Update Exif and Rotation API
		/// </summary>
		/// <param name="f">subPath filepath to file, split by dot comma (;)</param>
		/// <param name="inputModel">tags: use for keywords
		/// colorClass: int 0-9, the colorClass to fast select images
		/// description: string to update description/caption abstract, empty will be ignore
		/// title: edit image title</param>
		/// <param name="collections">StackCollections bool, default true</param>
		/// <param name="append">only for stings, add update to existing items</param>
		/// <param name="rotateClock">relative orientation -1 or 1</param>
		/// <returns>update json (IActionResult Update)</returns>
		/// <response code="200">the item including the updated content</response>
		/// <response code="404">item not found in the database or on disk</response>
		/// <response code="401">User unauthorized</response>
		[IgnoreAntiforgeryToken]
		[ProducesResponseType(typeof(List<FileIndexItem>),200)]
		[ProducesResponseType(typeof(List<FileIndexItem>),404)]
		[HttpPost("/api/update")]
		[Produces("application/json")]
		public async Task<IActionResult> UpdateAsync(FileIndexItem inputModel, string f, bool append, 
			bool collections = true, int rotateClock = 0)
		{
			var stopwatch = StopWatchLogger.StartUpdateReplaceStopWatch();
		    
			var inputFilePaths = PathHelper.SplitInputFilePaths(f);

			var (fileIndexResultsList, changedFileIndexItemName) =  await _metaPreflight.Preflight(inputModel, 
				inputFilePaths, append, collections, rotateClock);

			var operationId = HttpContext.GetOperationId();
			
			// Update >
			_bgTaskQueue.QueueBackgroundWorkItem(async _ =>
			{
				var operationHolder = RequestTelemetryHelper.GetOperationHolder(_scopeFactory,
					nameof(UpdateAsync), operationId);
				
				var metaUpdateService = _scopeFactory.CreateScope()
					.ServiceProvider.GetRequiredService<IMetaUpdateService>();

				var data = await metaUpdateService.UpdateAsync(
					changedFileIndexItemName, fileIndexResultsList, null,
						collections, append, rotateClock);
				operationHolder.SetData(_scopeFactory, data);
			});

			// before sending not founds
			new StopWatchLogger(_logger).StopUpdateReplaceStopWatch("update", f,collections, stopwatch);

			// When all items are not found
			if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok 
			                                  && p.Status != FileIndexItem.ExifStatus.Deleted))
				return NotFound(fileIndexResultsList);

			// Clone an new item in the list to display
			var returnNewResultList = fileIndexResultsList.Select(item => item.Clone()).ToList();
            
			// when switching very fast between images the background task has not run yet
			_metaUpdateService.UpdateReadMetaCache(returnNewResultList);
			
			// Push direct to socket when update or replace to avoid undo after a second
			_logger.LogInformation($"[UpdateController] send to socket {f}");

			await Task.Run(async () => await TaskRun(fileIndexResultsList));

			return Json(returnNewResultList);
		}

		private async Task TaskRun(List<FileIndexItem> fileIndexResultsList)
		{
			var webSocketResponse =
				new ApiNotificationResponseModel<List<FileIndexItem>>(fileIndexResultsList, ApiNotificationType.MetaUpdate);
			var realtimeConnectionsService = _scopeFactory.CreateScope()
				.ServiceProvider.GetRequiredService<IRealtimeConnectionsService>();
			await realtimeConnectionsService.NotificationToAllAsync(webSocketResponse, CancellationToken.None);
		}
	}
}
