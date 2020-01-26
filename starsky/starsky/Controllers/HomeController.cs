using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
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
		public IActionResult SearchPost(string t = "")
		{
			return Redirect($"/search{Request.QueryString}");
		}

		[HttpGet("/search")]
		public IActionResult Search(string t= "")
		{
			new AntiForgeryCookie(_antiForgery).SetAntiForgeryCookie(HttpContext);
			if ( !string.IsNullOrEmpty(CaseSensitiveRedirect(Request)) )
			{
				return Redirect(CaseSensitiveRedirect(Request));
			}
			return PhysicalFile(_clientApp, "text/html");
		}
		
		[HttpGet("/trash")]
		public IActionResult Trash()
		{
			if ( !string.IsNullOrEmpty(CaseSensitiveRedirect(Request)) )
			{
				return Redirect(CaseSensitiveRedirect(Request));
			}
			return PhysicalFile(_clientApp, "text/html");
		}

		[HttpGet("/import")]
		public IActionResult Import()
		{
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

				


		internal string CaseSensitiveRedirect(HttpRequest request)
		{
			if ( request.Path.Value != request.Path.Value.ToLowerInvariant() )
			{
				return request.Path.Value.ToLowerInvariant() + request.QueryString;
			}
			return string.Empty;
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
