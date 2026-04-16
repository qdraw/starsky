using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;
using starskytest.FakeMocks;
using starskytest.starsky.foundation.worker.Services;

namespace starskytest.starsky.foundation.worker.Helpers;

[TestClass]
public class ProcessTaskQueueTest
{
	[TestMethod]
	public void RoundUp_ShouldBeLowerThanValue()
	{
		var t = ProcessTaskQueue.RoundUp(new AppSettings
		{
			UseDiskWatcherIntervalInMilliseconds = 20000
		});
		Assert.IsLessThanOrEqualTo(20000, t.Item1.TotalMilliseconds);
	}

	[TestMethod]
	public async Task TryExecuteViaRegisteredHandlersAsync_NoHandlersRegistered_ReturnsFalse()
	{
		// Arrange: a scope factory without any IBackgroundJobHandler registrations
		var scopeFactory = new FakeIServiceScopeFactory();
		var queueJob = new BackgroundTaskQueueJob
		{
			JobType = "NoHandler", PayloadJson = "{}", MetaData = "meta"
		};

		// Act: 
		var result = await ProcessTaskQueue.TryExecuteViaRegisteredHandlersAsync(scopeFactory,
			queueJob,
			CancellationToken.None);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task
		TryExecuteViaRegisteredHandlersAsync_HandlerWithDifferentJobType_ReturnsFalse()
	{
		// Arrange: register a handler whose JobType does not match the queued job
		var scopeFactory = new FakeIServiceScopeFactory(null, services =>
		{
			// Register the same TestUpdateBackgroundJobHandler used elsewhere in tests
			services.AddScoped<TestUpdateBackgroundJobHandler>();
			services.AddScoped<IBackgroundJobHandler, TestUpdateBackgroundJobHandler>();
		});

		var queueJob = new BackgroundTaskQueueJob
		{
			JobType = "DifferentJobType", PayloadJson = "{}", MetaData = "meta"
		};

		// Act
		var result = await ProcessTaskQueue.TryExecuteViaRegisteredHandlersAsync(scopeFactory,
			queueJob,
			CancellationToken.None);


		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void RoundUp_WrongInput()
	{
		var t = ProcessTaskQueue.RoundUp(new AppSettings
		{
			UseDiskWatcherIntervalInMilliseconds = 0
		});
		Assert.AreEqual(0, t.Item1.TotalMilliseconds);
	}

	[TestMethod]
	[Timeout(10000, CooperativeCancellation = true)]
	public async Task ProcessBatchedLoopAsyncTest_NothingIn()
	{
		using var source = new CancellationTokenSource();
		var token = source.Token;

		var fakeService = new FakeDiskWatcherUpdateBackgroundTaskQueue();
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings { UseDiskWatcherIntervalInMilliseconds = 0 };

		source.CancelAfter(TimeSpan.FromSeconds(2));

		await ProcessTaskQueue.ProcessBatchedLoopAsync(fakeService, logger, appSettings, token);

		Assert.AreEqual(0, fakeService.DequeueAsyncCounter);
		Assert.AreEqual(0, fakeService.DequeueAsyncCounter);
	}

	[TestMethod]
	[Timeout(10000, CooperativeCancellation = true)]
	public async Task ProcessBatchedLoopAsyncTest_NothingIn_CanceledDuringWait()
	{
		using var source = new CancellationTokenSource();
		var token = source.Token;

		var fakeService = new FakeDiskWatcherUpdateBackgroundTaskQueue();
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings { UseDiskWatcherIntervalInMilliseconds = 11 };

		source.CancelAfter(TimeSpan.FromSeconds(2));

		// canceled in await Task.Delay
		await ProcessTaskQueue.ProcessBatchedLoopAsync(fakeService, logger, appSettings, token);

		Assert.AreEqual(0, fakeService.DequeueAsyncCounter);
		Assert.AreEqual(0, fakeService.DequeueAsyncCounter);
	}

	[TestMethod]
	[Timeout(10000, CooperativeCancellation = true)]
	public async Task ProcessBatchedLoopAsyncTest_ItemsIn()
	{
		using var source = new CancellationTokenSource();
		var token = source.Token;

		var fakeService = new FakeDiskWatcherUpdateBackgroundTaskQueue(null, 2);
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings { UseDiskWatcherIntervalInMilliseconds = 0 };

		source.CancelAfter(TimeSpan.FromSeconds(2));

		await ProcessTaskQueue.ProcessBatchedLoopAsync(fakeService, logger, appSettings, token);

		Assert.AreNotEqual(0, fakeService.DequeueAsyncCounter);
		Assert.AreEqual(2, fakeService.DequeueAsyncCounter);
	}

	[TestMethod]
	public async Task ExecuteTask_NullQueuedJob_ThrowsInvalidOperationException()
	{
		var mi = typeof(ProcessTaskQueue).GetMethod("ExecuteTask",
			BindingFlags.NonPublic | BindingFlags.Static);

		Assert.IsNotNull(mi, "Could not find private static ExecuteTask method");

		var logger = new FakeIWebLogger();

		// Parameters: (BackgroundTaskQueueJob? queueJob, IWebLogger logger, IBaseBackgroundTaskQueue? taskQueue,
		// CancellationToken cancellationToken, IServiceScopeFactory? scopeFactory)
		var parameters = new object?[] { null, logger, null, CancellationToken.None, null };

		var task = ( Task ) mi.Invoke(null, parameters)!;

		// Execute the task - the method catches exceptions internally and logs them
		await task.ConfigureAwait(false);

		// The implementation logs the exception via logger.LogError(ex, ...)
		Assert.IsNotEmpty(logger.TrackedExceptions,
			"Logger should have recorded at least one exception");
		var recorded = logger.TrackedExceptions[0];
		Assert.IsNotNull(recorded.Item1);
		Assert.Contains("Queued job is null", recorded.Item1.Message,
			"Exception message should contain 'Queued job is null'");
		Assert.Contains("Error occurred executing task work item", recorded.Item2 ?? string.Empty,
			"Log message should indicate error executing task work item");
	}
}
