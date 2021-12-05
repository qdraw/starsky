using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.database.Models;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Services;

namespace starsky.Controllers
{
	public class TestController : Controller
	{
		private readonly IBackgroundTaskQueue _bgTaskQueue;

		public TestController(IBackgroundTaskQueue bgService)
		{
			_bgTaskQueue = bgService;
		}
		
		[HttpPost("/api/test")]
		[HttpGet("/api/test")] // < = = = = = = = = subject to change!
		[ProducesResponseType(typeof(string),200)]
		[ProducesResponseType(typeof(string),401)]
		[Produces("application/json")]	   
		public async Task<IActionResult> Index(string f)
		{
			Console.WriteLine("sdfnklsdf111");
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				Console.WriteLine("sdfknsldf");
				throw new Exception();
			});
			return Ok("Job created");
		}
	}
}
