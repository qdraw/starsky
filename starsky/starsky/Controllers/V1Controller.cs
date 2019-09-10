using Microsoft.AspNetCore.Mvc;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.ViewModels;

namespace starsky.Controllers
{
	public class V1Controller : Controller
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly ISearch _search;

		public V1Controller(IQuery query, ISearch search, AppSettings appsettings = null)
		{
			_query = query;
			_appSettings = appsettings;
			_search = search;
		}
		

		// GET
		public IActionResult Index(
			string f = "/",
			string colorClass = null,
			bool collections = true,
			bool hidedelete = true
		)
		{
			var homeController = new IndexController(_query, _appSettings)
			{
				ControllerContext =
				{
					HttpContext = HttpContext,
				},
				Url = Url
			};
			return homeController.Index(f, colorClass, false, collections, hidedelete);
		}
		
		/// <summary>
		/// Gets the list of search results (cached)
		/// </summary>
		/// <param name="t">search query</param>
		/// <param name="p">page number</param>
		/// <param name="json">enable json response</param>
		/// <returns>the search results</returns>
		/// <response code="200">the search results (enable json to get json results)</response>
		[HttpGet("/v1/search")]
		[ProducesResponseType(typeof(SearchViewModel),200)] // ok
		public IActionResult Search(string t, int p = 0, bool json = false)
		{
			var model = _search.Search(t, p);
			return View("Search", model);
		}
		
		/// <summary>
		/// Post a form to search and redirect to the first page (no json)
		/// </summary>
		/// <param name="t">search query</param>
		/// <returns>redirect to search page</returns>
		/// <response code="301">redirect to search page (no json)</response>
		[HttpPost("/v1/search")]
		[ProducesResponseType(301)] // redirect
		public IActionResult IndexPost(string t)
		{
			return RedirectToAction("Search", new {t, p = 0 });
		}
		
		[HttpGet("/v1/import")]
		public IActionResult Import()
		{
			return View("Import");
		}

		
		/// <summary>
		/// List of files with the tag: !delete!
		/// Caching is disabled on this api call
		/// </summary>
		/// <param name="p">page number</param>
		/// <param name="json">enable json response</param>
		/// <returns>the delete files results</returns>
		/// <response code="200">the search results (enable json to get json results)</response>
		[HttpGet("/v1/search/trash")]
		[ProducesResponseType(typeof(SearchViewModel),200)] // ok
		public IActionResult Trash(int p = 0, bool json = false)
		{
			var model = _search.Search("!delete!", p, false);
			if (json) return Json(model);
			return View("~/Views/V1/trash.cshtml", model);
		}

	}
}
