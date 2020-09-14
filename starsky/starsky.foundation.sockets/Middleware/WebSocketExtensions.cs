using Microsoft.AspNetCore.Builder;
using starsky.foundation.sockets.Helpers;

namespace starsky.foundation.sockets.Middleware
{
	public static class WebSocketExtensions
	{
	    public static IApplicationBuilder UseCustomWebSocketManager(this IApplicationBuilder app)
	    {
	       return app.UseMiddleware<CustomWebSocketManager>();
	    }
	}
}
