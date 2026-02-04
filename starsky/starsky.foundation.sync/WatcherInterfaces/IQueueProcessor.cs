using System.IO;
using System.Threading.Tasks;

namespace starsky.foundation.sync.WatcherInterfaces
{
	public interface IQueueProcessor
	{
		Task QueueInput(string filepath, string? toPath,
			WatcherChangeTypes changeTypes);
	}
}
