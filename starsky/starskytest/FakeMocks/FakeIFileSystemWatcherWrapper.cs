using System;
using System.IO;
using starsky.foundation.sync.WatcherInterfaces;

namespace starskytest.FakeMocks;

public class FakeIFileSystemWatcherWrapper : IFileSystemWatcherWrapper
{
	public bool CrashOnEnableRaisingEvents { get; set; } = false;

	public bool EnableRaisingEventsPrivate { get; set; }

	public bool IsDisposed { get; set; }

	public event FileSystemEventHandler? Created;

	public event FileSystemEventHandler? Deleted;

	public event RenamedEventHandler? Renamed;

	public event FileSystemEventHandler? Changed;
	public event ErrorEventHandler? Error;

	public bool EnableRaisingEvents
	{
		get => EnableRaisingEventsPrivate;
		set
		{
			if ( CrashOnEnableRaisingEvents && value )
			{
				throw new Exception("test");
			}

			EnableRaisingEventsPrivate = value;
		}
	}

	public bool IncludeSubdirectories { get; set; }
	public string Path { get; set; } = string.Empty;
	public string Filter { get; set; } = string.Empty;
	public NotifyFilters NotifyFilter { get; set; }
	public int InternalBufferSize { get; set; }

	void IDisposable.Dispose()
	{
		GC.SuppressFinalize(this);
		IsDisposed = true;
		Dispose(true);
	}

	public void TriggerOnChanged(FileSystemEventArgs args)
	{
		Changed?.Invoke(this, args);
	}

	public void TriggerOnCreated(FileSystemEventArgs args)
	{
		Created?.Invoke(this, args);
	}

	public void TriggerOnDeleted(RenamedEventArgs args)
	{
		Deleted?.Invoke(this, args);
	}

	public void TriggerOnRename(RenamedEventArgs args)
	{
		Renamed?.Invoke(this, args);
	}

	public void TriggerOnError(ErrorEventArgs args)
	{
		Error?.Invoke(this, args);
	}

	protected virtual void Dispose(bool disposing)
	{
		IsDisposed = true;
	}
}
