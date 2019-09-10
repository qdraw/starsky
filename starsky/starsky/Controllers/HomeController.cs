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
		public IActionResult Index(string f = "")
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(),
				"clientapp", "build", "index.html");

			return PhysicalFile(file, "text/html");
		}

		[HttpGet("/search")]
		public IActionResult Search(string t= "")
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(),
				"clientapp", "build", "index.html");

			return PhysicalFile(file, "text/html");
		}
		
		// Error pages should be always visible
		// curl: (47) Maximum (50) redirects followed
		[AllowAnonymous]
		public IActionResult Error()
		{
			return View();
		}
	}
}
