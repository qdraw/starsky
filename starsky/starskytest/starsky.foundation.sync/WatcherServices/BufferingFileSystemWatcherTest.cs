using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.sync.WatcherServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherServices;

[TestClass]
public sealed class BufferingFileSystemWatcherTest
{
	private readonly string _tempExistingFilesFolder;
	private readonly string _tempFolder;

	public BufferingFileSystemWatcherTest()
	{
		_tempFolder = Path.Combine(new AppSettings().TempFolder, "__watcher");
		_tempExistingFilesFolder = Path.Combine(new AppSettings().TempFolder, "__watcher2");

		new StorageHostFullPathFilesystem(new FakeIWebLogger()).CreateDirectory(_tempFolder);
		new StorageHostFullPathFilesystem(new FakeIWebLogger()).CreateDirectory(
			_tempExistingFilesFolder);
	}

	[ClassCleanup(ClassCleanupBehavior.EndOfClass)]
	public static void ClassCleanup()
	{
		Console.WriteLine("cleanup run");
		var tempFolder = Path.Combine(new AppSettings().TempFolder, "__watcher");
		var tempExistingFilesFolder = Path.Combine(new AppSettings().TempFolder, "__watcher2");

		new StorageHostFullPathFilesystem(new FakeIWebLogger()).FolderDelete(tempFolder);
		new StorageHostFullPathFilesystem(new FakeIWebLogger()).FolderDelete(
			tempExistingFilesFolder);
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
		var bufferingFileSystemWatcher = new BufferingFileSystemWatcher(_tempFolder, "*.txt");
		Assert.AreEqual(bufferingFileSystemWatcher.Path, _tempFolder);
		Assert.IsNotNull(bufferingFileSystemWatcher);

		bufferingFileSystemWatcher.EnableRaisingEvents = false;
		bufferingFileSystemWatcher.Dispose();
	}

	[TestMethod]
	public void ctor_SetPath()
	{
		var bufferingFileSystemWatcher = new BufferingFileSystemWatcher();
		bufferingFileSystemWatcher.Path = _tempFolder;
		Assert.AreEqual(bufferingFileSystemWatcher.Path, _tempFolder);
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
		var watcher = new FileSystemWatcher(_tempFolder);
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
		var watcher = new FileSystemWatcher(_tempFolder);
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
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.IncludeSubdirectories = true;

		Assert.IsTrue(watcher.IncludeSubdirectories);
		Assert.IsTrue(wrapper.IncludeSubdirectories);

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void InternalBufferSize()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
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
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		const NotifyFilters expectedFilter = new();
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
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.SynchronizingObject = null;

		Assert.IsNull(watcher.SynchronizingObject);
		Assert.IsNull(wrapper.SynchronizingObject);

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void Site()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.Site = null;

		Assert.IsNull(watcher.Site);
		Assert.IsNull(wrapper.Site);

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void OrderByOldestFirst()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.OrderByOldestFirst = true;

		// wrapper only
		Assert.IsTrue(wrapper.OrderByOldestFirst);

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}


	[TestMethod]
	public void EventQueueCapacity()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
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
		var path = Path.Join(_tempExistingFilesFolder, "test.txt");
		new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStream(
			new MemoryStream(Array.Empty<byte>()), path);

		var watcher = new FileSystemWatcher(_tempExistingFilesFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);

		var message = "";
		wrapper.Existed += (_, s) => { message = s.FullPath; };

		wrapper.NotifyExistingFiles();

