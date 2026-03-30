using System.Threading;
using System.Threading.Tasks;

namespace starsky.foundation.import.Interfaces;

public interface IMountWatcherLogWriter
{
	Task WriteAsync(string eventName, object payload, CancellationToken cancellationToken = default);
}

