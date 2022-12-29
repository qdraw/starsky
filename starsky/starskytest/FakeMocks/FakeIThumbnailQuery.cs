using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;

namespace starskytest.FakeMocks;

public class FakeIThumbnailQuery : IThumbnailQuery
{
	public Task<List<ThumbnailItem>> AddThumbnailRangeAsync(ThumbnailSize size, IEnumerable<string> fileHashes,
		bool? setStatus = null)
	{
		return Task.FromResult(new List<ThumbnailItem>());
	}
}
