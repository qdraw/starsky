using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.WatcherServices;

namespace starskytest.starsky.foundation.sync.WatcherServices
{
	[TestClass]
	public class BufferingFileSystemWatcherTest
	{
		private readonly string _tempFolder;

		public BufferingFileSystemWatcherTest()
		{
			_tempFolder = Path.Combine(new AppSettings().TempFolder, "__watcher");
			new StorageHostFullPathFilesystem().CreateDirectory(_tempFolder);
		}
		
		
		[TestMethod]
		public void ctor_Default()
		{
			var bufferingFileSystemWatcher = new BufferingFileSystemWatcher();
			Assert.IsNotNull(bufferingFileSystemWatcher);

			bufferingFileSystemWatcher.EnableRaisingEvents = false;
			bufferingFileSystemWatcher.Dispose();
		}
		
		[TestMethod]
		public void ctor_Path()
		{
			var bufferingFileSystemWatcher = new BufferingFileSystemWatcher(_tempFolder);
			Assert.IsNotNull(bufferingFileSystemWatcher);
			Assert.AreEqual(bufferingFileSystemWatcher.Path, _tempFolder);

			bufferingFileSystemWatcher.EnableRaisingEvents = false;
			bufferingFileSystemWatcher.Dispose();
		}
		
		[TestMethod]
		public void ctor_PathFilter()
		{
			var bufferingFileSystemWatcher = new BufferingFileSystemWatcher(_tempFolder,"*.txt");
			Assert.AreEqual(bufferingFileSystemWatcher.Path, new AppSettings().TempFolder);
			Assert.IsNotNull(bufferingFileSystemWatcher);

			bufferingFileSystemWatcher.EnableRaisingEvents = false;
			bufferingFileSystemWatcher.Dispose();
		}
		
		[TestMethod]
		public void ctor_SetPath()
		{
			var bufferingFileSystemWatcher = new BufferingFileSystemWatcher();
			bufferingFileSystemWatcher.Path = new AppSettings().TempFolder;
			Assert.AreEqual(bufferingFileSystemWatcher.Path, new AppSettings().TempFolder);
			Assert.IsNotNull(bufferingFileSystemWatcher);
			
			bufferingFileSystemWatcher.EnableRaisingEvents = false;
			bufferingFileSystemWatcher.Dispose();
		}
		
		[TestMethod]
		public void EnableRaisingEvents()
		{
			var watcher = new FileSystemWatcher(_tempFolder);
			watcher.Filter = "*.txt";

			var wrapper = new BufferingFileSystemWatcher(watcher);
			
			wrapper.EnableRaisingEvents = true;
			
			Assert.IsTrue(watcher.EnableRaisingEvents);
			
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
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

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
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

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
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

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
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

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
		
		[TestMethod]
		public void NotifyFilter()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			const NotifyFilters expectedFilter = new NotifyFilters();
			wrapper.NotifyFilter = expectedFilter;
			
			Assert.AreEqual(expectedFilter, watcher.NotifyFilter);
			Assert.AreEqual(expectedFilter, wrapper.NotifyFilter);

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
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

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
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

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
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
			
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
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

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
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

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
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

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
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

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
		
		[TestMethod]
		public void NotifyExistingFiles_All_Remove()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.OrderByOldestFirst = true;
			
			var message = "";
			// ReSharper disable once EventUnsubscriptionViaAnonymousDelegate
			wrapper.All -= (_, s) =>
			{
				message = s.FullPath;
			};
			
			wrapper.NotifyExistingFiles();
			
			Assert.IsFalse(message.StartsWith(new AppSettings().TempFolder));

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
		
		[TestMethod]
		public void NotifyExistingFiles_Created_Add()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.OrderByOldestFirst = true;
			
			var message = "";
			FileSystemEventHandler welcome = (_, s) =>
			{
				message = s.FullPath;
			};

			wrapper.Created += welcome;

			
			Assert.IsFalse(message.StartsWith(new AppSettings().TempFolder));

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
		
		[TestMethod]
		public void NotifyExistingFiles_Created_Remove()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.OrderByOldestFirst = true;
			
			var message = "";
			FileSystemEventHandler welcome = (_, s) =>
			{
				message = s.FullPath;
			};

			wrapper.Created += welcome;
			wrapper.Created -= welcome;

			wrapper.NotifyExistingFiles();
			
			Assert.IsFalse(message.StartsWith(new AppSettings().TempFolder));

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
		
				
		[TestMethod]
		public void NotifyExistingFiles_Changed_Remove()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.OrderByOldestFirst = true;
			
			var message = "";
			FileSystemEventHandler welcome = (_, s) =>
			{
				message = s.FullPath;
			};

			wrapper.Changed += welcome;
			wrapper.Changed -= welcome;
		
			Assert.IsFalse(message.StartsWith(new AppSettings().TempFolder));

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
		
						
		[TestMethod]
		public void NotifyExistingFiles_Deleted_Remove()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.OrderByOldestFirst = true;
			
			var message = "";
			FileSystemEventHandler welcome = (_, s) =>
			{
				message = s.FullPath;
			};

			wrapper.Deleted += welcome;
			wrapper.Deleted -= welcome;
		
			Assert.IsFalse(message.StartsWith(new AppSettings().TempFolder));

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
		
								
		[TestMethod]
		public void NotifyExistingFiles_Renamed_Remove()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.OrderByOldestFirst = true;
			
			var message = "";
			RenamedEventHandler welcome = (_, s) =>
			{
				message = s.FullPath;
			};

			wrapper.Renamed += welcome;
			wrapper.Renamed -= welcome;
		
			Assert.IsFalse(message.StartsWith(new AppSettings().TempFolder));

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}

