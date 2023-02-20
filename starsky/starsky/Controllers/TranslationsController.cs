using Microsoft.AspNetCore.Mvc;
using starsky.feature.translations;

namespace starsky.Controllers;

public class TranslationsController : Controller
{
	/// <summary>
	/// (beta) Move a file to the trash
	/// </summary>
	/// <param name="f">subPath filepath to file, split by dot comma (;)</param>
	/// <returns>update json (IActionResult Update)</returns>
	/// <response code="200">the item including the updated content</response>
	[ProducesResponseType(typeof(string), 400)]
	[HttpGet("/api/translations/get")]
	[Produces("application/json")]
	public IActionResult TrashMoveAsync(string f)
	{
		return Json(new ContentTranslations().GetDictionary());
	}
}
