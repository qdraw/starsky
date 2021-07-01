using System;
using System.Collections.Generic;
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
using starsky.foundation.webtelemetry.Interfaces;
using starsky.foundation.worker.Services;
using starskytest.FakeMocks;

#pragma warning disable 1998

namespace starskytest.starsky.foundation.worker
{
	[TestClass]
	public class BackgroundTaskQueueTest
	{
		private readonly IBackgroundTaskQueue _bgTaskQueue;

		public BackgroundTaskQueueTest()
		{
			// Start using dependency injection
			var builder = new ConfigurationBuilder();  
			var dict = new Dictionary<string, string>
			{
				{ "App:Verbose", "true" }
			};
			// Add random config to dependency injection
			builder.AddInMemoryCollection(dict);
			// build config
			var configuration = builder.Build();
			var services = new ServiceCollection();

			// inject config as object to a service
			services.ConfigurePoCo<AppSettings>(configuration.GetSection("App"));
            
			// Add Background services
			services.AddSingleton<IHostedService, BackgroundQueuedHostedService>();
			services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
			services.AddSingleton<ITelemetryService, FakeTelemetryService>();

			// build the service
			var serviceProvider = services.BuildServiceProvider();
			_bgTaskQueue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();
		}
        
		[TestMethod]
		public void BackgroundTaskQueueTest_DequeueAsync()
		{
			_bgTaskQueue.QueueBackgroundWorkItem(async token =>
			{
				for (int delayLoop = 0; delayLoop < 3; delayLoop++)
				{
					await Task.Delay(TimeSpan.FromSeconds(1), token);
					Console.WriteLine(delayLoop);
					// Cancel request > not tested very good
					await _bgTaskQueue.DequeueAsync(token);
				}
			});
			Assert.IsNotNull(_bgTaskQueue);
		}

		// https://stackoverflow.com/a/51224556
		[TestMethod]
		[Timeout(5000)]
		public async Task BackgroundTaskQueueTest_Verify_Hosted_Service_Executes_Task() {
			IServiceCollection services = new ServiceCollection();
			services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
			services.AddHostedService<BackgroundQueuedHostedService>();
			services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
			services.AddSingleton<ITelemetryService, FakeTelemetryService>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();
			var serviceProvider = services.BuildServiceProvider();

			var service = serviceProvider.GetService<IHostedService>() as BackgroundQueuedHostedService;

			var backgroundQueue = serviceProvider.GetService<IBackgroundTaskQueue>();

			await service.StartAsync(CancellationToken.None);

			var isExecuted = false;
			backgroundQueue.QueueBackgroundWorkItem(async token => {
				isExecuted = true;
			});

			await Task.Delay(1000);
			Assert.IsTrue(isExecuted);

			await service.StopAsync(CancellationToken.None);
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public async Task BackgroundTaskQueueTest_ArgumentNullExceptionFail()
		{
			Func<CancellationToken, Task> func = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			_bgTaskQueue.QueueBackgroundWorkItem(func);
			Assert.IsNull(func);
		}

		[TestMethod]
		[Timeout(5000)]
		public async Task BackgroundQueuedHostedServiceTestHandleException()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
			services.AddHostedService<BackgroundQueuedHostedService>();
			services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();
			services.AddSingleton<ITelemetryService, FakeTelemetryService>();
			var serviceProvider = services.BuildServiceProvider();

			var service = serviceProvider.GetService<IHostedService>() as BackgroundQueuedHostedService;

			var backgroundQueue = serviceProvider.GetService<IBackgroundTaskQueue>();

			await service.StartAsync(CancellationToken.None);

			var isExecuted = false;
			backgroundQueue.QueueBackgroundWorkItem(async token =>
			{
				isExecuted = true;
				throw new Exception();
				// EXCEPTION IS IGNORED
			});

			await Task.Delay(1000);
			Assert.IsTrue(isExecuted);

		}

		[TestMethod]
		[Timeout(5000)]
		public async Task StartAsync_CancelBeforeStart()
		{
			var fakeLogger = new FakeIWebLogger();
			var service = new BackgroundQueuedHostedService(new FakeIBackgroundTaskQueue(), fakeLogger, new FakeTelemetryService());

			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.Cancel();
			
			// use reflection to hit protected method
			var method = service.GetType().GetTypeInfo().GetDeclaredMethod("ExecuteAsync");
			Assert.IsNotNull(method);
			method.Invoke(service, new object[]{cancelTokenSource.Token});
			// should stop and not hit timeout
		}

	}
}
