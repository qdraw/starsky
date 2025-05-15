using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starskytest.FakeMocks;

public class FakeIThumbnailSocketService : IThumbnailSocketService
{
	public Task NotificationSocketUpdate(string subPath,
		List<GenerationResultModel> generateThumbnailResults)
	{
		throw new NotImplementedException();
	}
}
