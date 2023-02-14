using Microsoft.AspNetCore.Mvc;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Trash;
using System;

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
			var result = new WindowsShellTrashBindingHelper(_logger).Send("C:\\temp\\test.bmp");
			return Json(result);
		}
	}
}
