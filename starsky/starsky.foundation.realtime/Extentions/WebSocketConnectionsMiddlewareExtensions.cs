using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using starsky.foundation.realtime.Middleware;
using starsky.foundation.realtime.Model;

namespace starsky.foundation.realtime.Extentions
{
	public static class WebSocketConnectionsMiddlewareExtensions
	{
		public static IApplicationBuilder MapWebSocketConnections(this IApplicationBuilder app, PathString pathMatch, WebSocketConnectionsOptions options)
		{
			if (app == null)
			{
				throw new ArgumentNullException(nameof(app));
			}

			return app.Map(pathMatch, branchedApp => branchedApp.UseMiddleware<WebSocketConnectionsMiddleware>(options));
		}
	}
}
