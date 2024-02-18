using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace starsky.foundation.realtime.Middleware
{
	[SuppressMessage("Performance", "CA1822:Mark members as static")]
	[SuppressMessage("ReSharper", "UnusedParameter.Local")]
	[SuppressMessage("Performance", "IDE0060:UnusedParameter.Local")]
	public sealed class DisabledWebSocketsMiddleware
	{
		[SuppressMessage("ReSharper", "UnusedParameter.Local")]
		[SuppressMessage("Usage", "IDE0060:Remove unused parameter")]
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
