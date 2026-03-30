using System;
using System.Collections.Generic;
using System.Linq;
using starsky.foundation.mountwatch.Interfaces;
using starsky.foundation.mountwatch.Services;

namespace starskytest.FakeMocks;

public class FakeMountWatcherFactory : IMountWatcherFactory
{
	public IMountWatcher CreateMountWatcher()
	{
		return new FakeMountWatcher();
	}

	private class FakeMountWatcher : IMountWatcher
	{
		public event EventHandler<MountDetectedEventArgs>? MountDetected;

		public void Start()
		{
		}

		public void Stop()
		{
		}

		public IEnumerable<string> GetMountedVolumes()
		{
			return Enumerable.Empty<string>();
		}
	}
}
