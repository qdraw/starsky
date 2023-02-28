using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.sync.WatcherHelpers;
using starsky.foundation.sync.WatcherInterfaces;
using starsky.foundation.sync.WatcherServices;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherServices
{
	[TestClass]
	public sealed class DiskWatcherTest
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly CreateAnImage _createAnImage;

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
			_createAnImage = new CreateAnImage();

		}
		
		[TestMethod]
		public void Watcher_ExpectPath()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();
			var watcher= new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory);
			var path = new CreateAnImage().BasePath;
			watcher.Watcher(new CreateAnImage().BasePath);
			watcher.Dispose();
			Assert.AreEqual(path,fakeIFileSystemWatcher.Path);
		}
		
		[TestMethod]
		[Timeout(5000)]
		public void DiskWatcherTest_Watcher_Error()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();
			
			var watcher = new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory);
			watcher.Watcher(_createAnImage.BasePath);
			
			var autoResetEvent = new AutoResetEvent(false);

			using var scope = _scopeFactory.CreateScope();
			// ISynchronize is a scoped service
			var synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>() as FakeISynchronize;
			if ( synchronize == null )
				throw new NullReferenceException("FakeISynchronize should not be null ");

			var message = string.Empty;
			fakeIFileSystemWatcher.Error += (_, e) =>
			{
				autoResetEvent.Set();
				message = e.GetException().Message;
			};
			
			fakeIFileSystemWatcher.TriggerOnError(new ErrorEventArgs(new InternalBufferOverflowException("test") ));
			fakeIFileSystemWatcher.TriggerOnError(new ErrorEventArgs(new InternalBufferOverflowException("test") ));

			var wasSignaled = autoResetEvent.WaitOne(TimeSpan.FromSeconds(200));
			if ( !wasSignaled )
			{
				wasSignaled = autoResetEvent.WaitOne(TimeSpan.FromSeconds(200));
			}
			watcher.Dispose();
			
			Assert.IsTrue(wasSignaled);
			Assert.IsTrue(message.Contains("test"));
		}

		[TestMethod]
		public void Watcher_DirNotFound()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();
			var watcher = new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory);
			watcher.Watcher("C:\\not-found");
			
			using var scope = _scopeFactory.CreateScope();
			var logger = scope.ServiceProvider.GetRequiredService<IWebLogger>() as FakeIWebLogger;

			Assert.IsTrue(logger?.TrackedExceptions.LastOrDefault().Item2.Contains("is not started"));
		}

		[TestMethod]
		[Timeout(3000)]
		public void Watcher_Changed()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();
			var watcher = new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory);
			watcher.Watcher(new CreateAnImage().BasePath);

			using var scope = _scopeFactory.CreateScope();
			// ISynchronize is a scoped service
			var synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>() as FakeISynchronize;

			if ( synchronize == null )
				throw new NullReferenceException("FakeISynchronize should not be null ");

			var logger = scope.ServiceProvider.GetRequiredService<IWebLogger>() as FakeIWebLogger;
			
			fakeIFileSystemWatcher.TriggerOnChanged(new FileSystemEventArgs(WatcherChangeTypes.Changed, "/","test.jpg"));

			watcher.Dispose();

			Console.WriteLine(logger!.TrackedDebug.LastOrDefault().Item2);
			Assert.IsTrue(logger.TrackedDebug.LastOrDefault().Item2.Contains("/test"));
			Assert.IsTrue(logger.TrackedDebug.LastOrDefault().Item2.Contains("Changed"));
		}
		
		
		[TestMethod]
		[Timeout(3000)]
		public void Watcher_Renamed()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();

			var watcher = new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory);
			watcher.Watcher(_createAnImage.BasePath);

			using var scope = _scopeFactory.CreateScope();
			// ISynchronize is a scoped service
			var synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>() as FakeISynchronize;
			if ( synchronize == null )
			{
				throw new NullReferenceException("FakeISynchronize should not be null ");
			}

			var logger = scope.ServiceProvider.GetRequiredService<IWebLogger>() as FakeIWebLogger;

			fakeIFileSystemWatcher.TriggerOnRename(new RenamedEventArgs(WatcherChangeTypes.Renamed, 
				_createAnImage.BasePath,_createAnImage.FileName,"test"));
			
			watcher.Dispose();
			
			Assert.IsTrue(logger!.TrackedInformation.LastOrDefault().Item2.Contains(_createAnImage.FileName));
			Assert.IsTrue(logger.TrackedInformation.LastOrDefault().Item2.Contains("OnRenamed to"));
			
		}

		[TestMethod]
		[Timeout(2000)]
		public void Watcher_Retry_Ok()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();

			var watcher = new DiskWatcher(
				fakeIFileSystemWatcher,
				_scopeFactory);
			
			var result = watcher.Retry(fakeIFileSystemWatcher);
			watcher.Dispose();
			
			Assert.IsTrue(result);	
		}
		
		
		[TestMethod]
		[Timeout(500)]
		public void Watcher_CrashAnd_Retry()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper()
			{
				CrashOnEnableRaisingEvents = true,
				Path = new CreateAnImage().BasePath
			};
			
			fakeIFileSystemWatcher.EnableRaisingEvents = false;
			
			var watcher =
				new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory);
			
			var result = watcher.Retry(fakeIFileSystemWatcher,1,0);
			watcher.Dispose();
			Assert.IsFalse(result);	
		}

		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public void OnChanged_ShouldHitQueueProcessor()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper()
			{
				CrashOnEnableRaisingEvents = true
			};

			var event1 = new FileSystemEventArgs(WatcherChangeTypes.Changed,
				"t", "test.jpg");
			var watcher = new DiskWatcher(fakeIFileSystemWatcher, new FakeIWebLogger(), null);
			watcher.OnChanged(null, event1);
			
			watcher.Dispose();
		}
		
		[TestMethod]
		public void OnChanged_Should_Not_HitQueueProcessor_tmp()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper()
			{
				CrashOnEnableRaisingEvents = true
			};

			var event1 = new FileSystemEventArgs(WatcherChangeTypes.Changed,
				"t", "test.tmp");

			QueueProcessor processor = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			var watcher = new DiskWatcher(fakeIFileSystemWatcher, new FakeIWebLogger(), processor);
			watcher.OnChanged(null, event1);
			
			Assert.IsNull(processor);
		}
		
		[TestMethod]
		public void OnChanged_Should_Not_HitQueueProcessor_esp()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper()
			{
				CrashOnEnableRaisingEvents = true
			};

			var event1 = new FileSystemEventArgs(WatcherChangeTypes.Changed,
				"t", "test.esp");

			QueueProcessor processor = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			var watcher = new DiskWatcher(fakeIFileSystemWatcher, new FakeIWebLogger(), processor);
			watcher.OnChanged(null, event1);
			
			Assert.IsNull(processor);
		}
		
		
		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public void OnRenamed_ShouldHitQueueProcessor()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper()
			{
				CrashOnEnableRaisingEvents = true
			};

			var event1 = new RenamedEventArgs(WatcherChangeTypes.Renamed, 
				"t", "test.jpg", "test2.jpg");
			
			new DiskWatcher(fakeIFileSystemWatcher, new FakeIWebLogger(), null).OnRenamed(null, event1);
		}
		
		[TestMethod]
		public void OnRenamed_Should_Not_HitQueueProcessor_FromTmp()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper()
			{
				CrashOnEnableRaisingEvents = true
			};

			// /folder/.syncthing.20211222_112808_DSC00998.jpg.tmp OnRenamed to: /folder/20211222_112808_DSC00998.jpg
			var event1 = new RenamedEventArgs(WatcherChangeTypes.Renamed, 
				"t", "test2.jpg", ".syncthing.test.jpg.tmp" );

			var processor = new FakeIQueueProcessor();
			// ReSharper disable once ExpressionIsAlwaysNull
			new DiskWatcher(fakeIFileSystemWatcher, new FakeIWebLogger(), processor).OnRenamed(null, event1);
			
			Assert.AreEqual(1, processor.Data.Count);
			Assert.AreEqual($"t{Path.DirectorySeparatorChar}test2.jpg", processor.Data[0].Item1);
			Assert.AreEqual(null, processor.Data[0].Item2);
		}
		
		[TestMethod]
		public void OnRenamed_Should_Not_HitQueueProcessor_FromTmp2()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper()
			{
				CrashOnEnableRaisingEvents = true
			};

			// /folder/tmp.{guid}.test.jpg OnRenamed to: /folder/20211222_112808_DSC00998.jpg
			var event1 = new RenamedEventArgs(WatcherChangeTypes.Renamed, 
				_createAnImage.BasePath, _createAnImage.FileName, Path.DirectorySeparatorChar + "tmp.{guid}.test.jpg" );

			var processor = new FakeIQueueProcessor();
			// ReSharper disable once ExpressionIsAlwaysNull
			new DiskWatcher(fakeIFileSystemWatcher, new FakeIWebLogger(), processor).OnRenamed(null, event1);
			
			Assert.AreEqual(1, processor.Data.Count);
			Assert.AreEqual(_createAnImage.FullFilePath, processor.Data[0].Item1);
			Assert.AreEqual(null, processor.Data[0].Item2);
		}
		
		[TestMethod]
		public void OnRenamed_Should_Not_HitQueueProcessor_ToTmp()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper()
			{
				CrashOnEnableRaisingEvents = true
			};

			// TO temp
			// /folder/20211222_112808_DSC00998.jpg  OnRenamed to: /folder/.syncthing.20211222_112808_DSC00998.jpg.tmp
			var event1 = new RenamedEventArgs(WatcherChangeTypes.Renamed, 
				"t", ".syncthing.test.jpg.tmp","test2.jpg" );

			var processor = new FakeIQueueProcessor();
			// ReSharper disable once ExpressionIsAlwaysNull
			new DiskWatcher(fakeIFileSystemWatcher, new FakeIWebLogger(), processor).OnRenamed(null, event1);
			
			Assert.AreEqual(0, processor.Data.Count);
		}
		
		[TestMethod]
		public void OnRenamed_Should_Not_HitQueueProcessor_ToTmp2()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper()
			{
				CrashOnEnableRaisingEvents = true
			};

			// TO temp
			// /folder/20211222_112808_DSC00998.jpg  OnRenamed to: /folder/tmp.{guid}.test.jpg
			var event1 = new RenamedEventArgs(WatcherChangeTypes.Renamed, 
				"t", Path.DirectorySeparatorChar + "tmp.{guid}.test.jpg","test2.jpg" );

			var processor = new FakeIQueueProcessor();
			// ReSharper disable once ExpressionIsAlwaysNull
			new DiskWatcher(fakeIFileSystemWatcher, new FakeIWebLogger(), processor).OnRenamed(null, event1);
			
			Assert.AreEqual(0, processor.Data.Count);
		}

		[TestMethod]
		public void Dispose()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper()
			{
				CrashOnEnableRaisingEvents = true
			};
			var watcher = new DiskWatcher(fakeIFileSystemWatcher, new FakeIWebLogger(),null!);
			watcher.Dispose();
			
			Assert.IsTrue(fakeIFileSystemWatcher.IsDisposed);
		}
		
		[TestMethod]
		public void Dispose_default()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper()
			{
				CrashOnEnableRaisingEvents = true
			};
			
			Assert.IsFalse(fakeIFileSystemWatcher.IsDisposed);
		}
	}
}
