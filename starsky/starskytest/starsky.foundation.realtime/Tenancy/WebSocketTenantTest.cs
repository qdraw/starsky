using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starsky.foundation.realtime.Helpers;
using starsky.foundation.realtime.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.realtime.Tenancy;

[TestClass]
public sealed class WebSocketTenantTest
{
	public TestContext TestContext { get; set; } = null!;

	[TestCleanup]
	public void Cleanup()
	{
		TenantAmbient.Reset();
	}

	private static (WebSocketConnectionsService Service, FakeWebSocket Default, FakeWebSocket Acme,
		FakeWebSocket Beta) Create()
	{
		var service = new WebSocketConnectionsService();
		var sockets = ( new FakeWebSocket(), new FakeWebSocket(), new FakeWebSocket() );
		service.AddConnection(new WebSocketConnection(sockets.Item1, new FakeIWebLogger()));
		service.AddConnection(new WebSocketConnection(sockets.Item2, new FakeIWebLogger())
		{
			TenantId = "acme"
		});
		service.AddConnection(new WebSocketConnection(sockets.Item3, new FakeIWebLogger())
		{
			TenantId = "beta"
		});
		return ( service, sockets.Item1, sockets.Item2, sockets.Item3 );
	}

	[TestMethod]
	public async Task SendToAll_OnlyReachesTheTenantOfTheCurrentFlow()
	{
		var (service, defaultSocket, acme, beta) = Create();

		TenantAmbient.Set("acme");
		await service.SendToAllAsync("for acme", TestContext.CancellationToken);

		Assert.HasCount(1, acme.FakeSendItems);
		Assert.AreEqual("for acme", acme.FakeSendItems[0]);
		Assert.IsEmpty(defaultSocket.FakeSendItems);
		Assert.IsEmpty(beta.FakeSendItems);
	}

	[TestMethod]
	public async Task SendToAll_WithoutTenant_IsTheDefaultTenant()
	{
		var (service, defaultSocket, acme, beta) = Create();

		await service.SendToAllAsync("hello", TestContext.CancellationToken);

		Assert.HasCount(1, defaultSocket.FakeSendItems);
		Assert.IsEmpty(acme.FakeSendItems);
		Assert.IsEmpty(beta.FakeSendItems);
	}

	[TestMethod]
	public async Task SendToAll_Typed_IsTenantScopedToo()
	{
		var (service, defaultSocket, acme, beta) = Create();

		TenantAmbient.Set("beta");
		await service.SendToAllAsync(
			new ApiNotificationResponseModel<string>("data", ApiNotificationType.Welcome),
			TestContext.CancellationToken);

		Assert.HasCount(1, beta.FakeSendItems);
		Assert.IsEmpty(defaultSocket.FakeSendItems);
		Assert.IsEmpty(acme.FakeSendItems);
	}

	[TestMethod]
	public async Task SendToTenant_ByName()
	{
		var (service, defaultSocket, acme, beta) = Create();

		await service.SendToTenantAsync("acme", "x", TestContext.CancellationToken);

		Assert.HasCount(1, acme.FakeSendItems);
		Assert.IsEmpty(defaultSocket.FakeSendItems);
		Assert.IsEmpty(beta.FakeSendItems);
	}

	[TestMethod]
	public async Task Heartbeat_SendToEveryTenant_ReachesAll()
	{
		var (service, defaultSocket, acme, beta) = Create();

		await service.SendToEveryTenantAsync(
			new ApiNotificationResponseModel<string>("beat", ApiNotificationType.Heartbeat),
			TestContext.CancellationToken);

		Assert.HasCount(1, defaultSocket.FakeSendItems);
		Assert.HasCount(1, acme.FakeSendItems);
		Assert.HasCount(1, beta.FakeSendItems);
	}

	[TestMethod]
	public async Task ConcurrentFlows_DoNotSeeEachOthersTenant()
	{
		var (service, _, acme, beta) = Create();

		await Task.WhenAll(
			Task.Run(async () =>
			{
				TenantAmbient.Set("acme");
				await Task.Yield();
				await service.SendToAllAsync("a", TestContext.CancellationToken);
			}, TestContext.CancellationToken),
			Task.Run(async () =>
			{
				TenantAmbient.Set("beta");
				await Task.Yield();
				await service.SendToAllAsync("b", TestContext.CancellationToken);
			}, TestContext.CancellationToken));

		CollectionAssert.AreEqual(new[] { "a" }, acme.FakeSendItems);
		CollectionAssert.AreEqual(new[] { "b" }, beta.FakeSendItems);
	}
}
