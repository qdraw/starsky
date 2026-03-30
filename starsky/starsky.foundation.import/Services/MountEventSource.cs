using System;
using System.Runtime.InteropServices;
using System.Threading;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.import.Services;

[Service(typeof(IMountEventSource), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class MountEventSource(IWebLogger logger) : IMountEventSource
{
	private const string DiskArbitrationLib =
		"/System/Library/Frameworks/DiskArbitration.framework/DiskArbitration";
	private const string CoreFoundationLib =
		"/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

	private readonly object _sync = new();
	private IntPtr _runLoop;
	private volatile bool _isRunning;
	private bool _disposed;

	public bool IsRunning => _isRunning;

	public event Action<MountAppearedEventModel>? MountAppeared;

	public bool Start()
	{
		lock ( _sync )
		{
			if ( _isRunning )
			{
				return true;
			}

			if ( !OperatingSystem.IsMacOS() )
			{
				return false;
			}

			var thread = new Thread(RunLoopThread)
			{
				Name = "Starsky.MountEventSource",
				IsBackground = true
			};
			thread.Start();
			return true;
		}
	}

	public void Stop()
	{
		lock ( _sync )
		{
			if ( _runLoop != IntPtr.Zero )
			{
				CFRunLoopStop(_runLoop);
			}
		}
	}

	private void RunLoopThread()
	{
		try
		{
			var session = DASessionCreate(IntPtr.Zero);
			if ( session == IntPtr.Zero )
			{
				logger.LogError("[MountEventSource] DASessionCreate failed");
				return;
			}

			_runLoop = CFRunLoopGetCurrent();
			var defaultRunLoopMode = CFStringCreateWithCString(IntPtr.Zero,
				"kCFRunLoopDefaultMode", 0x08000100);
			DARegisterDiskAppearedCallback(session, IntPtr.Zero, OnDiskAppeared, IntPtr.Zero);
			DASessionScheduleWithRunLoop(session, _runLoop, defaultRunLoopMode);
			_isRunning = true;
			CFRunLoopRun();
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, "[MountEventSource] mount watcher thread failed");
		}
		finally
		{
			_isRunning = false;
			_runLoop = IntPtr.Zero;
		}
	}

	private void OnDiskAppeared(IntPtr disk, IntPtr context)
	{
		try
		{
			MountAppeared?.Invoke(new MountAppearedEventModel());
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, "[MountEventSource] callback failed");
		}
	}

	public void Dispose()
	{
		if ( _disposed )
		{
			return;
		}

		Stop();
		_disposed = true;
	}

	private delegate void DiskAppearedCallback(IntPtr disk, IntPtr context);

	[DllImport(DiskArbitrationLib)]
	private static extern IntPtr DASessionCreate(IntPtr allocator);

	[DllImport(DiskArbitrationLib)]
	private static extern void DARegisterDiskAppearedCallback(IntPtr session,
		IntPtr match, DiskAppearedCallback callback, IntPtr context);

	[DllImport(DiskArbitrationLib)]
	private static extern void DASessionScheduleWithRunLoop(IntPtr session,
		IntPtr runLoop, IntPtr runLoopMode);

	[DllImport(CoreFoundationLib)]
	private static extern void CFRunLoopRun();

	[DllImport(CoreFoundationLib)]
	private static extern IntPtr CFRunLoopGetCurrent();

	[DllImport(CoreFoundationLib)]
	private static extern void CFRunLoopStop(IntPtr runLoop);

	[DllImport(CoreFoundationLib)]
	private static extern IntPtr CFStringCreateWithCString(IntPtr allocator,
		string cStr, uint encoding);
}





