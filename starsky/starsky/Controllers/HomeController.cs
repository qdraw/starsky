using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Antiforgery;
using starsky.foundation.platform.Models;
using starsky.Helpers;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.Controllers
{
	[Authorize]
	public sealed class HomeController : Controller
	{
		private readonly string  _clientApp;
		private readonly IAntiforgery _antiForgery;

		public HomeController(AppSettings appSettings, IAntiforgery antiForgery)
		{
			_antiForgery = antiForgery;
			_clientApp = Path.Combine(appSettings.BaseDirectoryProject,
				"clientapp", "build", "index.html");
		}

		/// <summary>
		/// Home page (HTML)
		/// </summary>
		/// <param name="f">subPath</param>
		/// <returns>client app html</returns>
		/// <response code="200">client app html</response>
		/// <response code="401">Login first</response>
		[Produces("text/html")]
		[ProducesResponseType(200)]
		[ProducesResponseType(401)]
		public IActionResult Index(string f = "")
		{
			new AntiForgeryCookie(_antiForgery).SetAntiForgeryCookie(HttpContext);
			return PhysicalFile(_clientApp, "text/html");
		}

		/// <summary>
		/// Redirect to search GET page (HTML)
		/// </summary>
		/// <param name="t">search query</param>
		/// <param name="p">page number</param>
		/// <returns>client app html</returns>
		/// <response code="301">redirect to get page</response>
		/// <response code="401">Login first</response>
		[Produces("text/html")]
		[ProducesResponseType(301)]
		[ProducesResponseType(401)]
		[HttpPost("/search")]
		public IActionResult SearchPost(string t = "", int p = 0)
		{
			if (string.IsNullOrEmpty(t))
			{
				return Redirect($"/search");
			}
			
			// Added filter to prevent redirects based on tainted, user-controlled data
			// unescaped: ^[a-zA-Z0-9_\-+"'/=:,\.>< ]+$
			if ( !Regex.IsMatch(t, "^[a-zA-Z0-9_\\-+\"'/=:,\\.>< ]+$", 
				RegexOptions.None, TimeSpan.FromMilliseconds(100)) )
			{
				return BadRequest("`t` is not allowed");
			}
			return Redirect(AppendPathBasePrefix(Request.PathBase.Value,$"/search?t={t}&p={p}"));
		}

		/// <summary>
		/// Search GET page (HTML)
		/// </summary>
		/// <param name="t">search query</param>
		/// <param name="p">page number</param>
		/// <returns>client app html</returns>
		/// <response code="200">client app html</response>
		/// <response code="301">Is Case Sensitive Redirect</response>
		/// <response code="400">input not allowed</response>
		/// <response code="401">Login first</response>
		[Produces("text/html")]
		[ProducesResponseType(200)]
		[ProducesResponseType(301)]
		[ProducesResponseType(400)]
		[ProducesResponseType(401)]
		[HttpGet("/search")]
		public IActionResult Search(string t= "", int p = 0)
		{
			new AntiForgeryCookie(_antiForgery).SetAntiForgeryCookie(HttpContext);

			if ( !IsCaseSensitiveRedirect("/search", Request.Path.Value) )
				return PhysicalFile(_clientApp, "text/html");
			
			// if not case sensitive is already served
			if (string.IsNullOrEmpty(t))
			{
				return Redirect(AppendPathBasePrefix(Request.PathBase.Value,$"/search"));
			}

			// Added filter to prevent redirects based on tainted, user-controlled data
			// unescaped: ^[a-zA-Z0-9_\-+"'/=:>< ]+$
			if (!Regex.IsMatch(t, "^[a-zA-Z0-9_\\-+\"'/=:>< ]+$", 
				RegexOptions.None, TimeSpan.FromMilliseconds(100)) )
			{
				return BadRequest("`t` is not allowed");
			}
			return Redirect(AppendPathBasePrefix(Request.PathBase.Value,$"/search?t={t}&p={p}"));
		}
		
		/// <summary>
		/// Trash page (HTML)
		/// </summary>
		/// <param name="p">page number</param>
		/// <returns>client app html</returns>
		/// <response code="200">client app html</response>
		/// <response code="301">Is Case Sensitive Redirect</response>
		/// <response code="401">Login first</response>
		[Produces("text/html")]
		[ProducesResponseType(200)]
		[ProducesResponseType(301)]
		[ProducesResponseType(401)]
		[HttpGet("/trash")]
		public IActionResult Trash( int p = 0)
		{
			if ( IsCaseSensitiveRedirect("/trash", Request.Path.Value) )
			{
				return Redirect(AppendPathBasePrefix(Request.PathBase.Value,$"/trash?p={p}"));
			}
			return PhysicalFile(_clientApp, "text/html");
		}

		/// <summary>
		/// Import page (HTML)
		/// </summary>
		/// <returns>client app html</returns>
		/// <response code="200">client app html</response>
		/// <response code="301">Is Case Sensitive Redirect</response>
		[Produces("text/html")]
		[ProducesResponseType(200)]
		[ProducesResponseType(301)]
		[HttpGet("/import")]
		public IActionResult Import()
		{
			if ( IsCaseSensitiveRedirect("/import", Request.Path.Value) )
			{
				return Redirect(AppendPathBasePrefix(Request.PathBase.Value,$"/import"));
			}
			return PhysicalFile(_clientApp, "text/html");
		}
		
		/// <summary>
		/// Preferences page (HTML)
		/// </summary>
		/// <returns>client app html</returns>
		/// <response code="200">client app html</response>
		/// <response code="301">Is Case Sensitive Redirect</response>
		[Produces("text/html")]
		[ProducesResponseType(200)]
		[ProducesResponseType(301)]
		[HttpGet("/preferences")]
		public IActionResult Preferences()
		{
			if ( IsCaseSensitiveRedirect("/preferences", Request.Path.Value) )
			{
				return Redirect(AppendPathBasePrefix(Request.PathBase.Value,$"/preferences"));
			}
			return PhysicalFile(_clientApp, "text/html");
		}
		
		/// <summary>
		/// View the Register form (HTML)
		/// </summary>
		/// <param name="returnUrl">when successful continue</param>
		/// <returns>client app html</returns>
		/// <response code="200">successful Register-page</response>
		[HttpGet("/account/register")]
		[AllowAnonymous]
		[ProducesResponseType(200)]
		[Produces("text/html")]
		[ProducesResponseType(200)]
		public IActionResult Register(string returnUrl = null)
		{
			new AntiForgeryCookie(_antiForgery).SetAntiForgeryCookie(HttpContext);
			return PhysicalFile(_clientApp, "text/html");
		}

		internal static string AppendPathBasePrefix(string? requestPathBase, string url)
		{
			return requestPathBase?.Equals("/starsky", 
				StringComparison.InvariantCultureIgnoreCase) == true ? $"/starsky{url}" : url;
		}

		internal static bool IsCaseSensitiveRedirect(string? expectedRequestPath, string? requestPathValue)
		{
			return expectedRequestPath != requestPathValue;
		}
	}
}
