using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using starsky.foundation.mountwatch.MountWatcher.MacOS;
using starsky.foundation.mountwatch.MountWatcher.MacOS.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.mountwatch.MountWatcher;

/// <summary>
///     macOS mount watcher using DiskArbitration framework for event-driven notifications
/// </summary>
internal class MacMountWatcher : BaseMountWatcher
{
	private readonly MacMountWatcherDelegate.DiskAppearedCallback _diskAppearedCallback;
	private readonly MacMountWatcherDelegate.DiskDisappearedCallback _diskDisappearedCallback;
	private readonly HashSet<string> _knownVolumes = new(StringComparer.OrdinalIgnoreCase);
	private readonly object _knownVolumesLock = new();
	private readonly IStorage _storage;
	private readonly IMacMountWatcherSystem _system = new MacMountWatcherSystem();
	private Thread? _backupPollThread;
	private IntPtr _runLoop;
	private IntPtr _runLoopMode;
	private IntPtr _session;

	public MacMountWatcher(IWebLogger logger, int pollIntervalMs) :
		base(logger, pollIntervalMs)
	{
		_storage = new StorageHostFullPathFilesystem(logger);
		_diskAppearedCallback = OnDiskAppeared;
		_diskDisappearedCallback = OnDiskDisappeared;
	}

	internal MacMountWatcher(IWebLogger logger, IStorage storage,
		IMacMountWatcherSystem system, int pollIntervalMs)
		: this(logger, pollIntervalMs)
	{
		_storage = storage;
		_system = system;
	}

	/// <summary>
	///     Start watching for mount events using DiskArbitration
	/// </summary>
	public override void Start()
	{
		if ( IsRunning )
		{
			return;
		}

		IsRunning = true;
		WatchThread = new Thread(RunWatcher) { IsBackground = true };
		WatchThread.Start();

		// Safety-net for environments where DiskArbitration callbacks are delayed or absent.
		_backupPollThread = new Thread(RunBackupPollingLoop) { IsBackground = true };
		_backupPollThread.Start();
	}

	/// <summary>
	///     Stop watching for mount events
	/// </summary>
	public override void Stop()
	{
		IsRunning = false;

		try
		{
			if ( _session != IntPtr.Zero && _runLoop != IntPtr.Zero && _runLoopMode != IntPtr.Zero )
			{
				_system.DASessionUnscheduleWithRunLoop(_session, _runLoop,
					_runLoopMode);
			}

			if ( _runLoop != IntPtr.Zero )
			{
				_system.CFRunLoopStop(_runLoop);
			}
		}
		catch
		{
			// Ignore cleanup errors
		}

		WatchThread?.Join(TimeSpan.FromSeconds(5));
		_backupPollThread?.Join(TimeSpan.FromSeconds(5));
	}

	/// <summary>
	///     Get the list of currently mounted volumes
	/// </summary>
	public override List<string> GetMountedVolumes()
	{
		var mounts = new List<string>();

		try
		{
			const string volumesPath = "/Volumes";
			if ( _storage.ExistFolder(volumesPath) )
			{
				var children =
					_storage.GetDirectories(volumesPath).Where(d => !d.StartsWith('.'));
				mounts.AddRange(children);
			}

			if ( _storage.ExistFolder("/") )
			{
				mounts.Add("/");
			}
		}
		catch
		{
			// Ignore errors
		}

		return mounts;
	}

