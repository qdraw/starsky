using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.webtelemetry.Helpers;
using starsky.foundation.worker.Interfaces;

namespace starsky.Controllers
{
	[Authorize]
	public class MetaUpdateController : Controller
	{
		private readonly IMetaPreflight _metaPreflight;
		private readonly IMetaReplaceService _metaReplaceService;
		private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
		private readonly IWebSocketConnectionsService _connectionsService;
		private readonly IWebLogger _logger;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IMetaUpdateService _metaUpdateService;

		public MetaUpdateController(IMetaPreflight metaPreflight, IMetaUpdateService metaUpdateService,
			IMetaReplaceService metaReplaceService,  IUpdateBackgroundTaskQueue queue, 
			IWebSocketConnectionsService connectionsService, IWebLogger logger, IServiceScopeFactory scopeFactory)
		{
			_metaPreflight = metaPreflight;
			_scopeFactory = scopeFactory;
			_metaUpdateService = metaUpdateService;
			_metaReplaceService = metaReplaceService;
			_bgTaskQueue = queue;
			_connectionsService = connectionsService;
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
			var stopwatch = StartUpdateReplaceStopWatch();
		    
			var inputFilePaths = PathHelper.SplitInputFilePaths(f);

			var (fileIndexResultsList, changedFileIndexItemName) =  await _metaPreflight.Preflight(inputModel, 
				inputFilePaths, append, collections, rotateClock);

			var operationId = HttpContext.GetOperationId();
			
			// Update >
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				var operationHolder = RequestTelemetryHelper.GetOperationHolder(_scopeFactory,
					nameof(UpdateAsync), operationId);
				
				var metaUpdateService = _scopeFactory.CreateScope()
					.ServiceProvider.GetService<IMetaUpdateService>();
				
				operationHolder.SetData(await metaUpdateService
					.Update(changedFileIndexItemName, fileIndexResultsList, null,
						collections, append, rotateClock));
			});
			
			// When all items are not found
			if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok 
			                                  && p.Status != FileIndexItem.ExifStatus.Deleted))
				return NotFound(fileIndexResultsList);

			// Clone an new item in the list to display
			var returnNewResultList = fileIndexResultsList.Select(item => item.Clone()).ToList();
            
			// when switching very fast between images the background task has not run yet
			_metaUpdateService.UpdateReadMetaCache(returnNewResultList);

			StopUpdateReplaceStopWatch("update", f,collections, stopwatch);

			// Push direct to socket when update or replace to avoid undo after a second
			_logger.LogInformation($"[UpdateController] send to socket {f}");
			await _connectionsService.SendToAllAsync("[system] /api/update called",CancellationToken.None);
			await _connectionsService.SendToAllAsync(JsonSerializer.Serialize(fileIndexResultsList, 
				DefaultJsonSerializer.CamelCase), CancellationToken.None);
			
			return Json(returnNewResultList);
		}

		private Stopwatch StartUpdateReplaceStopWatch()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			return stopWatch;
		}

		private void StopUpdateReplaceStopWatch(string name, string f, bool collections, Stopwatch stopwatch)
		{
			// for debug
			stopwatch.Stop();
			_logger.LogInformation($"[{name}] f: {f} Stopwatch response collections: " +
			                       $"{collections} {DateTime.UtcNow} duration: {stopwatch.Elapsed.TotalMilliseconds} ms or:" +
			                       $" {stopwatch.Elapsed.TotalSeconds} sec");
		}

		/// <summary>
		/// Search and Replace text in meta information 
		/// </summary>
		/// <param name="f">subPath filepath to file, split by dot comma (;)</param>
		/// <param name="fieldName">name of fileIndexItem field e.g. Tags</param>
		/// <param name="search">text to search for</param>
		/// <param name="replace">replace [search] with this text</param>
		/// <param name="collections">enable collections</param>
		/// <returns>list of changed files (IActionResult Replace)</returns>
		/// <response code="200">Initialized replace job</response>
		/// <response code="404">item(s) not found</response>
		/// <response code="401">User unauthorized</response>
		[HttpPost("/api/replace")]
		[ProducesResponseType(typeof(List<FileIndexItem>),200)]
		[ProducesResponseType(typeof(List<FileIndexItem>),404)]
		[Produces("application/json")]
		public async Task<IActionResult> Replace(string f, string fieldName, string search,
			string replace, bool collections = true)
		{
			var stopwatch = StartUpdateReplaceStopWatch();

			var fileIndexResultsList = await _metaReplaceService
				.Replace(f, fieldName, search, replace, collections);
		    
			var resultsOkOrDeleteList = fileIndexResultsList.Where(
				p => p.Status == FileIndexItem.ExifStatus.Ok || 
				     p.Status == FileIndexItem.ExifStatus.Deleted).ToList();
			
			var changedFileIndexItemName = resultsOkOrDeleteList.
				ToDictionary(item => item.FilePath, item => new List<string> {fieldName});

			// Update >
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				var metaUpdateService = _scopeFactory.CreateScope()
					.ServiceProvider.GetService<IMetaUpdateService>();
				await metaUpdateService
					.Update(changedFileIndexItemName, resultsOkOrDeleteList,
						null, collections, false, 0);
			});

			StopUpdateReplaceStopWatch("replace", f, collections, stopwatch);
			
			// When all items are not found
			if (!resultsOkOrDeleteList.Any())
			{
				return NotFound(fileIndexResultsList);
			}
			
			// Push direct to socket when update or replace to avoid undo after a second
			await _connectionsService.SendToAllAsync("[system] /api/replace called",CancellationToken.None);
			await _connectionsService.SendToAllAsync(JsonSerializer.Serialize(resultsOkOrDeleteList,
				DefaultJsonSerializer.CamelCase), CancellationToken.None);
			
			return Json(fileIndexResultsList);
		}
	}
}
