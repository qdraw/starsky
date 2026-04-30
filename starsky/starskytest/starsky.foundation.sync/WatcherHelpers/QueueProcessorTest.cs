using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.sync.WatcherHelpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherHelpers;

[TestClass]
public sealed class QueueProcessorTest
{
	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task QueueProcessorTest_QueueInput()
	{
		var diskWatcherBackgroundTaskQueue = new FakeDiskWatcherUpdateBackgroundTaskQueue();

		var queueProcessor = new QueueProcessor(diskWatcherBackgroundTaskQueue);

		await queueProcessor.QueueJob("t", "T", WatcherChangeTypes.All);
		Assert.IsTrue(diskWatcherBackgroundTaskQueue.QueueBackgroundWorkItemCalled);
	}

	[TestMethod]
	public async Task QueueProcessorTest_QueueInput_Counter()
	{
		var diskWatcherBackgroundTaskQueue = new FakeDiskWatcherUpdateBackgroundTaskQueue();

		var queueProcessor = new QueueProcessor(diskWatcherBackgroundTaskQueue);

		// Run 3 times & 1 time different
		await queueProcessor.QueueJob("t", "T", WatcherChangeTypes.All);
		await queueProcessor.QueueJob("t", "T", WatcherChangeTypes.All);
		await queueProcessor.QueueJob("t", "T", WatcherChangeTypes.All);
		await queueProcessor.QueueJob("1", "T", WatcherChangeTypes.All);

		Assert.AreEqual(4, diskWatcherBackgroundTaskQueue.QueueBackgroundWorkItemCalledCounter);
	}


	[TestMethod]
	public async Task QueueProcessorTest_QueueInput_Counter_NoCache()
	{
		var diskWatcherBackgroundTaskQueue = new FakeDiskWatcherUpdateBackgroundTaskQueue();

		var queueProcessor = new QueueProcessor(diskWatcherBackgroundTaskQueue);

		// Run 3 times & 1 time different
#pragma warning disable CS4014
		queueProcessor.QueueJob("t", "T", WatcherChangeTypes.All);
		await Task.Delay(TimeSpan.FromMilliseconds(2),
			TestContext.CancellationTokenSource.Token); // Sleep async
		queueProcessor.QueueJob("t", "T", WatcherChangeTypes.All);
#pragma warning restore CS4014
		Assert.AreEqual(2, diskWatcherBackgroundTaskQueue.QueueBackgroundWorkItemCalledCounter);
	}

	[TestMethod]
	public async Task QueueProcessor_ResolvesTenantFromFolderAndAddsTenantMetadata()
	{
		var queue = new FakeDiskWatcherUpdateBackgroundTaskQueue();
		var services = new ServiceCollection();
		services.AddSingleton<IDiskWatcherBackgroundTaskQueue>(queue);
		services.AddSingleton(new AppSettings { StorageFolder = "/photos/" });
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(QueueProcessor_ResolvesTenantFromFolderAndAddsTenantMetadata)));
		var provider = services.BuildServiceProvider();

		using ( var scope = provider.CreateScope() )
		{
			var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			await context.Tenants.AddAsync(new Tenant
			{
				Slug = "main",
				Name = "main",
				IsEnabled = true,
				Created = DateTime.UtcNow
			}, TestContext.CancellationTokenSource.Token);
			await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
		}

		var queueProcessor = new QueueProcessor(provider.GetRequiredService<IServiceScopeFactory>());
		var filePath = Path.Combine("/photos", "main", "2020", "image.jpg");
		await queueProcessor.QueueJob(filePath, null, WatcherChangeTypes.Created);

		Assert.IsNotNull(queue.LastQueuedJob);
		Assert.AreEqual("main", queue.LastQueuedJob.TenantSlug);
		Assert.IsTrue(queue.LastQueuedJob.TenantId.HasValue);
	}

	[TestMethod]
	public void ExtractTenantSlugFromPath_ReturnsTenantSegment()
	{
		var filePath = Path.Combine("/photos", "main", "2020", "image.jpg");
		var slug = QueueProcessor.ExtractTenantSlugFromPath(filePath, "/photos/");
		Assert.AreEqual("main", slug);
	}
}
