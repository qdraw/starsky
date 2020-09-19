using System.Net.WebSockets;

namespace starsky.foundation.sockets.Models
{
	public class RealtimeWebSocket
	{
		public WebSocket WebSocket { get; set; }
		public string Id { get; set; }
	}
}
