using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.Controllers
{
	[Authorize]
	public sealed class ThumbnailGenerationController : Controller
	{
		private readonly ISelectorStorage _selectorStorage;
		private readonly IThumbnailGenerationService _thumbnailGenerationService;

		public ThumbnailGenerationController(ISelectorStorage selectorStorage,
			IThumbnailGenerationService thumbnailGenerationService)
		{
			_selectorStorage = selectorStorage;
			_thumbnailGenerationService = thumbnailGenerationService;
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
			await _thumbnailGenerationService.BgQueue(subPath);
			
			return Json("Job started");
		}
	}
}
