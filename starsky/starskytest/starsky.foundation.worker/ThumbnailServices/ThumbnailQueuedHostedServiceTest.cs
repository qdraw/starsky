using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using starsky.foundation.webtelemetry.Interfaces;
using starsky.foundation.worker.CpuEventListener.Interfaces;
using starsky.foundation.worker.ThumbnailServices;
using starsky.foundation.worker.ThumbnailServices.Exceptions;
using starsky.foundation.worker.ThumbnailServices.Interfaces;
using starskytest.FakeMocks;

#pragma warning disable 1998

namespace starskytest.starsky.foundation.worker.ThumbnailServices
{
	/// <summary>
	/// ThumbnailBackgroundTaskQueueTest
	/// </summary>
	[TestClass]
	public sealed class ThumbnailQueuedHostedServiceTest
	{
		private readonly IThumbnailQueuedHostedService _bgTaskQueue;
		private readonly IServiceScopeFactory _scopeFactory;

		public ThumbnailQueuedHostedServiceTest()
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
			services.AddSingleton<IHostedService, ThumbnailQueuedHostedService>();
			services.AddSingleton<IThumbnailQueuedHostedService, ThumbnailBackgroundTaskQueue>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();
			services.AddSingleton<ICpuUsageListener, FakeICpuUsageListener>();
			services.AddSingleton<ITelemetryService, FakeTelemetryService>();

			// build the service
			var serviceProvider = services.BuildServiceProvider();
			_bgTaskQueue = serviceProvider.GetRequiredService<IThumbnailQueuedHostedService>();
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}

