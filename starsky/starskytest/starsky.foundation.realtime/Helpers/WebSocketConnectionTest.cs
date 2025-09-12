using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.realtime.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.realtime.Helpers;

[TestClass]
public sealed class WebSocketConnectionTest
{
	[TestMethod]
	public void Id_IsNotNull()
	{
		var socketConnection = new WebSocketConnection(new ClientWebSocket(), new FakeIWebLogger());
		Assert.AreNotEqual(string.Empty, socketConnection.Id.ToString());
	}

	[TestMethod]
	public void CloseStatus_IsNull()
	{
		var socketConnection = new WebSocketConnection(new ClientWebSocket(), new FakeIWebLogger());
		Assert.IsNull(socketConnection.CloseStatus);
	}

	[TestMethod]
	public void CloseStatusDescription_IsNull()
	{
		var socketConnection = new WebSocketConnection(new ClientWebSocket(), new FakeIWebLogger());
		Assert.IsNull(socketConnection.CloseStatusDescription);
	}

	[TestMethod]
	public async Task SendItems()
	{
		var fakeSocket = new FakeWebSocket();
		var socketConnection = new WebSocketConnection(fakeSocket, new FakeIWebLogger());
		await socketConnection.SendAsync("test", CancellationToken.None);

		Assert.IsNotNull(fakeSocket.FakeSendItems.LastOrDefault());
		Assert.AreEqual("test", fakeSocket.FakeSendItems.LastOrDefault());
	}


	[TestMethod]
	[Timeout(2000)]
	public async Task ReceiveMessagesUntilCloseAsync_And_Exit()
	{
		var fakeSocket = new FakeWebSocket();
		var socketConnection = new WebSocketConnection(fakeSocket, new FakeIWebLogger());

		var message = "";
		socketConnection.ReceiveText += (sender, s) => { message = s; };

		// when this unit test keeps hanging the end signal has not passed correctly
		await socketConnection.ReceiveMessagesUntilCloseAsync();

		Assert.StartsWith("message", message);
	}

	[TestMethod]
	[Timeout(2000, CooperativeCancellation = true)]
	public async Task ReceiveMessagesUntil_ConnectionClosedPrematurely_And_Exit()
	{
		var fakeSocket = new FakeWebSocket
		{
			ReceiveAsyncErrorType = WebSocketError.ConnectionClosedPrematurely
		};
		var socketConnection = new WebSocketConnection(fakeSocket, new FakeIWebLogger());

		var message = "";
		socketConnection.ReceiveText += (_, s) => { message = s; };

		// when this unit test keeps hanging the end signal has not passed correctly
		await socketConnection.ReceiveMessagesUntilCloseAsync();

		Assert.StartsWith("message", message);
	}
}
