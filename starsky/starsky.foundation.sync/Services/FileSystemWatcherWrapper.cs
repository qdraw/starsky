using System.IO;
using starsky.foundation.injection;
using starsky.foundation.sync.Interfaces;

namespace starsky.foundation.sync.Services
{
	[Service(typeof(IFileSystemWatcherWrapper), InjectionLifetime = InjectionLifetime.Transient)]
	public class FileSystemWatcherWrapper: FileSystemWatcher, IFileSystemWatcherWrapper
	{
		// no code its a wrapper around FileSystemWatcher
	}
}
