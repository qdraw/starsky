using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
		[SuppressMessage("Sonar", "CS0067:The event 'FakeMountWatcherFactory." +
		                          "FakeMountWatcher.MountDetected' is never used")]
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
