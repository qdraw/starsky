using starsky.foundation.injection;
using starsky.foundation.worker.Services;

namespace starsky.foundation.sync.WatcherBackgroundService
{
	[Service(typeof(DiskWatcherBackgroundTaskQueue), InjectionLifetime = InjectionLifetime.Singleton)]
	public class DiskWatcherBackgroundTaskQueue : BackgroundTaskQueue
	{
		// is emthy
	}
}
