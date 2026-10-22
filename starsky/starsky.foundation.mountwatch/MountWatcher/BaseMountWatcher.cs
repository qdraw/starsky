using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using starsky.foundation.mountwatch.MountWatcher.Interfaces;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.mountwatch.MountWatcher;

/// <summary>
///     Base class for mount watchers with shared polling fallback logic
/// </summary>
internal abstract class BaseMountWatcher(IWebLogger logger, int pollIntervalMs) : IMountWatcher, IDisposable
{
	protected readonly IWebLogger logger = logger;
	protected readonly int PollIntervalMs = pollIntervalMs;
	protected bool IsRunning;
	protected internal Thread? WatchThread;
	private readonly CancellationTokenSource _pollCts = new();

	public event EventHandler<MountDetectedEventArgs>? MountDetected;

	/// <summary>
	///     Start watching for mount events
	/// </summary>
	public abstract void Start();

	/// <summary>
	///     Stop watching for mount events
	/// </summary>
	public abstract void Stop();

	/// <summary>
	///     Get currently mounted volumes
	/// </summary>
	public abstract List<string> GetMountedVolumes();

	protected void CancelPolling() => _pollCts.Cancel();

	/// <summary>
	///     Run polling fallback - polls for mount changes at regular intervals
	/// </summary>
	protected void RunPollingFallback()
	{
		var previousMounts = new HashSet<string>(GetMountedVolumes());

		while ( IsRunning )
		{
			try
			{
				_pollCts.Token.WaitHandle.WaitOne(PollIntervalMs);
				if ( _pollCts.Token.IsCancellationRequested )
				{
					break;
				}

				var currentMounts = GetMountedVolumes();
				var newMounts = currentMounts.Except(previousMounts).ToList();

				if ( newMounts.Count > 0 )
				{
					foreach ( var mount in newMounts )
					{
						previousMounts.Add(mount);
						OnMountDetected(mount);
					}
				}

				// Check for unmounted volumes
				var removedMounts = previousMounts.Except(currentMounts).ToList();
				foreach ( var mount in removedMounts )
				{
					previousMounts.Remove(mount);
				}
			}
			catch
			{
				_pollCts.Token.WaitHandle.WaitOne(PollIntervalMs);
			}
		}
	}

	/// <summary>
	///     Raise MountDetected event
	/// </summary>
	protected void OnMountDetected(string mountPath)
	{
		MountDetected?.Invoke(this,
			new MountDetectedEventArgs { MountPath = mountPath, DetectedAt = DateTime.UtcNow });
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if ( disposing )
		{
			_pollCts.Dispose();
		}
	}
}
