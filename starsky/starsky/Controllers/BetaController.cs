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
	public class BetaController : Controller
	{
		public IActionResult Index(string f = "")
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(),
				"clientapp", "build", "index.html");

			return PhysicalFile(file, "text/html");
		}

		public IActionResult Search(string t= "")
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(),
				"clientapp", "build", "index.html");

			return PhysicalFile(file, "text/html");
		}
	}
}
