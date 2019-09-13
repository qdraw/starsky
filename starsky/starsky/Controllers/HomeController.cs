using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using starskycore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starskycore.Services;
using starskycore.Models;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace starsky.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		private readonly string  _clientApp;

		public HomeController()
		{
			_clientApp = Path.Combine(Directory.GetCurrentDirectory(),
				"clientapp", "build", "index.html");
		}
		
		public IActionResult Index(string f = "")
		{
			return PhysicalFile(_clientApp, "text/html");
		}

		[HttpPost("/search")]
		public IActionResult SearchPost(string t = "")
		{
			return Redirect(Request.Path.Value.ToLowerInvariant() + Request.QueryString);
		}

		[HttpGet("/search")]
		public IActionResult Search(string t= "")
		{
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

		private string CaseSensitiveRedirect(HttpRequest request)
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