		Assert.IsTrue(message.StartsWith(_tempExistingFilesFolder));

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void NotifyExistingFiles_OrderByOldestFirst()
	{
		var path = Path.Join(_tempExistingFilesFolder, "test.txt");
		new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStream(
			new MemoryStream(Array.Empty<byte>()), path);

		var watcher = new FileSystemWatcher(_tempExistingFilesFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.OrderByOldestFirst = true;

		var message = "";
		wrapper.Existed += (_, s) => { message = s.FullPath; };

		wrapper.NotifyExistingFiles();

		Assert.IsTrue(message.StartsWith(_tempFolder));

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void NotifyExistingFiles_All()
	{
		var path = Path.Join(_tempExistingFilesFolder, "test.txt");
		new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStream(
			new MemoryStream(Array.Empty<byte>()), path);

		var watcher = new FileSystemWatcher(_tempExistingFilesFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.OrderByOldestFirst = true;

		var message = "";
		FileSystemEventHandler welcome = (_, s) => { message = s.FullPath; };

		wrapper.All += welcome;

		wrapper.NotifyExistingFiles();

		Assert.IsTrue(message.StartsWith(_tempFolder));

		// and remove event
		wrapper.All -= welcome;

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	[SuppressMessage("ReSharper", "EventUnsubscriptionViaAnonymousDelegate")]
	public void NotifyExistingFiles_All_Remove()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.OrderByOldestFirst = true;

		var message = "";
		wrapper.All -= (_, s) => { message = s.FullPath; };

		wrapper.NotifyExistingFiles();

		Assert.IsFalse(message.StartsWith(_tempFolder));

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void NotifyExistingFiles_Created_Add()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.OrderByOldestFirst = true;

		var message = "";

		void Welcome(object _, FileSystemEventArgs s)
		{
			message = s.FullPath;
		}

		wrapper.Created += Welcome;


		Assert.IsFalse(message.StartsWith(_tempFolder));

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void NotifyExistingFiles_Created_Remove()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.OrderByOldestFirst = true;

		var message = "";
		FileSystemEventHandler welcome = (_, s) => { message = s.FullPath; };

		wrapper.Created += welcome;
		wrapper.Created -= welcome;

		wrapper.NotifyExistingFiles();

		Assert.IsFalse(message.StartsWith(_tempFolder));

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}


	[TestMethod]
	public void NotifyExistingFiles_Changed_Remove()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.OrderByOldestFirst = true;

		var message = "";
		FileSystemEventHandler welcome = (_, s) => { message = s.FullPath; };

		wrapper.Changed += welcome;
		wrapper.Changed -= welcome;

		Assert.IsFalse(message.StartsWith(_tempFolder));

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}


	[TestMethod]
	public void NotifyExistingFiles_Deleted_Remove()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.OrderByOldestFirst = true;

		var message = "";
		FileSystemEventHandler welcome = (_, s) => { message = s.FullPath; };

		wrapper.Deleted += welcome;
		wrapper.Deleted -= welcome;

		Assert.IsFalse(message.StartsWith(_tempFolder));

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}


	[TestMethod]
	public void NotifyExistingFiles_Renamed_Remove()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.OrderByOldestFirst = true;

		var message = "";
		RenamedEventHandler welcome = (_, s) => { message = s.FullPath; };

		wrapper.Renamed += welcome;
		wrapper.Renamed -= welcome;

		Assert.IsFalse(message.StartsWith(_tempFolder));

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void BufferEvent()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.EventQueueCapacity = 1;

		wrapper.StopRaisingBufferedEvents();

		wrapper.BufferEvent(null!, null!);
		wrapper.BufferEvent(null!, null!);

		Assert.IsNull(watcher.SynchronizingObject);

		wrapper.EnableRaisingEvents = false;

		try
		{
			wrapper.Dispose();
		}
		catch ( Exception )
		{
			// do nothing
		}

		watcher.Dispose();
	}

	[TestMethod]
	public void NotifyExistingFiles_Error_Remove()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		wrapper.OrderByOldestFirst = true;

		var message = "";
		ErrorEventHandler welcome = (_, s) => { message = s.ToString(); };

		wrapper.Error += welcome;
		wrapper.BufferingFileSystemWatcher_Error(null!,
			new ErrorEventArgs(new Exception("1")));
		wrapper.Error -= welcome;

