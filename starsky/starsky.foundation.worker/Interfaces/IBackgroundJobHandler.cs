using System.Threading;
using System.Threading.Tasks;

namespace starsky.foundation.worker.Interfaces;

public interface IBackgroundJobHandler
{
	string JobType { get; }
	Task ExecuteAsync(string? payloadJson, CancellationToken cancellationToken);
}

