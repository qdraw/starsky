using Microsoft.AspNetCore.Mvc;
using starsky.foundation.platform.Interfaces;
using System;
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
				var result = WindowsShellTrashBindingHelper.Send("C:\\temp\\test.bmp");
				return Json(result);
			}
			
			MacOsTrashBindingHelper.Main();
			
			Console.WriteLine("test");
			var content = OsxClipboard.GetText();
			return Json(content);
		}
	}
}
