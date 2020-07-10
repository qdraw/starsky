using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using starskycore.Services;

namespace starsky.Controllers
{
    [Authorize]
    public class ApiController : Controller
    {
        private readonly IQuery _query;
        private readonly IExifTool _exifTool;
        private readonly AppSettings _appSettings;
        private readonly IBackgroundTaskQueue _bgTaskQueue;
        private readonly IReadMeta _readMeta;
	    private readonly IStorage _iStorage;
	    private readonly IStorage _thumbnailStorage;

	    public ApiController(
            IQuery query, IExifTool exifTool, 
            AppSettings appSettings, IBackgroundTaskQueue queue,
			ISelectorStorage selectorStorage, IMemoryCache memoryCache)
        {
            _appSettings = appSettings;
            _query = query;
            _exifTool = exifTool;
            _bgTaskQueue = queue;
            _iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
            _thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
            _readMeta = new ReadMeta(_iStorage,_appSettings, memoryCache);
        }

	    /// <summary>
        /// Delete Database Cache (only the cache)
        /// </summary>
        /// <param name="f">subpath</param>
        /// <returns>redirect or if json enabled a status</returns>
        /// <response code="200">when json is true, "cache successful cleared"</response>
        /// <response code="412">"cache disabled in config"</response>
        /// <response code="400">ignored, please check if the 'f' path exist or use a folder string to clear the cache</response>
        /// <response code="302">redirect back to the url</response>
        /// <response code="401">User unauthorized</response>
        [HttpGet("/api/RemoveCache")]
        [HttpPost("/api/RemoveCache")]
        [ProducesResponseType(200)] // "cache successful cleared"
        [ProducesResponseType(412)] // "cache disabled in config"
        [ProducesResponseType(400)] // "ignored, please check if the 'f' path exist or use a folder string to clear the cache"
        [ProducesResponseType(302)] // redirect back to the url
        public IActionResult RemoveCache(string f = "/")
        {
            //For folder paths only
            if (!_appSettings.AddMemoryCache)
            {
				Response.StatusCode = 412;
				return Json("cache disabled in config");
            }

            var singleItem = _query.SingleItem(f);
            if (singleItem != null && singleItem.IsDirectory)
            {
                _query.RemoveCacheParentItem(f);
                return Json("cache successful cleared");
            }

            return BadRequest("ignored, please check if the 'f' path exist or use a folder string to clear the cache");
        }

    }
}
