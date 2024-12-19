using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Metrics;
using starsky.foundation.worker.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.worker.Services;

[TestClass]
public sealed class UpdateBackgroundTaskQueueTest
{
	private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
	private readonly IServiceScopeFactory _scopeFactory;

	public UpdateBackgroundTaskQueueTest()
	{
		// Start using dependency injection
		var builder = new ConfigurationBuilder();
		var dict = new Dictionary<string, string?> { { "App:Verbose", "true" } };
		// Add random config to dependency injection
		builder.AddInMemoryCollection(dict);
		// build config
		var configuration = builder.Build();
		var services = new ServiceCollection();

		// inject config as object to a service
		services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));

		// Add Background services
		services.AddSingleton<IHostedService, UpdateBackgroundQueuedHostedService>();
		services.AddSingleton<IUpdateBackgroundTaskQueue, UpdateBackgroundTaskQueue>();
		services.AddSingleton<IMeterFactory, FakeIMeterFactory>();
		services.AddSingleton<UpdateBackgroundQueuedMetrics>();

		// build the service
		var serviceProvider = services.BuildServiceProvider();
		_bgTaskQueue = serviceProvider.GetRequiredService<IUpdateBackgroundTaskQueue>();
		_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task BackgroundTaskQueueTest_DequeueAsync()
	{
		await _bgTaskQueue.QueueBackgroundWorkItemAsync(async token =>
		{
			for ( var delayLoop = 0; delayLoop < 3; delayLoop++ )
			{
				await Task.Delay(TimeSpan.FromSeconds(1), token);
				Console.WriteLine(delayLoop);
				// Cancel request > not tested very good
				await _bgTaskQueue.DequeueAsync(token);
			}
		}, string.Empty);
		Assert.IsNotNull(_bgTaskQueue);
	}

	[TestMethod]
	public async Task Count_AddOneForCount()
	{
		var backgroundQueue = new UpdateBackgroundTaskQueue(_scopeFactory);
		await backgroundQueue.QueueBackgroundWorkItemAsync(_ =>
			ValueTask.CompletedTask, string.Empty);
		var count = backgroundQueue.Count();
		Assert.AreEqual(1, count);
	}

	// https://stackoverflow.com/a/51224556
	[TestMethod]
	[Timeout(5000)]
	[SuppressMessage("Usage", "S2589:Dup isExecuted")]
	public async Task BackgroundTaskQueueTest_Verify_Hosted_Service_Executes_Task()
	{
		IServiceCollection services = new ServiceCollection();
		services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
		services.AddHostedService<UpdateBackgroundQueuedHostedService>();
		services.AddSingleton<IUpdateBackgroundTaskQueue, UpdateBackgroundTaskQueue>();
		services.AddSingleton<IWebLogger, FakeIWebLogger>();
		services.AddSingleton<IMeterFactory, FakeIMeterFactory>();
		services.AddSingleton<UpdateBackgroundQueuedMetrics>();

		var serviceProvider = services.BuildServiceProvider();

		var service =
			serviceProvider.GetService<IHostedService>() as UpdateBackgroundQueuedHostedService;

		var backgroundQueue = serviceProvider.GetService<IUpdateBackgroundTaskQueue>();

		if ( service == null )
		{
			throw new NullReferenceException("bg is null");
		}

		await service.StartAsync(CancellationToken.None);

		var isExecuted = false;
		await backgroundQueue!.QueueBackgroundWorkItemAsync(async _ =>
			{
				await Task.Yield();
				isExecuted = true;
			},
			string.Empty);

		await Task.Delay(100);
		await backgroundQueue.QueueBackgroundWorkItemAsync(async _ =>
		{
			await Task.Yield();
			isExecuted = true;
			throw new Exception();
			// EXCEPTION IS IGNORED
		}, string.Empty);

		if ( !isExecuted )
		{
			await Task.Delay(500);
		}

		if ( !isExecuted )
		{
			await Task.Delay(500);
		}

		Assert.IsTrue(isExecuted);

		await service.StopAsync(CancellationToken.None);
	}

	[TestMethod]
	public async Task BackgroundTaskQueueTest_ArgumentNullExceptionFail()
	{
		// Arrange
		Func<CancellationToken, ValueTask>? func = null;

		// Act & Assert
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
		{
			await _bgTaskQueue.QueueBackgroundWorkItemAsync(func!, string.Empty);
		});

		// Additional assert to verify state if needed
		Assert.IsNull(func);
	}

	[TestMethod]
	[Timeout(5000)]
	[SuppressMessage("Usage", "S2589:Dup isExecuted")]
	public async Task BackgroundQueuedHostedServiceTestHandleException()
	{
		IServiceCollection services = new ServiceCollection();
		services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
		services.AddHostedService<UpdateBackgroundQueuedHostedService>();
		services.AddSingleton<IUpdateBackgroundTaskQueue, UpdateBackgroundTaskQueue>();
		services.AddSingleton<IWebLogger, FakeIWebLogger>();
		services.AddSingleton<IMeterFactory, FakeIMeterFactory>();
		services.AddSingleton<UpdateBackgroundQueuedMetrics>();

		var serviceProvider = services.BuildServiceProvider();

		var service =
			serviceProvider.GetService<IHostedService>() as UpdateBackgroundQueuedHostedService;

		var backgroundQueue = serviceProvider.GetService<IUpdateBackgroundTaskQueue>();

		await service!.StartAsync(CancellationToken.None);

		var isExecuted = false;
		await backgroundQueue!.QueueBackgroundWorkItemAsync(async _ =>
		{
			await Task.Yield();
			isExecuted = true;
			throw new Exception();
			// EXCEPTION IS IGNORED
		}, string.Empty);

		await Task.Delay(100);
		await backgroundQueue.QueueBackgroundWorkItemAsync(async _ =>
		{
			await Task.Yield();
			isExecuted = true;
			throw new Exception();
			// EXCEPTION IS IGNORED
		}, string.Empty);

		if ( !isExecuted )
		{
			await Task.Delay(1000);
		}

		Assert.IsTrue(isExecuted);
	}

	[TestMethod]
	[Timeout(5000)]
	public async Task StartAsync_CancelBeforeStart()
	{
		var fakeLogger = new FakeIWebLogger();
		var service =
			new UpdateBackgroundQueuedHostedService(new FakeIUpdateBackgroundTaskQueue(),
				fakeLogger);

		using var cancelTokenSource = new CancellationTokenSource();
		await cancelTokenSource.CancelAsync();

		// use reflection to hit protected method
		var method = service.GetType().GetTypeInfo().GetDeclaredMethod("ExecuteAsync");
		Assert.IsNotNull(method);
		method.Invoke(service, [cancelTokenSource.Token]);
		// should stop and not hit timeout
	}

	[TestMethod]
	[Timeout(5000)]
	public async Task UpdateBackgroundTaskQueue_Update_End_StopAsync_Test()
	{
		var logger = new FakeIWebLogger();
		var service =
			new UpdateBackgroundQueuedHostedService(new FakeIUpdateBackgroundTaskQueue(),
				logger);

		using var source = new CancellationTokenSource();
		var token = source.Token;
		await source.CancelAsync(); // <- cancel before start

		await service.StopAsync(token);

		Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2?.Contains("is stopping"));
	}
}
