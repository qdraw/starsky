using System;

namespace starsky.foundation.sockets.Models
{
	public class CustomWebSocketMessage
	{
		public object Data { get; set; }
		public Guid? RequestId { get; set; }
		public string Id { get; set; }
	}
}
