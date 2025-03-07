using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.feature.search.Interfaces;
using starsky.feature.search.ViewModels;
using starsky.Helpers;

namespace starsky.Controllers;

public sealed class SearchSuggestController : Controller
{
	private readonly ISearchSuggest _suggest;

	public SearchSuggestController(ISearchSuggest suggest)
	{
		_suggest = suggest;
	}

	/// <summary>
	///     Gets the list of search results (cached)
	/// </summary>
	/// <param name="t">search query</param>
	/// <returns>the search results</returns>
	/// <response code="200">the search results</response>
	[HttpGet("/api/suggest")]
	[ProducesResponseType(typeof(SearchViewModel), 200)] // ok
	[Produces("application/json")]
	[Authorize]
	// ^ ^ ^ ^ = = = = = = = = = = = = = = = = = =
	public async Task<IActionResult> Suggest(string t)
	{
		if ( string.IsNullOrWhiteSpace(t) )
		{
			CacheControlOverwrite.SetExpiresResponseHeaders(Request); // 4 weeks
			return Json(new List<string>());
		}

		if ( !ModelState.IsValid )
		{
			return BadRequest("Model invalid");
		}

		var model = await _suggest.SearchSuggest(t);
		return Json(model);
	}

	/// <summary>
	///     Show all items in the search suggest cache
	/// </summary>
	/// <returns>a keyList with search suggestions</returns>
	/// <response code="200">the search results</response>
	[HttpGet("/api/suggest/all")]
	[ProducesResponseType(typeof(SearchViewModel), 200)] // ok
	[Produces("application/json")]
	[Authorize]
	// ^ ^ ^ ^ = = = = = = = = = = = = = = = = = =
	public async Task<IActionResult> All()
	{
		return Json(await _suggest.GetAllSuggestions());
	}

	/// <summary>
	///     To fill the cache with the data (only if cache is not already filled)
	/// </summary>
	/// <returns></returns>
	/// <response code="200">inflate done</response>
	[HttpGet("/api/suggest/inflate")]
	[ProducesResponseType(200)] // ok
	[AllowAnonymous]
	public async Task<IActionResult> Inflate()
	{
		await _suggest.Inflate();
		return Ok();
	}
}
