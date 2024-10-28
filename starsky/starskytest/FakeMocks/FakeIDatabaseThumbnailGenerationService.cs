using System.Threading.Tasks;
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
}
