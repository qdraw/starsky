using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.WatcherServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherServices
{
	[TestClass]
	public class BufferingFileSystemWatcherTest
	{
		[TestMethod]
		public void EnableRaisingEvents()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			watcher.Filter = "*.txt";

			new BufferingFileSystemWatcher(watcher).EnableRaisingEvents = true;
			
			Assert.IsTrue(watcher.EnableRaisingEvents);
			
			watcher.EnableRaisingEvents = false;
			watcher.Dispose();
		}
		
		[TestMethod]
		public void EnableRaisingDisableEvents()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.Filter = "*.txt";
			wrapper.EnableRaisingEvents = true;
			Assert.IsTrue(watcher.EnableRaisingEvents);
			Assert.IsTrue(wrapper.EnableRaisingEvents);

			wrapper.EnableRaisingEvents = false;
			
			Assert.IsFalse(watcher.EnableRaisingEvents);
			Assert.IsFalse(wrapper.EnableRaisingEvents);

			watcher.Dispose();
		}
		
		[TestMethod]
		public void Filter()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.Filter = "*.txt";
			
			Assert.AreEqual("*.txt", watcher.Filter);
			Assert.AreEqual("*.txt", wrapper.Filter);

			watcher.Dispose();
		}
				
		[TestMethod]
		public void IncludeSubdirectories()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.IncludeSubdirectories = true;
			
			Assert.AreEqual(true, watcher.IncludeSubdirectories);
			Assert.AreEqual(true, wrapper.IncludeSubdirectories);

			watcher.Dispose();
		}
						
		[TestMethod]
		public void InternalBufferSize()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.InternalBufferSize = 5000;
			
			Assert.AreEqual(5000, watcher.InternalBufferSize);
			Assert.AreEqual(5000, wrapper.InternalBufferSize);

			watcher.Dispose();
		}
		
		[TestMethod]
		public void SynchronizingObject()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.SynchronizingObject = null ;
			
			Assert.AreEqual(null, watcher.SynchronizingObject);
			Assert.AreEqual(null, wrapper.SynchronizingObject);

			watcher.Dispose();
		}
				
		[TestMethod]
		public void Site()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.Site = null ;
			
			Assert.AreEqual(null, watcher.Site);
			Assert.AreEqual(null, wrapper.Site);

			watcher.Dispose();
		}
		
		[TestMethod]
		public void OrderByOldestFirst()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.OrderByOldestFirst = true;
			
			// wrapper only
			Assert.AreEqual(true, wrapper.OrderByOldestFirst);
			
			watcher.Dispose();
		}
		
						
		[TestMethod]
		public void EventQueueCapacity()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.EventQueueCapacity = 32;
			
			// wrapper only
			Assert.AreEqual(32, wrapper.EventQueueCapacity);
			watcher.Dispose();
		}

		[TestMethod]
		public void NotifyExistingFiles()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			
			var message = "";
			wrapper.Existed += (_, s) =>
			{
				message = s.FullPath;
			};
			
			wrapper.NotifyExistingFiles();
			
			Assert.IsTrue(message.StartsWith(new AppSettings().TempFolder));
			watcher.Dispose();
		}
		
		[TestMethod]
		public void NotifyExistingFiles_OrderByOldestFirst()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.OrderByOldestFirst = true;
			
			var message = "";
			wrapper.Existed += (_, s) =>
			{
				message = s.FullPath;
			};
			
			wrapper.NotifyExistingFiles();
			
			Assert.IsTrue(message.StartsWith(new AppSettings().TempFolder));
			watcher.Dispose();
		}
		
		[TestMethod]
		public void NotifyExistingFiles_All()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.OrderByOldestFirst = true;
			
			var message = "";
			wrapper.All += (_, s) =>
			{
				message = s.FullPath;
			};
			
			wrapper.NotifyExistingFiles();
			
			Assert.IsTrue(message.StartsWith(new AppSettings().TempFolder));

			watcher.Dispose();
		}
	}
}
