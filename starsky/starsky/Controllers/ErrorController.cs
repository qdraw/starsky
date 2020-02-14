using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace starsky.Controllers
{
	public class ErrorController : Controller  
	{
		private readonly string  _clientApp;

		public ErrorController()
		{
			_clientApp = Path.Combine(Directory.GetCurrentDirectory(),
				"clientapp", "build", "index.html");
		}
		
		/// <summary>
		/// Return Error page
		/// </summary>
		/// <param name="statusCode">to add the status code to the response</param>
		/// <returns>Any Error html page</returns>
		[HttpGet("/error")]
		[Produces("text/html")]
		public IActionResult Error(int? statusCode = null)
		{
			if (statusCode.HasValue)
			{
				// here is the trick
				HttpContext.Response.StatusCode = statusCode.Value;
			}

			// or "~/error/${statusCode}.html"
			return PhysicalFile(_clientApp, "text/html");
		}
	}

}