	/// <summary>
	///     Run the DiskArbitration event watcher
	/// </summary>
	internal void RunWatcher()
	{
		try
		{
			_session = _system.DASessionCreate(IntPtr.Zero);
			if ( _session == IntPtr.Zero )
			{
				logger.LogError(
					"DiskArbitration session unavailable, switching to polling fallback");
				RunPollingFallback();
				return;
			}

			_runLoop = _system.CFRunLoopGetCurrent();
			_runLoopMode = _system.CFStringCreateWithCString(IntPtr.Zero,
				_system.GetCfRunLoopDefaultMode(),
				_system.GetCfStringEncodingUtf8());

			if ( _runLoopMode == IntPtr.Zero )
			{
				logger.LogError(
					"Unable to create CFRunLoop default mode, switching to polling fallback");
				RunPollingFallback();
				return;
			}

			_system.DASessionScheduleWithRunLoop(_session, _runLoop, _runLoopMode);
			_system.DARegisterDiskAppearedCallback(_session, IntPtr.Zero,
				_diskAppearedCallback,
				IntPtr.Zero);
			_system.DARegisterDiskDisappearedCallback(_session, IntPtr.Zero,
				_diskDisappearedCallback,
				IntPtr.Zero);

			logger.LogInformation("DiskArbitration watcher active");
			_system.CFRunLoopRun();
		}
		catch ( Exception ex )
		{
			logger.LogError(
				$"DiskArbitration watcher failed, switching to polling fallback: {ex.Message}");
			RunPollingFallback();
		}
		finally
		{
			try
			{
				if ( _session != IntPtr.Zero && _runLoop != IntPtr.Zero &&
				     _runLoopMode != IntPtr.Zero )
				{
					_system.DASessionUnscheduleWithRunLoop(_session, _runLoop,
						_runLoopMode);
				}
			}
			catch
			{
				// Ignore cleanup errors
			}

			if ( _runLoopMode != IntPtr.Zero )
			{
				_system.CFRelease(_runLoopMode);
				_runLoopMode = IntPtr.Zero;
			}

			if ( _session != IntPtr.Zero )
			{
				_system.CFRelease(_session);
				_session = IntPtr.Zero;
			}
		}
	}

	/// <summary>
	///     Called when a disk appears
	/// </summary>
	private void OnDiskAppeared(IntPtr diskRef, IntPtr context)
	{
		try
		{
			EmitNewExternalMounts("DiskArbitration");
		}
		catch ( Exception ex )
		{
			logger.LogError($"Error handling macOS disk appeared callback: {ex.Message}");
		}
	}

	private void RunBackupPollingLoop()
	{
		while ( IsRunning )
		{
			try
			{
				EmitNewExternalMounts("polling backup");
			}
			catch
			{
				// Backup polling should never crash the watcher loop.
			}

			Thread.Sleep(PollIntervalMs);
		}
	}

	private void EmitNewExternalMounts(string source)
	{
		var newMounts = DetectNewExternalMounts(GetMountedVolumes());

		foreach ( var mountPath in newMounts )
		{
			logger.LogInformation($"macOS volume appeared ({source}): {mountPath}");
			OnMountDetected(mountPath);
		}
	}

	internal List<string> DetectNewExternalMounts(IEnumerable<string> mountedVolumes)
	{
		var currentExternalMounts = mountedVolumes
			.Where(IsExternalVolumePath)
			.ToList();
		var currentExternalMountSet = currentExternalMounts
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		List<string> newMounts;
		lock ( _knownVolumesLock )
		{
			// Reconcile stale entries in case eject events were missed.
			_knownVolumes.RemoveWhere(path => !currentExternalMountSet.Contains(path));

			newMounts = currentExternalMounts
				.Where(m => !_knownVolumes.Contains(m))
				.ToList();

			foreach ( var mountPath in currentExternalMounts )
			{
				_knownVolumes.Add(mountPath);
			}
		}

		return newMounts;
	}

	internal void UpdateKnownExternalMounts(IEnumerable<string> mountedVolumes)
	{
		var currentExternalMounts = mountedVolumes
			.Where(IsExternalVolumePath)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		lock ( _knownVolumesLock )
		{
			_knownVolumes.RemoveWhere(path => !currentExternalMounts.Contains(path));
		}
	}

	/// <summary>
	///     Called when a disk disappears
	/// </summary>
	private void OnDiskDisappeared(IntPtr diskRef, IntPtr context)
	{
		try
		{
			UpdateKnownExternalMounts(GetMountedVolumes());
		}
		catch ( Exception ex )
		{
			logger.LogError($"Error handling macOS disk disappeared callback: {ex.Message}");
		}
	}

	private static bool IsExternalVolumePath(string mountPath)
	{
		return mountPath.StartsWith("/Volumes/", StringComparison.OrdinalIgnoreCase);
	}
}
