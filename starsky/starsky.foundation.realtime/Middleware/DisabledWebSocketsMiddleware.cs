using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace starsky.foundation.realtime.Middleware
{
	public class DisabledWebSocketsMiddleware
	{
		public DisabledWebSocketsMiddleware(RequestDelegate next)
		{
		}

#pragma warning disable 1998
		public async Task Invoke(HttpContext context)
#pragma warning restore 1998
		{
			context.Response.StatusCode = 204; 
		}
	}
}
