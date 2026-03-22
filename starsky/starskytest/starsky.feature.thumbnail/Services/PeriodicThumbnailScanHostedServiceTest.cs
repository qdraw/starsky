using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.thumbnail.Interfaces;
using starsky.feature.thumbnail.Services;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services;

[TestClass]
public class PeriodicThumbnailScanHostedServiceTest
{
	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task StartAsync_ApplicationStarted_InvokesRunStartupJobAsync()
	{
		var services = new ServiceCollection();
		// register as singleton so test can inspect the same instance
		services
			.AddSingleton<IDatabaseThumbnailGenerationService,
				FakeIDatabaseThumbnailGenerationService>();

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var fakeLifetime = new FakeTriggerableIHostApplicationLifetime();
		var logger = new FakeIWebLogger();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(
			new AppSettings(),
			logger,
			scopeFactory, fakeLifetime);

		// Start the hosted service which will register the ApplicationStarted callback
		await periodicThumbnailScanHostedService.StartAsync(CancellationToken.None);

		// Trigger the ApplicationStarted to invoke RunStartupJobAsync
		fakeLifetime.TriggerApplicationStarted();

		// Wait for the fake service to be invoked (with timeout)
		var sw = System.Diagnostics.Stopwatch.StartNew();
		var called = false;
		while (sw.ElapsedMilliseconds < 2000)
		{
			if ( serviceProvider.GetService<IDatabaseThumbnailGenerationService>() is FakeIDatabaseThumbnailGenerationService fakeService && fakeService.Count > 0 )
			{
				called = true;
				break;
			}
			await Task.Delay(50, TestContext.CancellationToken);
		}

		// Stop the hosted service to cleanup
		await periodicThumbnailScanHostedService.StopAsync(CancellationToken.None);

		Assert.IsTrue(called, "Expected RunStartupJobAsync to invoke StartBackgroundQueue on the database thumbnail generation service");
	}

