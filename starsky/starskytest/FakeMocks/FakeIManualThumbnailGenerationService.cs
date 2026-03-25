using System;
using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIManualThumbnailGenerationService : IManualThumbnailGenerationService
{
	public Task CreateJob(string subPath)
	{
		return Task.CompletedTask;
	}

	public Task WorkThumbnailGeneration(string subPath)
	{
		if ( subPath == "/" )
		{
			throw new InvalidOperationException();
		}
		return Task.CompletedTask;
	}
}
