using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;
using starsky.foundation.worker.ThumbnailServices.Interfaces;

namespace starsky.Controllers
{
	[Authorize]
	public sealed class ThumbnailGenerationController : Controller
	{
		private readonly ISelectorStorage _selectorStorage;
		private readonly IWebLogger _logger;
		private readonly IQuery _query;
		private readonly IWebSocketConnectionsService _connectionsService;
		private readonly IThumbnailService _thumbnailService;
		private readonly IThumbnailQueuedHostedService _bgTaskQueue;

		public ThumbnailGenerationController(ISelectorStorage selectorStorage,
			IQuery query, IWebLogger logger, IWebSocketConnectionsService connectionsService, 
			IThumbnailService thumbnailService, 
			IThumbnailQueuedHostedService bgTaskQueue)
		{
			_selectorStorage = selectorStorage;
			_query = query;
			_logger = logger;
			_connectionsService = connectionsService;
			_thumbnailService = thumbnailService;
			_bgTaskQueue = bgTaskQueue;
		}
		
		/// <summary>
		/// Create thumbnails for a folder in the background
		/// </summary>
		/// <response code="200">give start signal</response>
		/// <response code="404">folder not found</response>
		/// <returns>the ImportIndexItem of the imported files</returns>
		[HttpPost("/api/thumbnail-generation")]
		[Produces("application/json")]
		public async Task<IActionResult> ThumbnailGeneration(string f)
		{
			var subPath = f != "/" ? PathHelper.RemoveLatestSlash(f) : "/";
			var subPathStorage = _selectorStorage.Get(SelectorStorage.StorageServices.SubPath);

			if ( !subPathStorage.ExistFolder(subPath))
			{
				return NotFound("folder not found");
			}

			// When the CPU is to high its gives a Error 500
			await _bgTaskQueue.QueueBackgroundWorkItemAsync(async _ =>
			{
				await WorkThumbnailGeneration(subPath);
			}, f);
			
			return Json("Job started");
		}
				
		internal async Task WorkThumbnailGeneration(string subPath)
		{
			try
			{
				_logger.LogInformation($"[ThumbnailGenerationController] start {subPath}");
				var thumbs = await _thumbnailService.CreateThumbnailAsync(subPath);
				var getAllFilesAsync = await _query.GetAllFilesAsync(subPath);

				var result =
					WhichFilesNeedToBePushedForUpdates(thumbs, getAllFilesAsync);

				if ( !result.Any() )
				{
					_logger.LogInformation($"[ThumbnailGenerationController] done - no results {subPath}");
					return;
				}

				var webSocketResponse =
					new ApiNotificationResponseModel<List<FileIndexItem>>(result, ApiNotificationType.ThumbnailGeneration);
				await _connectionsService.SendToAllAsync(webSocketResponse, CancellationToken.None);
				
				_logger.LogInformation($"[ThumbnailGenerationController] done {subPath}");
			}
			catch ( UnauthorizedAccessException e )
			{
				_logger.LogError($"[ThumbnailGenerationController] catch-ed exception {e.Message}", e);
			}
		}

		internal static List<FileIndexItem> WhichFilesNeedToBePushedForUpdates(List<GenerationResultModel> thumbs, IEnumerable<FileIndexItem> getAllFilesAsync)
		{
			var result = new List<FileIndexItem>();
			var searchFor = getAllFilesAsync.Where(item =>
				thumbs.FirstOrDefault(p => p.SubPath == item.FilePath && item.Tags != null)
					?.Success == true).DistinctBy(p => p.FilePath);
			foreach ( var item in searchFor )
			{
				if ( item.Tags!.Contains("!delete!") ) continue;

				item.SetLastEdited();
				item.LastChanged = new List<string> {"LastEdited", "FileHash"};
				result.Add(item);
			}

			return result;
		}
	}
}
