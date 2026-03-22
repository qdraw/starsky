using System.Threading.Tasks;
using System.Threading;
using starsky.feature.thumbnail.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIDatabaseThumbnailGenerationService : IDatabaseThumbnailGenerationService
{
	public int Count { get; set; }

	public Task StartBackgroundQueue()
	{
		Count++;
		return Task.CompletedTask;
	}

	public Task ExecuteQueuedJobAsync(CancellationToken cancellationToken)
	{
		throw new System.NotImplementedException();
	}
}
