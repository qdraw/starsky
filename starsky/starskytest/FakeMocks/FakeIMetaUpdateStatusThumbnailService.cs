#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using starsky.foundation.thumbnailmeta.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIMetaUpdateStatusThumbnailService : IMetaUpdateStatusThumbnailService
{
	[SuppressMessage("Performance", "CA1822:Mark members as static")]
	public Task UpdateStatusThumbnail(List<(bool, bool, string, string?)> statusResultsWithSubPaths)
	{ 
		return Task.CompletedTask;
	}
}
