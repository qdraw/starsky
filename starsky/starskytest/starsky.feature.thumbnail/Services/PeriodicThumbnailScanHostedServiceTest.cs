using System;
using System.Linq;
using System.Reflection;
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
		
		await periodicThumbnailScanHostedService.StartBackgroundAsync(false,
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
	[ExpectedException(typeof(OperationCanceledException))]
	public async Task StartBackgroundAsync_StartDirect()
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
		
		await periodicThumbnailScanHostedService.StartBackgroundAsync(true,
			cancelToken.Token);
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
	[ExpectedException(typeof(OperationCanceledException))]
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
		
		periodicThumbnailScanHostedService.IsEnabled = true;
		periodicThumbnailScanHostedService.MinimumIntervalInMinutes = 0;

		var cancelToken = new CancellationTokenSource();
		cancelToken.CancelAfter(200);
		
		await periodicThumbnailScanHostedService.StartBackgroundAsync(false,
			cancelToken.Token);
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

		
	[TestMethod]
	[Timeout(5000)]
	[ExpectedException(typeof(OperationCanceledException))]
	public async Task RunJob_Canceled()
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
		
		periodicThumbnailScanHostedService.IsEnabled = true;
		periodicThumbnailScanHostedService.MinimumIntervalInMinutes = 0;

		var cancelToken = new CancellationTokenSource();
		cancelToken.Cancel();
		
		await periodicThumbnailScanHostedService.RunJob(
			cancelToken.Token);
	}
	
	[TestMethod]
	[Timeout(300)]
	public void ExecuteAsync_StartAsync_Test()
	{
		var services = new ServiceCollection();
		// missing service in service scope
		
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var logger = new FakeIWebLogger();
		var service = new PeriodicThumbnailScanHostedService(new AppSettings
			{
				ThumbnailGenerationIntervalInMinutes = 0
			},
			logger,
			scopeFactory);
			
		CancellationTokenSource source = new CancellationTokenSource();
		CancellationToken token = source.Token;
		source.Cancel(); // <- cancel before start

		MethodInfo dynMethod = service.GetType().GetMethod("ExecuteAsync", 
			BindingFlags.NonPublic | BindingFlags.Instance);
		if ( dynMethod == null )
			throw new Exception("missing ExecuteAsync");
		dynMethod.Invoke(service, new object[]
		{
			token
		});
			
		Assert.IsTrue(!logger.TrackedInformation.Any());
		Assert.IsTrue(!logger.TrackedExceptions.Any());

	}
}
