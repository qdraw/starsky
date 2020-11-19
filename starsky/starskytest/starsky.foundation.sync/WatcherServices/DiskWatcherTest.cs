using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.sync.SyncInterfaces;
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
			var serviceProvider = services.BuildServiceProvider();
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public void Watcher_ExpectPath()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();
			new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory, new ConsoleWrapper()).Watcher("/test");
			Assert.AreEqual("/test",fakeIFileSystemWatcher.Path);
		}
		
		[TestMethod]
		[Timeout(400)]
		public void Watcher_Error()
		{
			var fakeConsole = new FakeConsoleWrapper();
			
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();
			new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory, fakeConsole).Watcher("/test");
			var autoResetEvent = new AutoResetEvent(false);

			using var scope = _scopeFactory.CreateScope();
			// ISynchronize is a scoped service
			var synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>() as FakeISynchronize;

			if ( synchronize == null )
				throw new NullReferenceException("FakeISynchronize should not be null ");
			fakeIFileSystemWatcher.Error += (s, e) =>
			{
				Console.WriteLine();
				autoResetEvent.Set();
			};
			
			fakeIFileSystemWatcher.TriggerOnError(new ErrorEventArgs(new InternalBufferOverflowException() ));

			autoResetEvent.WaitOne(TimeSpan.FromSeconds(300));

			Assert.IsTrue(fakeConsole.WrittenLines[0].Contains("error"));
		}
		

		[TestMethod]
		[Timeout(400)]
		public void Watcher_Changed()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();
			new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory, new ConsoleWrapper()).Watcher("/test");
			var autoResetEvent = new AutoResetEvent(false);

			using var scope = _scopeFactory.CreateScope();
			// ISynchronize is a scoped service
			var synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>() as FakeISynchronize;

			if ( synchronize == null )
				throw new NullReferenceException("FakeISynchronize should not be null ");
			var receivedValue = string.Empty;
			synchronize.Receive += (s, e) =>
			{
				receivedValue = e;
				autoResetEvent.Set();
			};
			
			fakeIFileSystemWatcher.TriggerOnChanged(new FileSystemEventArgs(WatcherChangeTypes.Changed, "/","test"));

			var wasSignaled = autoResetEvent.WaitOne(TimeSpan.FromSeconds(300));
			Assert.IsTrue(wasSignaled);

			Assert.AreEqual("/test", receivedValue);
			Assert.AreEqual(new Tuple<string,bool>("/test", true), synchronize.Inputs.FirstOrDefault());
		}
		
		
		[TestMethod]
		[Timeout(400)]
		public void Watcher_Renamed()
		{
			var fakeIFileSystemWatcher = new FakeIFileSystemWatcherWrapper();
			new DiskWatcher(fakeIFileSystemWatcher, _scopeFactory, new ConsoleWrapper()).Watcher("/test");
			var autoResetEvent = new AutoResetEvent(false);

			using var scope = _scopeFactory.CreateScope();
			// ISynchronize is a scoped service
			var synchronize = scope.ServiceProvider.GetRequiredService<ISynchronize>() as FakeISynchronize;
			if ( synchronize == null )
				throw new NullReferenceException("FakeISynchronize should not be null ");
			
			var receivedValue = string.Empty;
			synchronize.Receive += (s, e) =>
			{
				receivedValue = e;
				autoResetEvent.Set();
			};
			
			fakeIFileSystemWatcher.TriggerOnRename(new RenamedEventArgs(WatcherChangeTypes.Renamed, 
				"/","test","test"));

			var wasSignaled = autoResetEvent.WaitOne(TimeSpan.FromSeconds(300));
			Assert.IsTrue(wasSignaled);

			Assert.AreEqual("/test", receivedValue);
			Assert.AreEqual(new Tuple<string,bool>("/test", true), synchronize.Inputs.FirstOrDefault());
		}
	}
}
