using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Backends;
using starsky.foundation.worker.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.worker.Backends;

[TestClass]
public sealed class DatabaseQueueBackendTest
{
	private static (DatabaseQueueBackend backend, ServiceProvider provider, FakeIWebLogger logger)
		CreateBackend(string queueName, bool throwConcurrencyOnce = false)
	{
		var services = new ServiceCollection();
		var databaseName = Guid.NewGuid().ToString();

		if ( throwConcurrencyOnce )
		{
			ThrowOnceClaimApplicationDbContext.ShouldThrowOnProcessingClaim = true;
			services
				.AddDbContext<ApplicationDbContext, ThrowOnceClaimApplicationDbContext>(options =>
					options.UseInMemoryDatabase(databaseName));
		}
		else
		{
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseInMemoryDatabase(databaseName));
		}

		var provider = services.BuildServiceProvider();
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var backend = new DatabaseQueueBackend(
			provider.GetRequiredService<IServiceScopeFactory>(), appSettings, logger, queueName);
		return ( backend, provider, logger );
	}

	[TestMethod]
	public async Task QueueAndDequeue_RoundtripAndCount_AreCorrect()
	{
		var (backend, _, _) = CreateBackend("Queue-A");

		await backend.QueueJobAsync(new BackgroundTaskQueueJob
		{
			JobType = "Job-1",
			PayloadJson = "{\"k\":1}",
			CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 1, DateTimeKind.Utc)
		});

		await backend.QueueJobAsync(new BackgroundTaskQueueJob
		{
			JobType = "Job-2",
			PayloadJson = "{\"k\":2}",
			CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 2, DateTimeKind.Utc)
		});

		Assert.AreEqual(2, backend.Count());

		var first = await backend.DequeueJobAsync(CancellationToken.None);
		var second = await backend.DequeueJobAsync(CancellationToken.None);

		Assert.AreEqual("Job-1", first.JobType);
		Assert.AreEqual("Job-2", second.JobType);
		Assert.AreEqual(0, backend.Count());
	}

	[TestMethod]
	public async Task Count_IsIsolatedPerQueueName()
	{
		var (backendA, provider, _) = CreateBackend("Queue-A");
		var backendB = new DatabaseQueueBackend(
			provider.GetRequiredService<IServiceScopeFactory>(),
			new AppSettings(), new FakeIWebLogger(), "Queue-B");

		await backendA.QueueJobAsync(new BackgroundTaskQueueJob { JobType = "A" });
		await backendB.QueueJobAsync(new BackgroundTaskQueueJob { JobType = "B" });

		Assert.AreEqual(1, backendA.Count());
		Assert.AreEqual(1, backendB.Count());

		var itemA = await backendA.DequeueJobAsync(CancellationToken.None);
		Assert.AreEqual("A", itemA.JobType);
		Assert.AreEqual(0, backendA.Count());
		Assert.AreEqual(1, backendB.Count());
	}

	[TestMethod]
	public async Task QueueJobAsync_WithoutJobType_ThrowsArgumentException()
	{
		var (backend, _, _) = CreateBackend("Queue-A");
		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await backend.QueueJobAsync(new BackgroundTaskQueueJob()));
	}

	[TestMethod]
	public async Task QueueJobAsync_Null_ThrowsArgumentNullException()
	{
		var (backend, _, _) = CreateBackend("Queue-A");
		await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
			await backend.QueueJobAsync(null!));
	}

	[TestMethod]
	public async Task QueueJobAsync_DefaultCreatedAtUtc_IsPersistedAsUtcNow()
	{
		var (backend, provider, _) = CreateBackend("Queue-A");

		await backend.QueueJobAsync(new BackgroundTaskQueueJob
		{
			JobType = "A", CreatedAtUtc = default
		});

		using var scope = provider.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var queueItem = context.QueueItems.Single();
		Assert.AreNotEqual(default, queueItem.CreatedAtUtc);
	}

	[TestMethod]
	public async Task DequeueJobAsync_CancelledToken_ThrowsOperationCanceledException()
	{
		var (backend, _, _) = CreateBackend("Queue-A");
		using var cancellation = new CancellationTokenSource();
		await cancellation.CancelAsync();

		await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
			await backend.DequeueJobAsync(cancellation.Token));
	}

	[TestMethod]
	public async Task DequeueJobAsync_EmptyQueue_WaitsUntilCancelled()
	{
		var (backend, _, _) = CreateBackend("Queue-A");
		using var cancellation = new CancellationTokenSource(150);
		Exception? exception = null;

		try
		{
			await backend.DequeueJobAsync(cancellation.Token);
		}
		catch ( Exception ex )
		{
			exception = ex;
		}

		Assert.IsNotNull(exception);
		Assert.IsInstanceOfType<OperationCanceledException>(exception);
	}

	[TestMethod]
	public async Task DequeueJobAsync_OnConcurrencyRace_RetriesAndLogsInformation()
	{
		var (backend, _, logger) = CreateBackend("Queue-A", true);
		await backend.QueueJobAsync(new BackgroundTaskQueueJob { JobType = "A" });

		var dequeued = await backend.DequeueJobAsync(CancellationToken.None);

		Assert.AreEqual("A", dequeued.JobType);
		Assert.Contains(info =>
				( info.Item2 ?? string.Empty ).Contains("Queue claim race detected"),
			logger.TrackedInformation);
	}

	[TestMethod]
	public async Task DequeueJobAsync_EmptyQueueLoopsWithContinue_ThenCancelled()
	{
		// This test verifies that the continue statement is hit when queue is empty
		var (backend, _, _) = CreateBackend("Queue-A");

		using var cancellation = new CancellationTokenSource();
		// Set timeout to allow multiple iterations of the empty queue loop
		// which will hit the continue statement multiple times
		cancellation.CancelAfter(500);

		Exception? exception = null;
		try
		{
			await backend.DequeueJobAsync(cancellation.Token);
		}
		catch ( Exception ex )
		{
			exception = ex;
		}

		Assert.IsNotNull(exception);
		Assert.IsInstanceOfType<OperationCanceledException>(exception);
	}
}

internal sealed class ThrowOnceClaimApplicationDbContext(DbContextOptions options)
	: ApplicationDbContext(options)
{
	internal static bool ShouldThrowOnProcessingClaim { get; set; }

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		if ( !ShouldThrowOnProcessingClaim )
		{
			return base.SaveChangesAsync(cancellationToken);
		}

		var hasProcessingClaim = ChangeTracker.Entries<QueueItem>().Any(entry =>
			entry.State == EntityState.Modified &&
			entry.Entity.Status == QueueItemStatus.Processing);

		if ( !hasProcessingClaim )
		{
			return base.SaveChangesAsync(cancellationToken);
		}

		ShouldThrowOnProcessingClaim = false;
		throw new DbUpdateConcurrencyException("Simulated queue claim race");
	}
}
