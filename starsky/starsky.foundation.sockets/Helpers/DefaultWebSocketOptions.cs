using System;
using Microsoft.AspNetCore.Builder;

namespace starsky.foundation.sockets.Helpers
{
	public static class DefaultWebSocketOptions
	{
		public static WebSocketOptions GetDefault()
		{
			return new WebSocketOptions()
			{
				KeepAliveInterval = TimeSpan.FromSeconds(120),
				ReceiveBufferSize = 4 * 1024
			};
		}
	}
}