		Assert.AreEqual("System.IO.ErrorEventArgs", message);

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void RaiseBufferedEventsUntilCancelledInLoop_Created()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		var args = new RenamedEventArgs(WatcherChangeTypes.Created,
			string.Empty, string.Empty, string.Empty);
		var result = wrapper.RaiseBufferedEventsUntilCancelledInLoop(args);
		Assert.AreEqual(WatcherChangeTypes.Created, result);

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void RaiseBufferedEventsUntilCancelledInLoop_Changed()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		var args = new RenamedEventArgs(WatcherChangeTypes.Changed,
			string.Empty, string.Empty, string.Empty);
		var result = wrapper.RaiseBufferedEventsUntilCancelledInLoop(args);
		Assert.AreEqual(WatcherChangeTypes.Changed, result);
	}

	[TestMethod]
	public void RaiseBufferedEventsUntilCancelledInLoop_Deleted()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		var result = wrapper.RaiseBufferedEventsUntilCancelledInLoop(
			new RenamedEventArgs(WatcherChangeTypes.Deleted, string.Empty, string.Empty,
				string.Empty));
		Assert.AreEqual(WatcherChangeTypes.Deleted, result);

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void RaiseBufferedEventsUntilCancelledInLoop_Renamed()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);
		var result = wrapper.RaiseBufferedEventsUntilCancelledInLoop(
			new RenamedEventArgs(WatcherChangeTypes.Renamed, string.Empty, string.Empty,
				string.Empty));
		Assert.AreEqual(WatcherChangeTypes.Renamed, result);

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void InvokeHandlerFileSystemEventHandlerNull()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);

		FileSystemEventHandler t = null!;
		var result = wrapper.InvokeHandler(t, null!);
		Assert.IsNull(result);

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void InvokeHandlerFileSystemEventHandler_NullReferenceException()
	{
		// Arrange
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);

		var message = string.Empty;
		FileSystemEventHandler welcome = (_, s) => { message = s.FullPath; };

		// Act & Assert
		Assert.ThrowsExactly<NullReferenceException>(() =>
		{
			wrapper.InvokeHandler(welcome, null!);

			// Cleanup
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		});

		// Optionally, check that the message is not altered
		Assert.IsNotNull(message);
	}

	[TestMethod]
	public void InvokeHandler_RenamedEventHandler_Null()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);

		RenamedEventHandler t = null!;
		var result = wrapper.InvokeHandler(t, null);
		Assert.IsNull(result);

		wrapper.EnableRaisingEvents = false;
		wrapper.Dispose();
		watcher.Dispose();
	}

	[TestMethod]
	public void InvokeHandler_RenamedEventHandler_ExpectException()
	{
		// Arrange
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);

		var message = string.Empty;
		RenamedEventHandler welcome = (_, s) => { message = s.FullPath; };

		// Act & Assert
		Assert.ThrowsExactly<ArgumentNullException>(() =>
		{
			wrapper.InvokeHandler(welcome, null);

			// Cleanup
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		});

		// Optionally, check that the message is not altered
		Assert.IsNotNull(message);
	}


	[TestMethod]
	public void InvokeHandler_ErrorEventHandler_Null()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);

		ErrorEventHandler? t = null;
		var result = wrapper.InvokeHandler(t, null!);
		Assert.IsNull(result);
	}

	[TestMethod]
	public void InvokeHandler_ErrorEventHandler()
	{
		// Arrange
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);

		var message = string.Empty;
		ErrorEventHandler welcome = (_, s) => { message = s.ToString(); };

		// Act & Assert
		Assert.ThrowsExactly<NullReferenceException>(() =>
		{
			wrapper.InvokeHandler(welcome, null!);

			// Cleanup
			wrapper.EnableRaisingEvents = false;
			wrapper.Dispose();
			watcher.Dispose();
		});

		// Optionally, check that the message was not altered
		Assert.IsNotNull(message);
	}

	[TestMethod]
	[SuppressMessage("Usage", "S3966:Objects should not be disposed more than once")]
	public void DisposeDouble()
	{
		var watcher = new FileSystemWatcher(_tempFolder);
		var wrapper = new BufferingFileSystemWatcher(watcher);

		wrapper.Dispose();
		Assert.IsTrue(wrapper.IsDisposed);

		wrapper.Dispose();

		Assert.IsTrue(wrapper.IsDisposed);
	}
}
