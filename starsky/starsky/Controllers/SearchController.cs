using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Models;
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
	    /// <returns>the search results</returns>
	    /// <response code="200">the search results</response>
	    [HttpGet("/api/search")]
	    [ProducesResponseType(typeof(SearchViewModel),200)] // ok
	    [Produces("application/json")]
        public IActionResult Index(string t, int p = 0)
        {
            var model = _search.Search(t, p);
            return Json(model);
        }
        
        /// <summary>
        /// Get relative paths in a search query
        /// Does not cover multiple pages (so it ends within the page)
        /// </summary>
        /// <param name="f">subpath</param>
        /// <param name="t">search query</param>
        /// <param name="p">pagenumer (search query)</param>
        /// <returns>Relative object (only this)</returns>
        /// <response code="200">the search results</response>
        [HttpGet("/api/search/relativeObjects")]
        [ProducesResponseType(typeof(SearchViewModel),200)] // ok
        [Produces("application/json")]
        public IActionResult SearchRelative(string f, string t, int p = 0)
        {
	        // Json api && View()            
	        var searchViewModel = _search.Search(t, p);

	        var photoIndexOfQuery = GetIndexFilePathFromSearch(searchViewModel,f);
	        if ( photoIndexOfQuery == -1 ) return NotFound("image not found in search result");
	        
	        var args = new Dictionary<string, string>
	        {
		        { "p", p.ToString() },
		        { "t", t }
	        };
	        
	        var relativeObject = new RelativeObjects{Args = args};

	        if (photoIndexOfQuery != searchViewModel.FileIndexItems.Count - 1 )
	        {
		        relativeObject.NextFilePath = searchViewModel.FileIndexItems[photoIndexOfQuery + 1]?.FilePath;
		        relativeObject.NextHash = searchViewModel.FileIndexItems[photoIndexOfQuery + 1]?.FileHash;
	        }

	        if (photoIndexOfQuery >= 1)
	        {
		        relativeObject.PrevFilePath = searchViewModel.FileIndexItems[photoIndexOfQuery - 1]?.FilePath;
		        relativeObject.PrevHash = searchViewModel.FileIndexItems[photoIndexOfQuery - 1]?.FileHash;
	        }
	        
	        return Json(relativeObject);
        }

        /// <summary>
        /// Get the index number (fallback == -1)
        /// </summary>
        /// <param name="searchViewModel">search results model</param>
        /// <param name="f">subpath to search for</param>
        /// <returns>int as index, fallback == -1</returns>
        private int GetIndexFilePathFromSearch(SearchViewModel searchViewModel, string f)
        {
	        var result = searchViewModel.FileIndexItems.FirstOrDefault(p => p.FilePath == f);
	        var photoIndexOfQuery = searchViewModel.FileIndexItems.IndexOf(result);
	        if ( result == null ) return -1;
	        return photoIndexOfQuery;
        }
        
	    /// <summary>
	    /// List of files with the tag: !delete!
	    /// Caching is disabled on this api call
	    /// </summary>
	    /// <param name="p">page number</param>
	    /// <returns>the delete files results</returns>
	    /// <response code="200">the search results</response>
	    [HttpGet("/api/search/trash")]
	    [ProducesResponseType(typeof(SearchViewModel),200)] // ok
	    [Produces("application/json")]
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
		/// <response code="200">cache is clear for this search query</response>
		/// <response code="412">Cache is disabled in config</response>
		/// <response code="401">User unauthorized</response>
		[HttpPost("/api/search/removeCache")]
		[Produces("application/json")]	    
		[ProducesResponseType(typeof(string),200)]
		[ProducesResponseType(typeof(string),412)]
		[ProducesResponseType(401)]
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
