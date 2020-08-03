using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Services;
using starskycore.Services;

namespace starsky.Controllers
{
	[Authorize]
	public class ThumbnailGenerationController : Controller
	{
		private readonly ISelectorStorage _selectorStorage;
		private readonly IBackgroundTaskQueue _bgTaskQueue;
		private readonly ITelemetryService _telemetryService;

		public ThumbnailGenerationController(ISelectorStorage selectorStorage,
			IBackgroundTaskQueue queue, ITelemetryService telemetryService = null)
		{
			_selectorStorage = selectorStorage;
			_bgTaskQueue = queue;
			_telemetryService = telemetryService;
		}
		
		/// <summary>
		/// Create thumbnails for a folder in the background
		/// </summary>
		/// <response code="200">give start signal</response>
		/// <response code="404">folder not found</response>
		/// <returns>the ImportIndexItem of the imported files</returns>
		[HttpPost("/api/thumbnail-generation")]
		[Produces("application/json")]
		public IActionResult ThumbnailGeneration(string f)
		{
			var subPath = PathHelper.RemoveLatestSlash(f);
			var subPathStorage = _selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			var thumbnailStorage = _selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);

			if ( !subPathStorage.ExistFolder(subPath))
			{
				return NotFound("folder not found");
			}

			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				await WorkItem(subPath, subPathStorage, thumbnailStorage);
			});	
			
			return Json("started");
		}
				
		internal async Task WorkItem(string subPath, IStorage subPathStorage, 
			IStorage thumbnailStorage)
		{
			try
			{
				new Thumbnail(subPathStorage, thumbnailStorage).CreateThumb(subPath);
			}
			catch ( UnauthorizedAccessException e )
			{
				Console.WriteLine(e);
				_telemetryService?.TrackException(e);
			}
		}
	}
}
