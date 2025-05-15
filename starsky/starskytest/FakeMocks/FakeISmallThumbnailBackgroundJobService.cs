using System;
using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;

namespace starskytest.FakeMocks;

public class FakeISmallThumbnailBackgroundJobService : ISmallThumbnailBackgroundJobService

{
	public Task<bool> CreateJob(bool? isAuthenticated, string? filePath)
	{
		throw new NotImplementedException();
	}
}
