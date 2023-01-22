using System;
using System.Linq;
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
	[Timeout(5000)]
	public async Task StartBackgroundAsync_Cancel()
	{
		var services = new ServiceCollection();
		services
			.AddScoped<IDatabaseThumbnailGenerationService,
				FakeIDatabaseThumbnailGenerationService>();
		
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(new AppSettings
			{
				ThumbnailGenerationIntervalInMinutes = -1
			},
			new FakeIWebLogger(),
			scopeFactory);
		var cancelToken = new CancellationTokenSource();
		cancelToken.Cancel();
		
		await periodicThumbnailScanHostedService.StartBackgroundAsync(
			cancelToken.Token);

		var fakeService = scopeFactory.CreateScope().ServiceProvider
			.GetService<IDatabaseThumbnailGenerationService>() as FakeIDatabaseThumbnailGenerationService;
		
		Assert.AreEqual(0,fakeService?.Count);
	}
	
	[TestMethod]
	[Timeout(5000)]
	public void StartBackgroundAsync_SetEnabled_DefaultShouldBeTrue()
	{
		var services = new ServiceCollection();
		services
			.AddScoped<IDatabaseThumbnailGenerationService,
				FakeIDatabaseThumbnailGenerationService>();
		
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(new AppSettings(),
			new FakeIWebLogger(),
			scopeFactory);
		var cancelToken = new CancellationTokenSource();
		cancelToken.Cancel();
		
		Assert.AreEqual(true, periodicThumbnailScanHostedService.IsEnabled);
	}
	
	[TestMethod]
	[Timeout(5000)]
	public void StartBackgroundAsync_SetEnabled_ValueBelow2Minutes()
	{
		var services = new ServiceCollection();
		services
			.AddScoped<IDatabaseThumbnailGenerationService,
				FakeIDatabaseThumbnailGenerationService>();
		
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(new AppSettings
			{
				ThumbnailGenerationIntervalInMinutes = 1	
			},
			new FakeIWebLogger(),
			scopeFactory);
		var cancelToken = new CancellationTokenSource();
		cancelToken.Cancel();
		
		Assert.AreEqual(false, periodicThumbnailScanHostedService.IsEnabled);
		Assert.AreEqual( TimeSpan.FromMinutes(15), periodicThumbnailScanHostedService.Period);
	}
	
	[TestMethod]
	[Timeout(5000)]
	public async Task StartBackgroundAsync_ShouldRun_SlowTest()
	{
		var services = new ServiceCollection();
		services
			.AddSingleton<IDatabaseThumbnailGenerationService,
				FakeIDatabaseThumbnailGenerationService>();
		
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(new AppSettings
			{
				ThumbnailGenerationIntervalInMinutes = 0
			},
			new FakeIWebLogger(),
			scopeFactory);
		var cancelToken = new CancellationTokenSource();
		
		// maybe on slow machines this is not enough time
		cancelToken.CancelAfter(200);

		periodicThumbnailScanHostedService.IsEnabled = true;
		periodicThumbnailScanHostedService.MinimumIntervalInMinutes = 0;

		try
		{
			await periodicThumbnailScanHostedService.StartBackgroundAsync(
				cancelToken.Token);
		}
		catch ( OperationCanceledException e )
		{
			Console.WriteLine(e);
		}

		var fakeService = scopeFactory.CreateScope().ServiceProvider
			.GetService<IDatabaseThumbnailGenerationService>() as FakeIDatabaseThumbnailGenerationService;
		
		Assert.AreEqual(1,fakeService?.Count);
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

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(new AppSettings(),
			new FakeIWebLogger(),
			scopeFactory);

		var result = await periodicThumbnailScanHostedService.RunJob();
		Assert.AreEqual(true,result);
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

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(new AppSettings(),
			new FakeIWebLogger(),
			scopeFactory);
		periodicThumbnailScanHostedService.IsEnabled = false;
		
		var result = await periodicThumbnailScanHostedService.RunJob();
		Assert.AreEqual(false,result);
	}
	
	[TestMethod]
	public async Task RunJob_FailCase()
	{
		var services = new ServiceCollection();
		// missing service in service scope
		
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var periodicThumbnailScanHostedService = new PeriodicThumbnailScanHostedService(new AppSettings(),
			new FakeIWebLogger(),
			scopeFactory);
		
		var result = await periodicThumbnailScanHostedService.RunJob();
		Assert.AreEqual(null,result);
	}

}
