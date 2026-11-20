using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Tenancy;
using starsky.foundation.worker.Backends;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.worker.Tenancy;

[TestClass]
public sealed class QueueTenantTest
{
	public TestContext TestContext { get; set; } = null!;

	private static readonly string[] FourJobs = ["acme", "beta", "acme", "beta"];

	[TestCleanup]
	public void Cleanup()
	{
		TenantAmbient.Reset();
	}

	/// <summary>
	///     Records the tenant the handler runs as
	/// </summary>
	private sealed class RecordingHandler(ITenantContext tenantContext) : IBackgroundJobHandler
	{
		public static readonly List<(string Scoped, string Ambient)> Runs = [];
		public string JobType => "record";

		public Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken)
		{
			lock ( Runs )
			{
				Runs.Add(( tenantContext.TenantId, TenantAmbient.Current ));
			}

			return Task.CompletedTask;
		}
	}

	private async Task<(ServiceProvider Provider, SqliteConnection Connection)> CreateServices()
	{
		var connection = new SqliteConnection("Filename=:memory:");
		await connection.OpenAsync(TestContext.CancellationToken);
		var services = new ServiceCollection();
		services.AddScoped<ITenantContext, TenantContext>();
		services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(connection));
		services.AddScoped<IBackgroundJobHandler, RecordingHandler>();
		var provider = services.BuildServiceProvider();
		using var scope = provider.CreateScope();
		await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database
			.EnsureCreatedAsync(TestContext.CancellationToken);
		return ( provider, connection );
	}

	[TestMethod]
	public void Job_CapturesTheAmbientTenant_AtCreation()
	{
		Assert.AreEqual("default", new BackgroundTaskQueueJob().TenantId);

		TenantAmbient.Set("acme");
		var job = new BackgroundTaskQueueJob();
		TenantAmbient.Set("beta");

		Assert.AreEqual("acme", job.TenantId);
		Assert.AreEqual("beta", new BackgroundTaskQueueJob().TenantId);
	}

	[TestMethod]
	public async Task DatabaseQueue_CarriesTheTenantOfEachJob_AndCountsAll()
	{
		var (provider, connection) = await CreateServices();
		await using var _ = provider;
		await using var __ = connection;
		var backend = new DatabaseQueueBackend(
			provider.GetRequiredService<IServiceScopeFactory>(), new AppSettings(),
			new FakeIWebLogger(), "test");

		TenantAmbient.Set("acme");
		await backend.QueueJobAsync(new BackgroundTaskQueueJob { JobType = "record", MetaData = "a" });
		TenantAmbient.Set("beta");
		await backend.QueueJobAsync(new BackgroundTaskQueueJob { JobType = "record", MetaData = "b" });
		TenantAmbient.Reset();
		await backend.QueueJobAsync(new BackgroundTaskQueueJob { JobType = "record", MetaData = "c" });

		Assert.AreEqual(3, backend.Count());
		var first = await backend.DequeueJobAsync(TestContext.CancellationToken);
		var second = await backend.DequeueJobAsync(TestContext.CancellationToken);
		var third = await backend.DequeueJobAsync(TestContext.CancellationToken);

		Assert.AreEqual(("a", "acme"), (first.MetaData, first.TenantId));
		Assert.AreEqual(("b", "beta"), (second.MetaData, second.TenantId));
		Assert.AreEqual(("c", "default"), (third.MetaData, third.TenantId));
		Assert.AreEqual(0, backend.Count());
	}

	[TestMethod]
	public async Task DatabaseQueue_ARequestOfOneTenant_CanNotSeeTheJobsOfAnother()
	{
		var (provider, connection) = await CreateServices();
		await using var _ = provider;
		await using var __ = connection;
		var backend = new DatabaseQueueBackend(
			provider.GetRequiredService<IServiceScopeFactory>(), new AppSettings(),
			new FakeIWebLogger(), "test");
		TenantAmbient.Set("acme");
		await backend.QueueJobAsync(new BackgroundTaskQueueJob { JobType = "record" });

		// a request of the default tenant (the middleware sets the tenant explicitly)
		using var scope = provider.GetRequiredService<IServiceScopeFactory>()
			.CreateTenantScope("default");
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		// the normal (filtered) context of the default tenant does not show the job of acme
		Assert.AreEqual(0, await context.QueueItems.CountAsync(TestContext.CancellationToken));
	}

	[TestMethod]
	public async Task Handler_RunsAsTheTenantOfTheJob_AndAmbientIsResetAfterwards()
	{
		var (provider, connection) = await CreateServices();
		await using var _ = provider;
		await using var __ = connection;
		RecordingHandler.Runs.Clear();
		TenantAmbient.Set("beta");
		var job = new BackgroundTaskQueueJob { JobType = "record" };
		TenantAmbient.Reset();

		var executed = await ProcessTaskQueue.TryExecuteViaRegisteredHandlersAsync(
			provider.GetRequiredService<IServiceScopeFactory>(), job,
			TestContext.CancellationToken);

		Assert.IsTrue(executed);
		Assert.HasCount(1, RecordingHandler.Runs);
		Assert.AreEqual(("beta", "beta"), RecordingHandler.Runs[0]);
		Assert.AreEqual("default", TenantAmbient.Current);
	}

	[TestMethod]
	public async Task Handler_TwoJobsOfDifferentTenants_DoNotShareTheTenant()
	{
		var (provider, connection) = await CreateServices();
		await using var _ = provider;
		await using var __ = connection;
		RecordingHandler.Runs.Clear();
		var factory = provider.GetRequiredService<IServiceScopeFactory>();

		await Task.WhenAll(FourJobs.Select(async tenant =>
		{
			TenantAmbient.Set(tenant);
			var job = new BackgroundTaskQueueJob { JobType = "record" };
			await Task.Yield();
			await ProcessTaskQueue.TryExecuteViaRegisteredHandlersAsync(factory, job,
				TestContext.CancellationToken);
		}));

		Assert.HasCount(4, RecordingHandler.Runs);
		Assert.IsTrue(RecordingHandler.Runs.TrueForAll(r => r.Scoped == r.Ambient));
		Assert.AreEqual(2, RecordingHandler.Runs.Count(r => r.Scoped == "acme"));
		Assert.AreEqual(2, RecordingHandler.Runs.Count(r => r.Scoped == "beta"));
	}

	[TestMethod]
	public async Task Handler_DoesNotRun_ForATenantThatIsNotActive()
	{
		var connection = new SqliteConnection("Filename=:memory:");
		await connection.OpenAsync(TestContext.CancellationToken);
		var services = new ServiceCollection();
		services.AddScoped<ITenantContext, TenantContext>();
		services.AddSingleton<ITenantRegistry>(new FakeTenantRegistry("default", "acme"));
		services.AddScoped<IBackgroundJobHandler, RecordingHandler>();
		await using var provider = services.BuildServiceProvider();
		await using var _ = connection;
		RecordingHandler.Runs.Clear();
		var factory = provider.GetRequiredService<IServiceScopeFactory>();

		TenantAmbient.Set("disabled");
		var disabledJob = new BackgroundTaskQueueJob { JobType = "record" };
		TenantAmbient.Set("acme");
		var activeJob = new BackgroundTaskQueueJob { JobType = "record" };
		TenantAmbient.Reset();

		var skipped = await ProcessTaskQueue.TryExecuteViaRegisteredHandlersAsync(factory,
			disabledJob, TestContext.CancellationToken);
		await ProcessTaskQueue.TryExecuteViaRegisteredHandlersAsync(factory, activeJob,
			TestContext.CancellationToken);

		Assert.IsTrue(skipped, "handled (dropped), not left for another path");
		Assert.HasCount(1, RecordingHandler.Runs);
		Assert.AreEqual("acme", RecordingHandler.Runs[0].Scoped);
	}
}
