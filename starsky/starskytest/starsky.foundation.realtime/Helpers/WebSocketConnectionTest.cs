using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.realtime.Helpers;
using starskytest.FakeMocks;

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
		public void CloseStatus_IsNull()
		{
			var socketConnection = new WebSocketConnection(new ClientWebSocket());
			Assert.IsNull(socketConnection.CloseStatus);
		}
		
		[TestMethod]
		public void CloseStatusDescription_IsNull()
		{
			var socketConnection = new WebSocketConnection(new ClientWebSocket());
			Assert.IsNull(socketConnection.CloseStatusDescription);
		}
		
		[TestMethod]
		public async Task SendItems()
		{
			var fakeSocket = new FakeWebSocket();
			var socketConnection = new WebSocketConnection(fakeSocket);
			await socketConnection.SendAsync("test", CancellationToken.None);

			Assert.IsNotNull(fakeSocket.FakeSendItems.LastOrDefault());
			Assert.AreEqual("test", fakeSocket.FakeSendItems.LastOrDefault());
		}
		


		[TestMethod]
		[Timeout(600)]
		public async Task ReceiveMessagesUntilCloseAsync_And_Exit()
		{
		
			var fakeSocket = new FakeWebSocket();
			var socketConnection = new WebSocketConnection(fakeSocket);

			var message = "";
			socketConnection.ReceiveText += (sender, s) =>
			{
				message = s;
			};
			
			// when this unit test keeps hanging the end signal has not passed correctly
			await socketConnection.ReceiveMessagesUntilCloseAsync();

			Assert.IsTrue(message.StartsWith("message"));
		}
		
		[TestMethod]
		[Timeout(600)]
		public async Task ReceiveMessagesUntil_ConnectionClosedPrematurely_And_Exit()
		{

			var fakeSocket = new FakeWebSocket
			{
				ReceiveAsyncErrorType = WebSocketError.ConnectionClosedPrematurely
			};
			var socketConnection = new WebSocketConnection(fakeSocket);

			var message = "";
			socketConnection.ReceiveText += (sender, s) =>
			{
				message = s;
			};
			
			// when this unit test keeps hanging the end signal has not passed correctly
			await socketConnection.ReceiveMessagesUntilCloseAsync();

			Assert.IsTrue(message.StartsWith("message"));
		}
	}
}
