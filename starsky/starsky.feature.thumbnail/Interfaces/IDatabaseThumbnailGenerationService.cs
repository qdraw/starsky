using System.Threading.Tasks;

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
	Task ExecuteQueuedJobAsync();
}
