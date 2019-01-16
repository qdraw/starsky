using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;

namespace starskytests.Services
{
    [TestClass]
    public class BackgroundTaskQueueTest
    {
        private IBackgroundTaskQueue _bgTaskQueue;

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
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
            
            // Add Background services
            services.AddSingleton<IHostedService, BackgroundQueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            
            // build the service
            var serviceProvider = services.BuildServiceProvider();
            _bgTaskQueue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();;
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

        }
	    
	    // https://stackoverflow.com/a/51224556
	    [TestMethod]
	    public async Task BackgroundTaskQueueTest_Verify_Hosted_Service_Executes_Task() {
		    IServiceCollection services = new ServiceCollection();
		    services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
		    services.AddHostedService<BackgroundQueuedHostedService>();
		    services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
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
		    _bgTaskQueue.QueueBackgroundWorkItem(null);
	    }

	    [TestMethod]
		public async Task BackgroundQueuedHostedServiceTestHandleException()
	    {
			IServiceCollection services = new ServiceCollection();
		    services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
		    services.AddHostedService<BackgroundQueuedHostedService>();
		    services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
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



	}
}
