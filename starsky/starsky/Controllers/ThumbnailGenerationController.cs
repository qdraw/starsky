using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Helpers;
using starsky.foundation.webtelemetry.Interfaces;
using starsky.foundation.worker.Services;

namespace starsky.Controllers
{
	[Authorize]
	public class ThumbnailGenerationController : Controller
	{
		private readonly ISelectorStorage _selectorStorage;
		private readonly IWebLogger _logger;
		private readonly IQuery _query;
		private readonly IWebSocketConnectionsService _connectionsService;

		public ThumbnailGenerationController(ISelectorStorage selectorStorage,
			IQuery query, IWebLogger logger, IWebSocketConnectionsService connectionsService)
		{
			_selectorStorage = selectorStorage;
			_query = query;
			_logger = logger;
			_connectionsService = connectionsService;
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
			var thumbnailStorage = _selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

			if ( !subPathStorage.ExistFolder(subPath))
			{
				return NotFound("folder not found");
			}

			await Task.Factory.StartNew(() => WorkItem(subPath, subPathStorage, thumbnailStorage));
			
			return Json("Job started");
		}
				
		internal async Task WorkItem(string subPath, IStorage subPathStorage, 
			IStorage thumbnailStorage)
		{
			try
			{
				_logger.LogInformation($"[ThumbnailGenerationController] start {subPath}");
				var thumbnail = new Thumbnail(subPathStorage, 
					thumbnailStorage, _logger);
				var thumbs = await thumbnail.CreateThumb(subPath);
				var getAllFilesAsync = await _query.GetAllFilesAsync(subPath);

				var result = new List<FileIndexItem>();
				foreach ( var item in 
				         getAllFilesAsync.Where(item => thumbs.FirstOrDefault(p => p.Item1 == item.FilePath).Item2) )
				{
					if ( item.Tags.Contains("!delete!") ) continue;

					item.SetLastEdited();
					result.Add(item);
				}

				if ( !result.Any() ) return;

				var webSocketResponse =
					new ApiResponseModel<List<FileIndexItem>>(result, ApiMessageType.ThumbnailGeneration);
				await _connectionsService.SendToAllAsync(JsonSerializer.Serialize(
					webSocketResponse,
					DefaultJsonSerializer.CamelCase), CancellationToken.None);
				
				_logger.LogInformation($"[ThumbnailGenerationController] done {subPath}");
			}
			catch ( UnauthorizedAccessException e )
			{
				_logger.LogError($"[ThumbnailGenerationController] catch-ed exceptioin {e.Message}", e);
			}
		}
	}
}