		[TestMethod]
		public async Task ThumbnailQueuedHostedServiceTest_DequeueAsync()
		{
			await _bgTaskQueue.QueueBackgroundWorkItemAsync(async token =>
			{
				for ( int delayLoop = 0; delayLoop < 3; delayLoop++ )
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
			var backgroundQueue = new ThumbnailBackgroundTaskQueue(new FakeICpuUsageListener(),
				new FakeIWebLogger(), new AppSettings());
			await backgroundQueue.QueueBackgroundWorkItemAsync(_ => ValueTask.CompletedTask,
				string.Empty);
			var count = backgroundQueue.Count();
			Assert.AreEqual(1, count);
		}

		[TestMethod]
		[ExpectedException(typeof(ToManyUsageException))]
		public async Task Count_AddOneForCount_UsageException()
		{
			var e = new FakeICpuUsageListener(100d);
			Console.WriteLine(e.CpuUsageMean);
			var backgroundQueue =
				new ThumbnailBackgroundTaskQueue(e, new FakeIWebLogger(), new AppSettings());
			await backgroundQueue.QueueBackgroundWorkItemAsync(_ => ValueTask.CompletedTask,
				string.Empty);
			var count = backgroundQueue.Count();
			Assert.AreEqual(0, count);
		}


		/// <summary>
		/// @see: https://stackoverflow.com/a/51224556
		/// </summary>
		/// <exception cref="NotSupportedException">not found</exception>
		/// <exception cref="NullReferenceException">null ref</exception>
		[TestMethod]
		[Timeout(5000)]
		[SuppressMessage("Usage", "S2589:Dup isExecuted")]
		public async Task ThumbnailQueuedHostedServiceTest_Verify_Hosted_Service_Executes_Task()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddSingleton<IHostedService, ThumbnailQueuedHostedService>();
			services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
			services.AddSingleton<IThumbnailQueuedHostedService, ThumbnailBackgroundTaskQueue>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();
			services.AddSingleton<ICpuUsageListener, FakeICpuUsageListener>();
			services.AddSingleton<ITelemetryService, FakeTelemetryService>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();
			services.AddSingleton<AppSettings, AppSettings>();

			var serviceProvider = services.BuildServiceProvider();

			var hostedServices = serviceProvider.GetServices<IHostedService>().ToList();
			if ( hostedServices.Count != 1 )
			{
				throw new NotSupportedException("hostedServices.Count() != 1");
			}

			var service = hostedServices[0] as ThumbnailQueuedHostedService;


			var backgroundQueue = serviceProvider.GetService<IThumbnailQueuedHostedService>();

			if ( service == null )
				throw new NullReferenceException("bg is null");

			await service.StartAsync(CancellationToken.None);

			var isExecuted = false;
			await backgroundQueue!.QueueBackgroundWorkItemAsync(async _ => { isExecuted = true; },
				string.Empty);

			await Task.Delay(100);
			if ( !isExecuted )
			{
				await Task.Delay(400);
			}

			if ( !isExecuted )
			{
				await Task.Delay(500);
			}

			Assert.IsTrue(isExecuted);

			await service.StopAsync(CancellationToken.None);
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public async Task ThumbnailQueuedHostedServiceTest_ArgumentNullExceptionFail()
		{
			Func<CancellationToken, ValueTask>? func = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			await _bgTaskQueue.QueueBackgroundWorkItemAsync(func!, string.Empty);
			Assert.IsNull(func);
		}

		[TestMethod]
		[Timeout(5000)]
		[SuppressMessage("Usage", "S2589:Dup isExecuted")]
		public async Task BackgroundQueuedHostedServiceTestHandleException()
		{
			IServiceCollection services = new ServiceCollection();
			services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
			services.AddSingleton<IHostedService, ThumbnailQueuedHostedService>();
			services.AddSingleton<IThumbnailQueuedHostedService, ThumbnailBackgroundTaskQueue>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();
			services.AddSingleton<AppSettings, AppSettings>();
			services.AddSingleton<ICpuUsageListener, FakeICpuUsageListener>();
			var serviceProvider = services.BuildServiceProvider();

			var hostedServices = serviceProvider.GetServices<IHostedService>().ToList();
			if ( hostedServices.Count != 1 )
			{
				throw new NotSupportedException("hostedServices.Count() != 1");
			}

			var service = hostedServices[0] as ThumbnailQueuedHostedService;

			var backgroundQueue = serviceProvider.GetService<IThumbnailQueuedHostedService>();

			await service!.StartAsync(CancellationToken.None);

			var isExecuted = false;
			await backgroundQueue!.QueueBackgroundWorkItemAsync(async _ =>
			{
				isExecuted = true;
				throw new Exception();
				// EXCEPTION IS IGNORED
			}, string.Empty);

			await Task.Delay(100);
			if ( !isExecuted )
			{
				await Task.Delay(400);
			}

			if ( !isExecuted )
			{
				await Task.Delay(500);
			}

			Assert.IsTrue(isExecuted);
		}

		[TestMethod]
		[Timeout(5000)]
		public async Task StartAsync_CancelBeforeStart()
		{
			var fakeLogger = new FakeIWebLogger();
			var service = new ThumbnailQueuedHostedService(new FakeThumbnailBackgroundTaskQueue(),
				fakeLogger, new AppSettings());

			using var cancelTokenSource = new CancellationTokenSource();
			await cancelTokenSource.CancelAsync();

			// use reflection to hit protected method
			var method = service.GetType().GetTypeInfo().GetDeclaredMethod("ExecuteAsync");
			Assert.IsNotNull(method);
			method.Invoke(service, new object[] { cancelTokenSource.Token });
			// should stop and not hit timeout
		}

		[TestMethod]
		[Timeout(1000)]
		public async Task ThumbnailQueuedHostedService_Update_End_StopAsync_Test()
		{
			var logger = new FakeIWebLogger();
			var service = new ThumbnailQueuedHostedService(new FakeThumbnailBackgroundTaskQueue(),
				logger, new AppSettings());

			using var source = new CancellationTokenSource();
			var token = source.Token;
			await source.CancelAsync(); // <- cancel before start

			await service.StopAsync(token);

			Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2?.Contains("is stopping"));
		}
	}
}
