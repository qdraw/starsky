using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.mountwatch.MountWatcher;

/// <summary>
///     macOS mount watcher using DiskArbitration framework for event-driven notifications
/// </summary>
internal class MacMountWatcher : BaseMountWatcher
{
	private const int BackupPollIntervalMs = 2000;
	private const uint CfStringEncodingUtf8 = 0x08000100;
	private const string CfRunLoopDefaultMode = "kCFRunLoopDefaultMode";
	private readonly DiskAppearedCallback _diskAppearedCallback;
	private readonly DiskDisappearedCallback _diskDisappearedCallback;
	private readonly HashSet<string> _knownVolumes = new(StringComparer.OrdinalIgnoreCase);
	private readonly object _knownVolumesLock = new();
	private Thread? _backupPollThread;
	private IntPtr _runLoop;
	private IntPtr _runLoopMode;
	private IntPtr _session;

	public MacMountWatcher(IWebLogger logger) : base(logger)
	{
		_diskAppearedCallback = OnDiskAppeared;
		_diskDisappearedCallback = OnDiskDisappeared;
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

		lock ( _knownVolumesLock )
		{
			_knownVolumes.Clear();
			foreach ( var mountPath in GetMountedVolumes().Where(IsExternalVolumePath) )
			{
				_knownVolumes.Add(mountPath);
			}
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
				DASessionUnscheduleWithRunLoop(_session, _runLoop, _runLoopMode);
			}

			if ( _runLoop != IntPtr.Zero )
			{
				CFRunLoopStop(_runLoop);
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
			if ( Directory.Exists(volumesPath) )
			{
				var volumeInfo = new DirectoryInfo(volumesPath);
				mounts.AddRange(volumeInfo
					.GetDirectories()
					.Where(d => !d.Name.StartsWith("."))
					.Select(d => d.FullName));
			}

			if ( Directory.Exists("/") )
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

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern IntPtr DASessionCreate(IntPtr allocator);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern void DASessionScheduleWithRunLoop(
		IntPtr session, IntPtr runLoop, IntPtr runLoopMode);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern void DASessionUnscheduleWithRunLoop(
		IntPtr session, IntPtr runLoop, IntPtr runLoopMode);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern void DARegisterDiskAppearedCallback(
		IntPtr session, IntPtr match, DiskAppearedCallback callback, IntPtr context);

	[DllImport("/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration")]
	private static extern void DARegisterDiskDisappearedCallback(
		IntPtr session, IntPtr match, DiskDisappearedCallback callback, IntPtr context);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern IntPtr CFRunLoopGetCurrent();

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern void CFRunLoopRun();

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern void CFRunLoopStop(IntPtr runLoop);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern IntPtr CFStringCreateWithCString(
		IntPtr allocator, string cStr, uint encoding);

	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern void CFRelease(IntPtr cf);

	/// <summary>
	///     Run the DiskArbitration event watcher
	/// </summary>
	private void RunWatcher()
	{
		try
		{
			_session = DASessionCreate(IntPtr.Zero);
			if ( _session == IntPtr.Zero )
			{
				logger.LogError(
					"DiskArbitration session unavailable, switching to polling fallback");
				RunPollingFallback();
				return;
			}

			_runLoop = CFRunLoopGetCurrent();
			_runLoopMode = CFStringCreateWithCString(IntPtr.Zero, CfRunLoopDefaultMode,
				CfStringEncodingUtf8);

			if ( _runLoopMode == IntPtr.Zero )
			{
				logger.LogError(
					"Unable to create CFRunLoop default mode, switching to polling fallback");
				RunPollingFallback();
				return;
			}

			DASessionScheduleWithRunLoop(_session, _runLoop, _runLoopMode);
			DARegisterDiskAppearedCallback(_session, IntPtr.Zero, _diskAppearedCallback,
				IntPtr.Zero);
			DARegisterDiskDisappearedCallback(_session, IntPtr.Zero, _diskDisappearedCallback,
				IntPtr.Zero);

			logger.LogInformation("DiskArbitration watcher active");
			CFRunLoopRun();
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
					DASessionUnscheduleWithRunLoop(_session, _runLoop, _runLoopMode);
				}
			}
			catch
			{
				// Ignore cleanup errors
			}

			if ( _runLoopMode != IntPtr.Zero )
			{
				CFRelease(_runLoopMode);
				_runLoopMode = IntPtr.Zero;
			}

			if ( _session != IntPtr.Zero )
			{
				CFRelease(_session);
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

			Thread.Sleep(BackupPollIntervalMs);
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

		List<string> newMounts;
		lock ( _knownVolumesLock )
		{
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

	// DiskArbitration framework P/Invoke declarations
	private delegate void DiskAppearedCallback(IntPtr diskRef, IntPtr context);

	private delegate void DiskDisappearedCallback(IntPtr diskRef, IntPtr context);
}
