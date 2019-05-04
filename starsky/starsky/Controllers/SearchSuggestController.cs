using Microsoft.AspNetCore.Mvc;
using starskycore.Interfaces;
using starskycore.ViewModels;

namespace starsky.Controllers
{
	public class SearchSuggestController : Controller
	{
		private readonly ISearchSuggest _suggest;

		public SearchSuggestController(ISearchSuggest suggest) 
		{
			_suggest = suggest;
		}

		/// <summary>
		/// Gets the list of search results (cached) -- WORK IN PROGRESS
		/// </summary>
		/// <param name="t">search query</param>
		/// <param name="p">page number</param>
		/// <param name="json">enable json response</param>
		/// <returns>the search results</returns>
		/// <response code="200">the search results (enable json to get json results)</response>
		[HttpGet("/suggest")]
		[ProducesResponseType(typeof(SearchViewModel),200)] // ok
		public IActionResult Suggest(string t)
		{
			_suggest.Populate();
			
			var model = _suggest.SearchSuggest(t);
			return Json(model);
		}
	}
}
