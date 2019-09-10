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

		[HttpGet("/search")]
		public IActionResult Search(string t= "")
		{
			return PhysicalFile(_clientApp, "text/html");
		}
		
		[HttpGet("/trash")]
		public IActionResult Trash()
		{
			return PhysicalFile(_clientApp, "text/html");
		}

		[HttpGet("/import")]
		public IActionResult Import()
		{
			return PhysicalFile(_clientApp, "text/html");
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
