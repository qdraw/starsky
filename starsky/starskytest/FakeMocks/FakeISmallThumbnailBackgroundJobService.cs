using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;

namespace starskytest.FakeMocks;

public class FakeISmallThumbnailBackgroundJobService : ISmallThumbnailBackgroundJobService

{
	public HashSet<string> FilePaths { get; set; } = [];

	public Task<bool> CreateJob(bool? isAuthenticated, string? filePath)
	{
		FilePaths.Add(filePath ?? string.Empty);
		return Task.FromResult(true);
	}
}
