using Microsoft.AspNetCore.Mvc;
using starsky.foundation.platform.Interfaces;
using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using starsky.foundation.native.Trash;
using starsky.foundation.platform.Models;
using starskycore.Attributes;

namespace starsky.Controllers
{
	[Authorize]
	[ApiExplorerSettings(IgnoreApi=true)]
	public class TestController : Controller
	{
		private readonly IWebLogger _logger;

		[ExcludeFromCoverage]
		public TestController(IWebLogger logger)
		{
			_logger = logger;
		}

		[HttpGet("/api/test/trash")]
		[Produces("application/json")]
		[ExcludeFromCoverage]
		public IActionResult Trash()
		{
			_logger.LogInformation("UserInteractive: " + Environment.UserInteractive);
			_logger.LogInformation($"use trash: {new TrashService().DetectToUseSystemTrash()}");
			
			var path = Path.Combine(new AppSettings().TempFolder, "test.bak");
			System.IO.File.WriteAllText(path, "example file content");

			new TrashService().Trash(path);
			
			return Json("ok");
		}
	}
}
