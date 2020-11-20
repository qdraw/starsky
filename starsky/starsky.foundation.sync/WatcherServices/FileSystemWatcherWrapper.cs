using System.IO;
using starsky.foundation.injection;
using starsky.foundation.sync.WatcherInterfaces;

namespace starsky.foundation.sync.WatcherServices
{
	[Service(typeof(IFileSystemWatcherWrapper), InjectionLifetime = InjectionLifetime.Transient)]
	public class FileSystemWatcherWrapper: FileSystemWatcher, IFileSystemWatcherWrapper
	{
		// no code its a wrapper around FileSystemWatcher
	}
}
