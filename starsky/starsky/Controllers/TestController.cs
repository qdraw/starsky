using Microsoft.AspNetCore.Mvc;
using starsky.foundation.platform.Interfaces;
using System;
using starsky.foundation.platform.Models;
using starsky.foundation.platformSystemBindings.Trash;

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
				var result = WindowsShellTrashBindingHelper.Send("C:\\temp\\test.bmp");
				return Json(result);
			}
			
			MacOsTrashBindingHelper.Main();
			Console.WriteLine("test");
			return Json(true);
		}
	}
}
