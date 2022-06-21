using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace starsky.foundation.accountmanagement.Middleware
{
	public class AntiForgeryCookieHeaderMiddleware
	{
		private readonly RequestDelegate _next;

		public AntiForgeryCookieHeaderMiddleware(RequestDelegate next)
		{
			_next = next;
		}
		
		/// <summary>
		/// Add header based on cookies
		/// </summary>
		/// <param name="context"></param>
		public async Task Invoke(HttpContext context)
		{
			if ( context.Request.Path.Value?.Contains("api/account/register") == true )
			{
				var tokenValue = context.Request.Cookies.FirstOrDefault(p =>
					p.Key == "X-XSRF-TOKEN").Value;
				context.Request.Headers.TryAdd("X-XSRF-TOKEN", tokenValue);
			}
			await _next.Invoke(context);
		}
	}

}

