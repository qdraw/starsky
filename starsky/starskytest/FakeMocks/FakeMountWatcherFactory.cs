using System;
using System.Collections.Generic;
using starsky.foundation.mountwatch.Interfaces;
using starsky.foundation.mountwatch.Services;

#pragma warning disable 0067

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

	public sealed class FakeMountWatcher : IMountWatcher
	{
		private readonly Exception? _exceptionToThrow;

		public FakeMountWatcher(Exception? exceptionToThrow = null)
		{
			_exceptionToThrow = exceptionToThrow;
		}
		public event EventHandler<MountDetectedEventArgs>? MountDetected;

		public void Start()
		{
			if ( _exceptionToThrow != null )
			{
				throw _exceptionToThrow;
			}
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
