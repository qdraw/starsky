using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.worker.Services;

namespace starsky.Controllers
{
	[Authorize]
	public class MetaUpdateController : Controller
	{
		private readonly IMetaPreflight _metaPreflight;
		private readonly IMetaUpdateService _metaUpdateService;
		private readonly IMetaReplaceService _metaReplaceService;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly IWebSocketConnectionsService _connectionsService;
		private readonly IWebLogger _logger;

		public MetaUpdateController(IMetaPreflight metaPreflight, IMetaUpdateService metaUpdateService,
			IMetaReplaceService metaReplaceService,  IBackgroundTaskQueue queue, 
			IWebSocketConnectionsService connectionsService, IWebLogger logger)
		{
			_metaPreflight = metaPreflight;
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
			var stopwatch = StartUpdateReplaceStopWatch("update", f, collections);
		    
			var inputFilePaths = PathHelper.SplitInputFilePaths(f);

			var (fileIndexResultsList, changedFileIndexItemName) =  await _metaPreflight.Preflight(inputModel, 
				inputFilePaths, append, collections, rotateClock);

			// Update >
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				var updatedList = await _metaUpdateService
					.Update(changedFileIndexItemName, 
						fileIndexResultsList, inputModel, collections, append, rotateClock);
				await _connectionsService.SendToAllAsync(JsonSerializer.Serialize(updatedList, 
					DefaultJsonSerializer.CamelCase), token);
			});
			
			// When all items are not found
			if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok 
			                                  && p.Status != FileIndexItem.ExifStatus.Deleted))
				return NotFound(fileIndexResultsList);

			// Clone an new item in the list to display
			var returnNewResultList = fileIndexResultsList.Select(item => item.Clone()).ToList();
            
			// when switching very fast between images the background task has not run yet
			_metaUpdateService.UpdateReadMetaCache(returnNewResultList);

			StopUpdateReplaceStopWatch("update", f,stopwatch);

			return Json(returnNewResultList);
		}

		private Stopwatch StartUpdateReplaceStopWatch(string name, string f, bool collections)
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			_logger.LogInformation($"[{name}] f: {f} {DateTime.UtcNow} start  collections: {collections}");
			return stopWatch;
		}

		private void StopUpdateReplaceStopWatch(string name, string f, Stopwatch stopwatch)
		{
			// for debug
			stopwatch.Stop();
			_logger.LogInformation($"[{name}] f: {f} stop duration: {stopwatch.Elapsed.TotalMilliseconds}");
		}

		/// <summary>
		/// Search and Replace text in meta information 
		/// </summary>
		/// <param name="f">subPath filepath to file, split by dot comma (;)</param>
		/// <param name="fieldName">name of fileIndexItem field e.g. Tags</param>
		/// <param name="search">text to search for</param>
		/// <param name="replace">replace [search] with this text</param>
		/// <param name="collections">enable collections</param>
		/// <returns>list of changed files</returns>
		/// <response code="200">Initialized replace job</response>
		/// <response code="404">item(s) not found</response>
		/// <response code="401">User unauthorized</response>
		[HttpPost("/api/replace")]
		[ProducesResponseType(typeof(List<FileIndexItem>),200)]
		[ProducesResponseType(typeof(List<FileIndexItem>),404)]
		[Produces("application/json")]
		public IActionResult Replace(string f, string fieldName, string search,
			string replace, bool collections = true)
		{
			var stopwatch = StartUpdateReplaceStopWatch("replace", f, collections);

			var fileIndexResultsList = _metaReplaceService
				.Replace(f, fieldName, search, replace, collections);
		    
			// Update >
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				var resultsOkList =
					fileIndexResultsList.Where(p => p.Status
					                                == FileIndexItem.ExifStatus.Ok).ToList();
				
				foreach ( var inputModel in resultsOkList )
				{
					// The differences are specified before update
					var changedFileIndexItemName = new Dictionary<string, List<string>>
					{
						{ 
							inputModel.FilePath, new List<string>
							{
								fieldName
							} 
						}
					};
					
					await _metaUpdateService
						.Update(changedFileIndexItemName,new List<FileIndexItem>{inputModel}, inputModel, 
							collections, false, 0);
				}

				if ( resultsOkList.Any() )
				{
					await _connectionsService.SendToAllAsync(JsonSerializer.Serialize(resultsOkList,
						DefaultJsonSerializer.CamelCase), token);
				}

			});

			StopUpdateReplaceStopWatch("replace", f,stopwatch);
			
			// When all items are not found
			if (fileIndexResultsList.All(p => p.Status != FileIndexItem.ExifStatus.Ok))
			{
				return NotFound(fileIndexResultsList);
			}

			// Clone an new item in the list to display
			var returnNewResultList = new List<FileIndexItem>();
			foreach ( var clonedItem in fileIndexResultsList.Select(item => item.Clone()) )
			{
				clonedItem.FileHash = null;
				returnNewResultList.Add(clonedItem);
			}
			
			return Json(returnNewResultList);
		}
	}
}
