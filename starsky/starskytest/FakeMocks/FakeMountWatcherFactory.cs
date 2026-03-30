using System;
using System.Collections.Generic;
using starsky.foundation.mountwatch.Interfaces;
using starsky.foundation.mountwatch.Services;

namespace starskytest.FakeMocks;

public class FakeMountWatcherFactory : IMountWatcherFactory
{
	public IMountWatcher CreateMountWatcher()
	{
		return new FakeMountWatcher();
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
