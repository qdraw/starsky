using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starskycore.Interfaces;
using starskycore.ViewModels;

namespace starsky.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly ISearch _search;

        public SearchController(ISearch search)
        {
            _search = search;
        }

        /// <summary>
	    /// Gets the list of search results (cached)
	    /// </summary>
	    /// <param name="t">search query</param>
	    /// <param name="p">page number</param>
	    /// <param name="json">enable json response</param>
	    /// <returns>the search results</returns>
	    /// <response code="200">the search results (enable json to get json results)</response>
	    [HttpGet("/api/search")]
	    [ProducesResponseType(typeof(SearchViewModel),200)] // ok
        public IActionResult Index(string t, int p = 0)
        {
            // Json api && View()            
            var model = _search.Search(t, p);
            return Json(model);
        }

	    /// <summary>
	    /// List of files with the tag: !delete!
	    /// Caching is disabled on this api call
	    /// </summary>
	    /// <param name="p">page number</param>
	    /// <param name="json">enable json response</param>
	    /// <returns>the delete files results</returns>
	    /// <response code="200">the search results (enable json to get json results)</response>
	    [HttpGet("/api/search/trash")]
	    [ProducesResponseType(typeof(SearchViewModel),200)] // ok
        public IActionResult Trash(int p = 0)
        {
            var model = _search.Search("!delete!", p, false);
	        return Json(model);
        }

		/// <summary>
		/// Clear search cache to show the correct results
		/// </summary>
		/// <param name="t">search query</param>
		/// <returns>status</returns>
		[HttpPost("/api/search/removeCache")]
	    public IActionResult RemoveCache(string t = "")
	    {
		    var cache = _search.RemoveCache(t);

		    if ( cache != null )
			    return Json(cache == false ? "there is no cached item" : "cache cleared");
		    
		    Response.StatusCode = 412;
		    return Json("cache disabled in config");
	    }

    }
}
