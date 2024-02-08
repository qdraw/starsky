using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.platform.Models;

namespace starsky.Controllers
{
	[AllowAnonymous]
	public sealed class ErrorController : Controller
	{
		private readonly string _clientApp;

		public ErrorController(AppSettings appSettings)
		{
			_clientApp = Path.Combine(appSettings.BaseDirectoryProject,
				"clientapp", "build", "index.html");
		}

		/// <summary>
		/// Return Error page (HTML)
		/// </summary>
		/// <param name="statusCode">to add the status code to the response</param>
		/// <returns>Any Error html page</returns>
		[HttpGet("/error")]
		[Produces("text/html")]
		public IActionResult Error(int? statusCode = null)
		{
			if ( statusCode.HasValue )
			{
				// here is the trick
				HttpContext.Response.StatusCode = statusCode.Value;
			}

			// or "~/error/${statusCode}.html"
			return PhysicalFile(_clientApp, "text/html");
		}
	}

}