		[TestMethod]
		public void BufferEvent()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.EventQueueCapacity = 1;

			wrapper.StopRaisingBufferedEvents();

			wrapper.BufferEvent(null, null);
			wrapper.BufferEvent(null, null);

			Assert.IsNull(watcher.SynchronizingObject);
			
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
		
		[TestMethod]
		public void NotifyExistingFiles_Error_Remove()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			wrapper.OrderByOldestFirst = true;
			
			var message = "";
			ErrorEventHandler welcome = (_, s) =>
			{
				message = s.ToString();
			};

			wrapper.Error += welcome;
			wrapper.BufferingFileSystemWatcher_Error(null,
				new ErrorEventArgs(new Exception("1")));
			wrapper.Error -= welcome;
		
			Assert.AreEqual("System.IO.ErrorEventArgs",message);

			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}

		[TestMethod]
		public void RaiseBufferedEventsUntilCancelledInLoop_Created()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			var result = wrapper.RaiseBufferedEventsUntilCancelledInLoop(new RenamedEventArgs(WatcherChangeTypes.Created, string.Empty, string.Empty, string.Empty));
			Assert.AreEqual(WatcherChangeTypes.Created, result);
			
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
		
		[TestMethod]
		public void RaiseBufferedEventsUntilCancelledInLoop_Changed()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			var result = wrapper.RaiseBufferedEventsUntilCancelledInLoop(new RenamedEventArgs(WatcherChangeTypes.Changed, string.Empty, string.Empty, string.Empty));
			Assert.AreEqual(WatcherChangeTypes.Changed, result);
		}
				
		[TestMethod]
		public void RaiseBufferedEventsUntilCancelledInLoop_Deleted()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			var result = wrapper.RaiseBufferedEventsUntilCancelledInLoop(new RenamedEventArgs(WatcherChangeTypes.Deleted, string.Empty, string.Empty, string.Empty));
			Assert.AreEqual(WatcherChangeTypes.Deleted, result);
			
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
						
		[TestMethod]
		public void RaiseBufferedEventsUntilCancelledInLoop_Renamed()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);
			var result = wrapper.RaiseBufferedEventsUntilCancelledInLoop(new RenamedEventArgs(WatcherChangeTypes.Renamed, string.Empty, string.Empty, string.Empty));
			Assert.AreEqual(WatcherChangeTypes.Renamed, result);
			
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}

		[TestMethod]
		public void InvokeHandlerFileSystemEventHandlerNull()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);

			FileSystemEventHandler t = null;
			var result = wrapper.InvokeHandler(t, null);
			Assert.IsNull(result);
			
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
		
		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public void InvokeHandlerFileSystemEventHandler()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);

			var message = string.Empty;
			FileSystemEventHandler welcome = (_, s) =>
			{
				message = s.FullPath;
			};
			
			wrapper.InvokeHandler(welcome, null);
			
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
		
		[TestMethod]
		public void InvokeHandler_RenamedEventHandler_Null()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);

			RenamedEventHandler t = null;
			var result = wrapper.InvokeHandler(t, null);
			Assert.IsNull(result);
			
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
		
		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public void InvokeHandler_RenamedEventHandler()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);

			var message = string.Empty;
			RenamedEventHandler welcome = (_, s) =>
			{
				message = s.FullPath;
			};
			
			wrapper.InvokeHandler(welcome, null);
			
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
		
				
		[TestMethod]
		public void InvokeHandler_ErrorEventHandler_Null()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);

			ErrorEventHandler t = null;
			var result = wrapper.InvokeHandler(t, null);
			Assert.IsNull(result);
		}
		
		[TestMethod]
		[ExpectedException(typeof(NullReferenceException))]
		public void InvokeHandler_ErrorEventHandler()
		{
			var watcher = new FileSystemWatcher(new AppSettings().TempFolder);
			var wrapper = new BufferingFileSystemWatcher(watcher);

			var message = string.Empty;
			ErrorEventHandler welcome = (_, s) =>
			{
				message = s.ToString();
			};
			
			wrapper.InvokeHandler(welcome, null);
			
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		}
	}
}
