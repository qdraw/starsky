// Not working :(


//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.Abstractions;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using starsky.Services;
//
//namespace starskytests.Services
//{
//    [TestClass]
//    public class BackgroundQueuedHostedServiceTest
//    {
//
//        public BackgroundQueuedHostedServiceTest()
//        {
//            TaskQueue = new BackgroundTaskQueue();
//        }
//        
//        private IBackgroundTaskQueue TaskQueue { get; }
//
//        [TestMethod]
//        public async Task Verify_Hosted_Service_Executes_Task() {
//            IServiceCollection services = new ServiceCollection();
//            services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
//            services.AddSingleton<IHostedService, BackgroundQueuedHostedService>();
//            // .Net Core 2.1 -> services.AddHostedService<QueuedHostedService>();
//            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
//            var serviceProvider = services.BuildServiceProvider();
//
//            var service = serviceProvider.GetService<BackgroundQueuedHostedService>();
//
//            var backgroundQueue = serviceProvider.GetService<IBackgroundTaskQueue>();
//
//            await service.StartAsync( new CancellationToken());
//            
//            var isExecuted = false;
//            backgroundQueue.QueueBackgroundWorkItem(async token =>
//            {
//                isExecuted = true;
//            });
//
//            await Task.Delay(10000);
//            Assert.IsTrue(isExecuted);
//
//            await service.StopAsync(CancellationToken.None);
//        }
//    }
//}