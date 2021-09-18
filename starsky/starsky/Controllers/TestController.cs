using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.mail.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.Controllers
{
	[Authorize]
	public class TestController : Controller
	{
		private readonly AppSettings _appSettings;
		private readonly IWebLogger _logger;

		public TestController(AppSettings appSettings, IWebLogger logger)
		{
			_appSettings = appSettings;
			_logger = logger;
		}
		
		[HttpGet("/test/test")]
		public async Task<IActionResult> Tetung(string f)
		{
			var text = @"Hey Chandler,

I just wanted to let you know that Monica and I were going to go play some paintball, you in?

-- Joey";
			
			await new SendMail(_appSettings, _logger).SendAsync("dionvanvelde@gmail.com","","test", text);
			return Ok();
		}

		
	}
}
