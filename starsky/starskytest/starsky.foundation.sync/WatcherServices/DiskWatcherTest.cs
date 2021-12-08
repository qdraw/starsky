using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.sync.WatcherInterfaces;
using starsky.foundation.sync.WatcherServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherServices
{
	[TestClass]
	public class DiskWatcherTest
	{
		private readonly IServiceScopeFactory _scopeFactory;

		public DiskWatcherTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<ISynchronize, FakeISynchronize>();
			services.AddSingleton<AppSettings>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();
			services.AddScoped<IWebSocketConnectionsService, FakeIWebSocketConnectionsService>();
			services.AddScoped<IQuery, FakeIQuery>();
			services.AddScoped<IFileSystemWatcherWrapper, FakeIFileSystemWatcherWrapper>();
			services.AddScoped<IDiskWatcherBackgroundTaskQueue,FakeDiskWatcherUpdateBackgroundTaskQueue>();

			var serviceProvider = services.BuildServiceProvider();
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public void Watcher_ExpectPath()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();
			new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory).Watcher("/test");
			Assert.AreEqual("/test",fakeIFileSystemWatcher.Path);
		}
		
		[TestMethod]
		[Timeout(400)]
		public void Watcher_Error()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();
			
			new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory).Watcher("/test");
			var autoResetEvent = new AutoResetEvent(false);

			using var scope = _scopeFactory.CreateScope();
			// ISynchronize is a scoped service
			var synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>() as FakeISynchronize;
			if ( synchronize == null )
				throw new NullReferenceException("FakeISynchronize should not be null ");

			var message = string.Empty;
			fakeIFileSystemWatcher.Error += (s, e) =>
			{
				autoResetEvent.Set();
				message = e.GetException().Message;
			};
			
			fakeIFileSystemWatcher.TriggerOnError(new ErrorEventArgs(new InternalBufferOverflowException("test") ));
			var wasSignaled = autoResetEvent.WaitOne(TimeSpan.FromSeconds(200));
			
			Assert.IsTrue(wasSignaled);
			Assert.IsTrue(message.Contains("test"));
		}
		
		[TestMethod]
		[Timeout(400)]
		public void Watcher_Changed()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();
			new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory).Watcher("/test");

			using var scope = _scopeFactory.CreateScope();
			// ISynchronize is a scoped service
			var synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>() as FakeISynchronize;

			if ( synchronize == null )
				throw new NullReferenceException("FakeISynchronize should not be null ");

			var logger = scope.ServiceProvider.GetRequiredService<IWebLogger>() as FakeIWebLogger;
			
			fakeIFileSystemWatcher.TriggerOnChanged(new FileSystemEventArgs(WatcherChangeTypes.Changed, "/","test"));

			Console.WriteLine(logger.TrackedDebug.LastOrDefault().Item2);
			Assert.IsTrue(logger.TrackedDebug.LastOrDefault().Item2.Contains("/test"));
			Assert.IsTrue(logger.TrackedDebug.LastOrDefault().Item2.Contains("Changed"));
		}
		
		
		[TestMethod]
		[Timeout(400)]
		public void Watcher_Renamed()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();
			new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory).Watcher("/test");

			using var scope = _scopeFactory.CreateScope();
			// ISynchronize is a scoped service
			var synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>() as FakeISynchronize;
			if ( synchronize == null )
				throw new NullReferenceException("FakeISynchronize should not be null ");

			var logger = scope.ServiceProvider.GetRequiredService<IWebLogger>() as FakeIWebLogger;

			
			fakeIFileSystemWatcher.TriggerOnRename(new RenamedEventArgs(WatcherChangeTypes.Renamed, 
				"/","test","test"));
			
			Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2.Contains("/test"));
			Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2.Contains("OnRenamed to"));
		}

		[TestMethod]
		[Timeout(200)]
		public void Watcher_Retry_Ok()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();

			var result = new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory).Retry(fakeIFileSystemWatcher);
			
			Assert.IsTrue(result);	
		}
		
		
		[TestMethod]
		[Timeout(500)]
		public void Watcher_CrashAnd_Retry()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper()
			{
				CrashOnEnableRaisingEvents = true
			};
			
			fakeIFileSystemWatcher.EnableRaisingEvents = false;
			
			var result = new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory).Retry(fakeIFileSystemWatcher,1,0);
			
			Assert.IsFalse(result);	
		}
	}
}
