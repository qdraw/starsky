using Microsoft.AspNetCore.Http;

namespace starsky.Helpers
{
	public static class CacheControlOverwrite
	{
			    
		/// <summary>
		/// For Performance on slow devices
		/// </summary>
		/// </summary>
		/// <param name="request"></param>
		/// <param name="time">29030400 = 4 weeks</param>
		public static void SetExpiresResponseHeaders(HttpRequest request, int time = 29030400)
		{
			request.HttpContext.Response.Headers.Remove("Cache-Control");
			request.HttpContext.Response.Headers.Add("Cache-Control", $"private,max-age={time}");
        
			request.HttpContext.Response.Headers.Remove("Expires");
			request.HttpContext.Response.Headers.Add("Expires", time.ToString());
		}
	}
}