	[Timeout(5000, CooperativeCancellation = true)]
	public async Task StartBackgroundAsync_Cancel()
	{
		var services = new ServiceCollection();
		services
			.AddScoped<IDatabaseThumbnailGenerationService,
				FakeIDatabaseThumbnailGenerationService>();

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(
			new AppSettings { ThumbnailGenerationIntervalInMinutes = -1 },
			new FakeIWebLogger(),
			scopeFactory, new FakeIApplicationLifetime());

		using var cancelToken = new CancellationTokenSource();
		await cancelToken.CancelAsync();

		await periodicThumbnailScanHostedService.StartBackgroundAsync(false,
			cancelToken.Token);

		var fakeService = scopeFactory.CreateScope().ServiceProvider
				.GetService<IDatabaseThumbnailGenerationService>() as
			FakeIDatabaseThumbnailGenerationService;

		Assert.AreEqual(0, fakeService?.Count);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public void StartBackgroundAsync_SetEnabled_DefaultShouldBeTrue()
	{
		var services = new ServiceCollection();
		services
			.AddScoped<IDatabaseThumbnailGenerationService,
				FakeIDatabaseThumbnailGenerationService>();

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(
			new AppSettings(),
			new FakeIWebLogger(),
			scopeFactory, new FakeIApplicationLifetime());
		using var cancelToken = new CancellationTokenSource();
		cancelToken.Cancel();

		Assert.IsTrue(periodicThumbnailScanHostedService.IsEnabled);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task StartBackgroundAsync_StartDirect()
	{
		var services = new ServiceCollection();
		services
			.AddScoped<IDatabaseThumbnailGenerationService,
				FakeIDatabaseThumbnailGenerationService>();

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(
			new AppSettings { ThumbnailGenerationIntervalInMinutes = 0 },
			new FakeIWebLogger(),
			scopeFactory, new FakeIApplicationLifetime()) { MinimumIntervalInMinutes = 0, IsEnabled = true };
		using var cancelToken = new CancellationTokenSource();
		await cancelToken.CancelAsync();

		await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
			await periodicThumbnailScanHostedService.StartBackgroundAsync(true,
				cancelToken.Token));
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public void StartBackgroundAsync_SetEnabled_ValueBelow2Minutes()
	{
		var services = new ServiceCollection();
		services
			.AddScoped<IDatabaseThumbnailGenerationService,
				FakeIDatabaseThumbnailGenerationService>();

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(
			new AppSettings { ThumbnailGenerationIntervalInMinutes = 1 },
			new FakeIWebLogger(),
			scopeFactory, new FakeIApplicationLifetime());
		using var cancelToken = new CancellationTokenSource();
		cancelToken.Cancel();

		Assert.IsFalse(periodicThumbnailScanHostedService.IsEnabled);
		Assert.AreEqual(TimeSpan.FromMinutes(60), periodicThumbnailScanHostedService.Period);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task StartBackgroundAsync_ShouldRun_SlowTest()
	{
		var services = new ServiceCollection();
		services
			.AddSingleton<IDatabaseThumbnailGenerationService,
				FakeIDatabaseThumbnailGenerationService>();

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(
			new AppSettings { ThumbnailGenerationIntervalInMinutes = 0 },
			new FakeIWebLogger(),
			scopeFactory, new FakeIApplicationLifetime());

		periodicThumbnailScanHostedService.IsEnabled = true;
		periodicThumbnailScanHostedService.MinimumIntervalInMinutes = 0;

		using var cancelToken = new CancellationTokenSource();
		cancelToken.CancelAfter(400);

		var result = await periodicThumbnailScanHostedService.StartBackgroundAsync(false,
			cancelToken.Token);

		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task RunJob_ShouldRun()
	{
		var services = new ServiceCollection();
		services
			.AddSingleton<IDatabaseThumbnailGenerationService,
				FakeIDatabaseThumbnailGenerationService>();

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(
			new AppSettings(),
			new FakeIWebLogger(),
			scopeFactory, new FakeIApplicationLifetime());

		var result = await periodicThumbnailScanHostedService.RunJob(TestContext.CancellationTokenSource.Token);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task RunJob_ShouldNotRun_NotEnabled()
	{
		var services = new ServiceCollection();
		services
			.AddSingleton<IDatabaseThumbnailGenerationService,
				FakeIDatabaseThumbnailGenerationService>();

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(
			new AppSettings(),
			new FakeIWebLogger(),
			scopeFactory, new FakeIApplicationLifetime());
		periodicThumbnailScanHostedService.IsEnabled = false;

		var result = await periodicThumbnailScanHostedService.RunJob(TestContext.CancellationTokenSource.Token);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task RunJob_FailCase()
	{
		var services = new ServiceCollection();
		// missing service in service scope

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(
			new AppSettings(),
			new FakeIWebLogger(),
			scopeFactory, new FakeIApplicationLifetime());

		var result = await periodicThumbnailScanHostedService.RunJob(TestContext.CancellationTokenSource.Token);
		Assert.IsNull(result);
	}


	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task RunJob_Canceled()
	{
		var services = new ServiceCollection();
		services
			.AddSingleton<IDatabaseThumbnailGenerationService,
				FakeIDatabaseThumbnailGenerationService>();

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(
			new AppSettings { ThumbnailGenerationIntervalInMinutes = 0 },
			new FakeIWebLogger(),
			scopeFactory, new FakeIApplicationLifetime());

		periodicThumbnailScanHostedService.IsEnabled = true;
		periodicThumbnailScanHostedService.MinimumIntervalInMinutes = 0;

		using var cancelToken = new CancellationTokenSource();
		await cancelToken.CancelAsync();

		await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
			await periodicThumbnailScanHostedService.RunJob(
				cancelToken.Token));
	}

	[TestMethod]
	[Timeout(2000, CooperativeCancellation = true)]
	public async Task PeriodicThumbnailScanHostedService_StartAsync_StopAsync_Test()
	{
		var services = new ServiceCollection();
		// missing service in service scope

		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var logger = new FakeIWebLogger();
		var service = new PeriodicThumbnailScanHostedService(
			new AppSettings { ThumbnailGenerationIntervalInMinutes = 0 },
			logger,
			scopeFactory, new FakeIApplicationLifetime());

		await service.StartAsync(TestContext.CancellationTokenSource.Token);
		await service.StopAsync(TestContext.CancellationTokenSource.Token);

		Assert.IsEmpty(logger.TrackedInformation);
		Assert.IsEmpty(logger.TrackedExceptions);
	}

	public TestContext TestContext { get; set; }
}
