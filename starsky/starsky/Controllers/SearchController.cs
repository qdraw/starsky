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
        private readonly ISearchSuggest _suggest;

        public SearchController(ISearch search, ISearchSuggest suggest)
        {
            _search = search;
            _suggest = suggest;
        }
        
	    /// <summary>
	    /// Post a form to search and redirect to the first page (no json)
	    /// </summary>
	    /// <param name="t">search query</param>
	    /// <returns>redirect to search page</returns>
	    /// <response code="301">redirect to search page (no json)</response>
	    [HttpPost("/search")]
	    [ProducesResponseType(301)] // redirect
	    public IActionResult IndexPost(string t)
        {
			return RedirectToAction("Index", new {t, p = 0 });
        }

	    /// <summary>
	    /// Gets the list of search results (cached)
	    /// </summary>
	    /// <param name="t">search query</param>
	    /// <param name="p">page number</param>
	    /// <param name="json">enable json response</param>
	    /// <returns>the search results</returns>
	    /// <response code="200">the search results (enable json to get json results)</response>
	    [HttpGet("/search")]
	    [ProducesResponseType(typeof(SearchViewModel),200)] // ok
        public IActionResult Index(string t, int p = 0, bool json = false)
        {
            // Json api && View()            
            var model = _search.Search(t, p);
            if (json) return Json(model);
            return View("Index", model);
        }

	    /// <summary>
	    /// List of files with the tag: !delete!
	    /// Caching is disabled on this api call
	    /// </summary>
	    /// <param name="p">page number</param>
	    /// <param name="json">enable json response</param>
	    /// <returns>the delete files results</returns>
	    /// <response code="200">the search results (enable json to get json results)</response>
	    [HttpGet("/search/trash")]
	    [ProducesResponseType(typeof(SearchViewModel),200)] // ok
        public IActionResult Trash(int p = 0, bool json = false)
        {
            var model = _search.Search("!delete!", p, false);
            if (json) return Json(model);
            return View("Trash", model);
        }

        //public IActionResult Error()
        //{
        //    // copy to controller, this one below is only for copying
        //    Response.StatusCode = 404;
        //    return View();
        //}
        
        /// <summary>
        /// Gets the list of search results (cached) -- WORK IN PROGRESS
        /// </summary>
        /// <param name="t">search query</param>
        /// <param name="p">page number</param>
        /// <param name="json">enable json response</param>
        /// <returns>the search results</returns>
        /// <response code="200">the search results (enable json to get json results)</response>
        [HttpGet("/search/suggest")]
        [ProducesResponseType(typeof(SearchViewModel),200)] // ok
        public IActionResult Suggest(string t)
        {
	        // Json api && View()            
	        var model = _suggest.SearchSuggest(t);
	        return Json(model);
        }
    }
}
