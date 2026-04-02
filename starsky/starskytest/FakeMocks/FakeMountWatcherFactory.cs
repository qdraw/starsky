using System;
using System.Collections.Generic;
using starsky.foundation.mountwatch.Interfaces;
using starsky.foundation.mountwatch.Services;

namespace starskytest.FakeMocks;

public class FakeMountWatcherFactory : IMountWatcherFactory
{
	public IMountWatcher CreateMountWatcher()
	{
		var watcher = new FakeMountWatcher();
		watcher.MountDetected += (_, _) =>
		{
			// This event handler is intentionally left empty to suppress "event is never used" warnings
		};
		return watcher;
	}

	private sealed class FakeMountWatcher : IMountWatcher
	{
		public event EventHandler<MountDetectedEventArgs>? MountDetected;

		public void Start()
		{
		}

		public void Stop()
		{
		}

		public List<string> GetMountedVolumes()
		{
			return [];
		}
	}
}
