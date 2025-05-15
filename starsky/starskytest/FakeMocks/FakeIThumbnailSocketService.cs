using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.thumbnail.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starskytest.FakeMocks;

public class FakeIThumbnailSocketService : IThumbnailSocketService
{
	public Dictionary<string, List<GenerationResultModel>> Results { get; set; } = new();

	public Task NotificationSocketUpdate(string subPath,
		List<GenerationResultModel> generateThumbnailResults)
	{
		Results.Add(subPath, generateThumbnailResults);
		return Task.CompletedTask;
	}
}
