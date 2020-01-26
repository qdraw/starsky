using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Antiforgery;
using starsky.Helpers;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		private readonly string  _clientApp;
		private readonly IAntiforgery _antiForgery;


		public HomeController(IAntiforgery antiForgery)
		{
			_antiForgery = antiForgery;
			_clientApp = Path.Combine(Directory.GetCurrentDirectory(),
				"clientapp", "build", "index.html");
		}

	
		public IActionResult Index(string f = "")
		{
			new AntiForgeryCookie(_antiForgery).SetAntiForgeryCookie(HttpContext);
			return PhysicalFile(_clientApp, "text/html");
		}

		[HttpPost("/search")]
		public IActionResult SearchPost(string t = "", int p = 0)
		{
			return Redirect($"/search?t={t}&p={p}");
		}

		[HttpGet("/search")]
		public IActionResult Search(string t= "", int p = 0)
		{
			new AntiForgeryCookie(_antiForgery).SetAntiForgeryCookie(HttpContext);
			if ( IsCaseSensitiveRedirect("/search", Request.Path.Value) )
			{
				return Redirect($"/search?t={t}&p={p}");
			}
			return PhysicalFile(_clientApp, "text/html");
		}
		
		[HttpGet("/trash")]
		public IActionResult Trash( int p = 0)
		{
			if ( IsCaseSensitiveRedirect("/trash", Request.Path.Value) )
			{
				return Redirect($"/trash?p={p}");
			}
			return PhysicalFile(_clientApp, "text/html");
		}

		[HttpGet("/import")]
		public IActionResult Import()
		{
			if ( IsCaseSensitiveRedirect("/import", Request.Path.Value) )
			{
				return Redirect($"/import");
			}
			return PhysicalFile(_clientApp, "text/html");
		}
		
		/// <summary>
		/// View the Register form
		/// </summary>
		/// <param name="returnUrl">when successful continue</param>
		/// <returns></returns>
		/// <response code="200">successful Register-page</response>
		[HttpGet("/account/register")]
		[AllowAnonymous]
		[ProducesResponseType(200)]
		public IActionResult Register(string returnUrl = null)
		{
			new AntiForgeryCookie(_antiForgery).SetAntiForgeryCookie(HttpContext);
			return PhysicalFile(_clientApp, "text/html");
		}

		internal bool IsCaseSensitiveRedirect(string expectedRequestPath, string requestPathValue)
		{
			return expectedRequestPath != requestPathValue;
		}
		
		// Error pages should be always visible
		// curl: (47) Maximum (50) redirects followed
		[AllowAnonymous]
		public IActionResult Error()
		{
			return PhysicalFile(_clientApp, "text/html");
		}
	}
}
