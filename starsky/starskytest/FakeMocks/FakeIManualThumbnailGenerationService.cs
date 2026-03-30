using System;
using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIManualThumbnailGenerationService : IManualThumbnailGenerationService
{
	public bool WasCreateJobCalled;
	public string? LastSubPath;

	public Task CreateJob(string subPath)
	{
		WasCreateJobCalled = true;
		LastSubPath = subPath;
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
