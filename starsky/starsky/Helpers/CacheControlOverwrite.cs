using Microsoft.AspNetCore.Http;

namespace starsky.Helpers;

public static class CacheControlOverwrite
{
	private const string CacheControl = "Cache-Control";

	public static void SetNoCacheResponseHeaders(HttpRequest request)
	{
		request.HttpContext.Response.Headers.Remove(CacheControl);
		request.HttpContext.Response.Headers.Append(CacheControl,
			"no-store, no-cache, must-revalidate");
	}

	/// <summary>
	///     For Performance on slow devices
	/// </summary>
	/// <param name="request"></param>
	/// <param name="time">29030400 = 4 weeks</param>
	public static void SetExpiresResponseHeaders(HttpRequest request, int time = 29030400)
	{
		request.HttpContext.Response.Headers.Remove(CacheControl);
		request.HttpContext.Response.Headers.Append(CacheControl, $"private,max-age={time}");

		request.HttpContext.Response.Headers.Remove("Expires");
		request.HttpContext.Response.Headers.Append("Expires", time.ToString());
	}
}
