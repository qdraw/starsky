using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.Controllers
{
    [Authorize]
    public class CacheIndexController : Controller
    {
        private readonly IQuery _query;
        private readonly AppSettings _appSettings;

	    public CacheIndexController(
            IQuery query, AppSettings appSettings)
        {
            _appSettings = appSettings;
            _query = query;
        }
	    
	    /// <summary>
	    /// Get Database Cache (only the cache)
	    /// </summary>
	    /// <param name="f">subPath (only direct so no dot;comma list)</param>
	    /// <returns>redirect or if json enabled a status</returns>
	    /// <response code="200">when json"</response>
	    /// <response code="412">"cache disabled in config"</response>
	    /// <response code="400">ignored, please check if the 'f' path exist or use a folder string to clear the cache</response>
	    /// <response code="401">User unauthorized</response>
	    [HttpGet("/api/cache/list")]
	    public IActionResult ListCache(string f = "/")
	    {
		    //For folder paths only
		    if (_appSettings.AddMemoryCache == false)
		    {
			    Response.StatusCode = 412;
			    return Json("cache disabled in config");
		    }

		    var (success, singleItem) = _query.CacheGetParentFolder(f);
		    if ( !success || singleItem == null )
			    return BadRequest(
				    "ignored, please check if the 'f' path exist or use a folder string to get the cache");
            
		    return Json(singleItem);
	    }

	    /// <summary>
        /// Delete Database Cache (only the cache)
        /// </summary>
        /// <param name="f">subPath (only direct so no dot;comma list)</param>
        /// <returns>redirect or if json enabled a status</returns>
        /// <response code="200">when json is true, "cache successful cleared"</response>
        /// <response code="412">"cache disabled in config"</response>
        /// <response code="400">ignored, please check if the 'f' path exist or use a folder string to clear the cache</response>
        /// <response code="302">redirect back to the url</response>
        /// <response code="401">User unauthorized</response>
        [HttpGet("/api/remove-cache")]
        [HttpPost("/api/remove-cache")]
        [ProducesResponseType(200)] // "cache successful cleared"
        [ProducesResponseType(412)] // "cache disabled in config"
        [ProducesResponseType(400)] // "ignored, please check if the 'f' path exist or use a folder string to clear the cache"
        [ProducesResponseType(302)] // redirect back to the url
        public IActionResult RemoveCache(string f = "/")
        {
            //For folder paths only
            if (_appSettings.AddMemoryCache == false)
            {
				Response.StatusCode = 412;
				return Json("cache disabled in config");
            }

            var singleItem = _query.SingleItem(f);
            if ( singleItem == null || !singleItem.IsDirectory )
	            return BadRequest(
		            "ignored, please check if the 'f' path exist or use a folder string to clear the cache");
            
            return Json(_query.RemoveCacheParentItem(f) ? "cache successful cleared" : "cache did not exist");
        }

    }
}
