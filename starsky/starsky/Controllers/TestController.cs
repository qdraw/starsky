using Microsoft.AspNetCore.Mvc;
using starsky.foundation.platform.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using starsky.foundation.native.Helpers;
using starsky.foundation.native.Trash;
using starsky.foundation.platform.Models;

namespace starsky.Controllers
{
	public class TestController : Controller
	{
		private IWebLogger _logger;

		public TestController(IWebLogger logger)
		{
			_logger = logger;
		}

		[HttpGet("/api/test/trash")]
		[Produces("application/json")]
		public IActionResult Trash()
		{
			_logger.LogInformation("UserInteractive: " + Environment.UserInteractive);

			if ( new AppSettings().IsWindows )
			{
				var result = WindowsShellTrashBindingHelper.Trash("C:\\temp\\test.bmp", OperatingSystemHelper.GetPlatform());
				return Json(result);
			}
			
			var testFile = "/tmp/test/test.jpg";
			System.IO.File.WriteAllText(testFile, "example file content");
			
			MacOsTrashBindingHelper.Trash(new List<string>{testFile}, OperatingSystemHelper.GetPlatform());
			
			return Json("ok");
		}
	}
}
