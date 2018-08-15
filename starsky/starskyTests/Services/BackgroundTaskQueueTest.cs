using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    }
}