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
			wrapper.EnableRaisingEvents = false;
			
			Assert.IsFalse(watcher.EnableRaisingEvents);
			watcher.Dispose();
		}
		
		[TestMethod]
		public void Filter()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.Filter = "*.txt";
			
			Assert.AreEqual("*.txt", watcher.Filter);
			watcher.Dispose();
		}
				
		[TestMethod]
		public void IncludeSubdirectories()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.IncludeSubdirectories = true;
			
			Assert.AreEqual(true, watcher.IncludeSubdirectories);
			watcher.Dispose();
		}
						
		[TestMethod]
		public void InternalBufferSize()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.InternalBufferSize = 5000;
			
			Assert.AreEqual(5000, watcher.InternalBufferSize);
			watcher.Dispose();
		}
	}
}
