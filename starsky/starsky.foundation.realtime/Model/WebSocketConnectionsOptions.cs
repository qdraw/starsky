using System.Collections.Generic;

namespace starsky.foundation.realtime.Model
{
	public sealed class WebSocketConnectionsOptions
	{
		public HashSet<string> AllowedOrigins { get; set; }

		public int ReceivePayloadBufferSize { get; set; }

		public WebSocketConnectionsOptions()
		{
			ReceivePayloadBufferSize = 4 * 1024;
		}
	}
}
