using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using starsky.foundation.realtime.Middleware;
using starsky.foundation.realtime.Model;

namespace starsky.foundation.realtime.Extentions
{
	public static class WebSocketConnectionsMiddlewareExtensions
	{
		public static IApplicationBuilder MapWebSocketConnections(this IApplicationBuilder app, 
			PathString pathMatch, WebSocketConnectionsOptions options, 
			bool? featureToggleEnabled = true)
		{
			if (app == null)
			{
				throw new ArgumentNullException(nameof(app));
			}

			return app.Map(pathMatch, branchedApp =>
			{
				if ( featureToggleEnabled == true )
				{
					branchedApp.UseMiddleware<WebSocketConnectionsMiddleware>(options);
					return;
				}
				branchedApp.UseMiddleware<DisabledWebSocketsMiddleware>();
			});
		}
	}
}
