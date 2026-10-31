using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Helpers;
using starsky.foundation.realtime.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.realtime.Services;

[TestClass]
public sealed class WebSocketConnectionsServiceTest
{
	[TestMethod]
	public async Task SendToAllAsync_success()
	{
		var logger = new FakeIWebLogger();
		var service = new WebSocketConnectionsService();
		var fakeSocket = new FakeWebSocket();
		service.AddConnection(new WebSocketConnection(fakeSocket, logger));

		await service.SendToAllAsync("test", CancellationToken.None);

		Assert.IsTrue(fakeSocket.FakeSendItems.LastOrDefault()?.StartsWith("test"));
	}

	[TestMethod]
	public async Task SendToAllAsync_WebsocketExceptionDueNoContent()
	{
		var logger = new FakeIWebLogger();
		var service = new WebSocketConnectionsService();
		var fakeSocket = new FakeWebSocket();
		service.AddConnection(new WebSocketConnection(fakeSocket, logger));

		await service.SendToAllAsync(null!, CancellationToken.None);
		Assert.HasCount(1, logger.TrackedInformation);
	}

	[TestMethod]
	public async Task SendToAllAsync_one_of_two_Fail()
	{
		var logger = new FakeIWebLogger();
		var service = new WebSocketConnectionsService();
		var fakeSocket = new FakeWebSocket(1); // mock one fail

		service.AddConnection(new WebSocketConnection(fakeSocket, logger));
		service.AddConnection(new WebSocketConnection(fakeSocket, logger));

		await service.SendToAllAsync("test", CancellationToken.None);

		// One of Two fails
		Assert.HasCount(1, fakeSocket.FakeSendItems);
		Assert.IsTrue(fakeSocket.FakeSendItems.LastOrDefault()?.StartsWith("test"));
		Assert.HasCount(1, logger.TrackedInformation);
		Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2
			?.Contains("WebSocketException"));
	}

	[TestMethod]
	public async Task SendToAllAsync_GenericException()
	{
		var logger = new FakeIWebLogger();
		var service = new WebSocketConnectionsService();
		var fakeSocket = new FakeWebSocket();
		service.AddConnection(new WebSocketConnection(fakeSocket, logger));

		const string message = "💥"; // magic string to trigger exception
		await service.SendToAllAsync(message, CancellationToken.None);
		Assert.HasCount(1, logger.TrackedExceptions);
	}

	[TestMethod]
	public async Task SendToAllAsync_Model_success()
	{
		var service = new WebSocketConnectionsService();
		var fakeSocket = new FakeWebSocket();
		var logger = new FakeIWebLogger();
		service.AddConnection(new WebSocketConnection(fakeSocket, logger));

		await service.SendToAllAsync(new ApiNotificationResponseModel<string>("test"),
			CancellationToken.None);

		var json = JsonSerializer.Serialize(
			new ApiNotificationResponseModel<string>("test"),
			DefaultJsonSerializer.CamelCaseNoEnters);
		Assert.AreEqual(json, fakeSocket.FakeSendItems.LastOrDefault());

		Assert.AreEqual("{\"data\":\"test\",\"type\":\"Unknown\"}",
			fakeSocket.FakeSendItems.LastOrDefault());
	}

	[TestMethod]
	public async Task CloseConnectionsAsync_OnlyMatchingTenantAndUser()
	{
		var logger = new FakeIWebLogger();
		var service = new WebSocketConnectionsService();
		var acmeUser1 = new FakeWebSocket();
		var acmeUser2 = new FakeWebSocket();
		var betaUser1 = new FakeWebSocket();
		service.AddConnection(new WebSocketConnection(acmeUser1, logger)
		{
			TenantId = "acme", UserId = 1
		});
		service.AddConnection(new WebSocketConnection(acmeUser2, logger)
		{
			TenantId = "acme", UserId = 2
		});
		service.AddConnection(new WebSocketConnection(betaUser1, logger)
		{
			TenantId = "beta", UserId = 1
		});

		await service.CloseConnectionsAsync("acme", 1, "revoked");

		Assert.HasCount(1, acmeUser1.FakeCloseOutputAsync);
		Assert.IsEmpty(acmeUser2.FakeCloseOutputAsync);
		Assert.IsEmpty(betaUser1.FakeCloseOutputAsync);

		// every tenant, every user
		await service.CloseConnectionsAsync(null, null, "shutdown");
		Assert.HasCount(1, acmeUser2.FakeCloseOutputAsync);
		Assert.HasCount(1, betaUser1.FakeCloseOutputAsync);
	}

	[TestMethod]
	public async Task CloseConnectionsAsync_UserInEveryTenant()
	{
		var logger = new FakeIWebLogger();
		var service = new WebSocketConnectionsService();
		var acme = new FakeWebSocket();
		var beta = new FakeWebSocket();
		var other = new FakeWebSocket();
		service.AddConnection(new WebSocketConnection(acme, logger)
		{
			TenantId = "acme", UserId = 1
		});
		service.AddConnection(new WebSocketConnection(beta, logger)
		{
			TenantId = "beta", UserId = 1
		});
		service.AddConnection(new WebSocketConnection(other, logger)
		{
			TenantId = "beta", UserId = 2
		});

		await service.CloseConnectionsAsync(null, 1, "removed");

		Assert.HasCount(1, acme.FakeCloseOutputAsync);
		Assert.HasCount(1, beta.FakeCloseOutputAsync);
		Assert.IsEmpty(other.FakeCloseOutputAsync);
	}
}
