using System.IO;

namespace starsky.foundation.sync.WatcherInterfaces
{
	public interface IQueueProcessor
	{
		void QueueInput(string filepath, string toPath,
			WatcherChangeTypes changeTypes);
	}
}
