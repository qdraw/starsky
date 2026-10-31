using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.realtime.Interface;
using starsky.feature.realtime.Services;
using starsky.feature.syncbackground.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starsky.foundation.settings.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.worker.Models;
using starskytest.FakeMocks;

namespace starskytest.Tenancy;

[TestClass]
public sealed class TenantJobsTest
{
	public TestContext TestContext { get; set; } = null!;

	[TestCleanup]
	public void Cleanup()
	{
		TenantAmbient.Reset();
	}

	private sealed class RecordingQueue : IDiskWatcherBackgroundTaskQueue
	{
		public readonly List<BackgroundTaskQueueJob> Jobs = [];
		public int Count() => Jobs.Count;

		public ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
		{
			Jobs.Add(job);
			return ValueTask.CompletedTask;
		}

		public ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(CancellationToken token)
		{
			throw new NotSupportedException();
		}
	}

	private sealed class RecordingRealtime : IRealtimeConnectionsService
	{
		public static readonly List<string> CleanedFor = [];

		public Task NotificationToAllAsync<T>(ApiNotificationResponseModel<T> message,
			CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task CleanOldMessagesAsync()
		{
			// the tenant of the scope this service was resolved in
			lock ( CleanedFor )
			{
				CleanedFor.Add(TenantAmbient.Current);
			}

			return Task.CompletedTask;
		}
	}

	private static ServiceProvider Services(RecordingQueue? queue = null)
	{
		var services = new ServiceCollection();
		services.AddScoped<ITenantContext, TenantContext>();
		services.AddSingleton(new AppSettings { SyncOnStartup = true });
		services.AddSingleton<ISynchronize, FakeISynchronize>();
		services.AddSingleton<IWebLogger, FakeIWebLogger>();
		services.AddSingleton<ISettingsService, FakeISettingsService>();
		services.AddSingleton<IDiskWatcherBackgroundTaskQueue>(queue ?? new RecordingQueue());
		services.AddScoped<IRealtimeConnectionsService, RecordingRealtime>();
		return services.BuildServiceProvider();
	}

	[TestMethod]
	public async Task ForEachActiveTenant_EachTenantGetsItsOwnScopeAndContext()
	{
		await using var provider = Services();
		var seen = new List<(string Tenant, string Scoped, string Ambient)>();

		await TenantJobs.ForEachActiveTenantAsync(
			provider.GetRequiredService<IServiceScopeFactory>(),
			new FakeTenantRegistry("default", "acme", "beta"),
			(sp, tenantId) =>
			{
				seen.Add(( tenantId, sp.GetRequiredService<ITenantContext>().TenantId,
					TenantAmbient.Current ));
				return Task.CompletedTask;
			});

		CollectionAssert.AreEqual(
			new[] { ( "default", "default", "default" ), ( "acme", "acme", "acme" ),
				( "beta", "beta", "beta" ) }, seen);
		Assert.AreEqual("default", TenantAmbient.Current, "the caller must not inherit a tenant");
	}

	[TestMethod]
	public async Task ForEachActiveTenant_FailingTenant_DoesNotStopTheOthers()
	{
		await using var provider = Services();
		var done = new List<string>();
		var errors = new List<string>();

		await TenantJobs.ForEachActiveTenantAsync(
			provider.GetRequiredService<IServiceScopeFactory>(),
			new FakeTenantRegistry("default", "acme", "beta"),
			(_, tenantId) =>
			{
				if ( tenantId == "acme" )
				{
					throw new InvalidOperationException("boom");
				}

				done.Add(tenantId);
				return Task.CompletedTask;
			}, (tenantId, _) => errors.Add(tenantId));

		CollectionAssert.AreEqual(new[] { "default", "beta" }, done);
		CollectionAssert.AreEqual(new[] { "acme" }, errors);
	}

	[TestMethod]
	public async Task CleanUpConnections_RunsForEveryTenant()
	{
		await using var provider = Services();
		RecordingRealtime.CleanedFor.Clear();

		await new TestableCleanUp(provider.GetRequiredService<IServiceScopeFactory>(),
			new FakeTenantRegistry("default", "acme")).Run();

		CollectionAssert.AreEqual(new[] { "default", "acme" }, RecordingRealtime.CleanedFor);
	}

	private sealed class TestableCleanUp(IServiceScopeFactory factory, ITenantRegistry registry)
		: CleanUpConnectionBackgroundService(factory, registry)
	{
		public Task Run()
		{
			return ExecuteAsync(CancellationToken.None);
		}
	}

	[TestMethod]
	public async Task OnStartupSync_QueuesOneJobPerTenant_WithThatTenant()
	{
		var queue = new RecordingQueue();
		await using var provider = Services(queue);

		await new OnStartupSyncBackgroundService(
			provider.GetRequiredService<IServiceScopeFactory>(),
			new FakeTenantRegistry("default", "acme", "beta")).StartAsync();

		CollectionAssert.AreEqual(new[] { "default", "acme", "beta" },
			queue.Jobs.Select(j => j.TenantId).ToArray());
	}
}
