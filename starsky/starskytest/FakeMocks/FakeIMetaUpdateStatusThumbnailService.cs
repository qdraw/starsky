using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using starsky.foundation.metathumbnail.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIMetaUpdateStatusThumbnailService : IMetaUpdateStatusThumbnailService
{
	[SuppressMessage("Performance", "CA1822:Mark members as static")]
	public Task UpdateStatusThumbnail(List<(bool, string)> statusList)
	{
		return Task.CompletedTask;
	}
}
