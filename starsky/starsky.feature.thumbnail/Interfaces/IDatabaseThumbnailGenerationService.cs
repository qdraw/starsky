using System.Threading.Tasks;
using System.Threading;

namespace starsky.feature.thumbnail.Interfaces;

public interface IDatabaseThumbnailGenerationService
{
	/// <summary>
	///     Start the job
	/// </summary>
	/// <returns>create job</returns>
	Task StartBackgroundQueue();

	/// <summary>
	///     Execute job
	/// </summary>
	/// <returns></returns>
	Task ExecuteQueuedJobAsync(CancellationToken cancellationToken);
}
