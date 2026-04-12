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
internal abstract class BaseMountWatcher(IWebLogger logger, int pollIntervalMs) : IMountWatcher
{
	private const int DefaultPollIntervalMs = 1000;
	protected readonly IWebLogger logger = logger;
	protected readonly int PollIntervalMs = SanitizePollInterval(pollIntervalMs);
	protected bool IsRunning;
	protected internal Thread? WatchThread;

	private static int SanitizePollInterval(int pollIntervalMs)
	{
		return pollIntervalMs > 0 ? pollIntervalMs : DefaultPollIntervalMs;
	}

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
				Thread.Sleep(PollIntervalMs);

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
				Thread.Sleep(PollIntervalMs);
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
}
