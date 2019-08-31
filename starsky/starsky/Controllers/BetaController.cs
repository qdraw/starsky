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
		private AppSettings _appSettings;

		public BetaController(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}

		public IActionResult Index(string f = "")
		{
			var file = Path.Combine(Directory.GetCurrentDirectory(),
				"clientapp", "build", "index.html");

			return PhysicalFile(file, "text/html");
		}




		////[Route("/beta/")
		//// Route("/beta/fetchdata"), Route("/beta/counter")]
		//[Produces("text/html")]
		//public ContentResult Index1()
		//{
		//	var path = Path.Combine(_appSettings.BaseDirectoryProject, "clientapp", "build", "index.html");
		//	if ( !new StorageHostFullPathFilesystem().ExistFile(path) ) return new ContentResult
		//	{
		//		StatusCode = ( int )HttpStatusCode.NotFound,
		//		Content = path,
		//	};

		//	StreamReader reader = new StreamReader(new StorageHostFullPathFilesystem().ReadStream(path));
		//	return new ContentResult
		//	{
		//		ContentType = "text/html",
		//		StatusCode = ( int )HttpStatusCode.OK,
		//		Content = reader.ReadToEnd()
		//	};
		//}
	}
}
