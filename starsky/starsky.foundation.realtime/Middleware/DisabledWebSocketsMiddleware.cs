using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace starsky.foundation.realtime.Middleware
{
	public class DisabledWebSocketsMiddleware
	{
		public DisabledWebSocketsMiddleware(RequestDelegate next)
		{
		}

		public async Task Invoke(HttpContext context)
		{
			if ( context.WebSockets.IsWebSocketRequest )
			{
				var webSocket = await context.WebSockets.AcceptWebSocketAsync();
				// StatusCode MessageTooBig = 1009
				await webSocket.CloseOutputAsync(WebSocketCloseStatus.MessageTooBig, 
					"Feature toggle disabled", CancellationToken.None);
				return;
			}
			context.Response.StatusCode = StatusCodes.Status400BadRequest;
		}
	}
}
