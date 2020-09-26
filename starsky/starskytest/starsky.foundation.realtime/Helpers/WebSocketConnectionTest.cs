using System;
using System.Net.WebSockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.realtime.Helpers;

namespace starskytest.starsky.foundation.realtime.Helpers
{
	[TestClass]
	public class WebSocketConnectionTest
	{
		[TestMethod]
		public void Id_IsNotNull()
		{
			var socketConnection = new WebSocketConnection(new ClientWebSocket());
			Assert.IsNotNull(socketConnection.Id);
		}
		
		[TestMethod]
		public void Id_IsNot1Null()
		{
			var socketConnection = new WebSocketConnection(new ClientWebSocket());
			Assert.IsNotNull(socketConnection.Id);
		}
	}
}
