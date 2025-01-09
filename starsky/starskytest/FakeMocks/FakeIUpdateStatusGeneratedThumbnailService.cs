using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;
using starsky.foundation.thumbnailgeneration.Services;

namespace starskytest.FakeMocks;

public class FakeIUpdateStatusGeneratedThumbnailService : IUpdateStatusGeneratedThumbnailService
{
	public async Task<List<ThumbnailResultDataTransferModel>> AddOrUpdateStatusAsync(
		List<GenerationResultModel> generationResults)
	{
		return await new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery())
			.AddOrUpdateStatusAsync(generationResults);
	}

	public async Task<List<string>> RemoveNotfoundStatusAsync(
		List<GenerationResultModel> generationResults)
	{
		return await new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery())
			.RemoveNotfoundStatusAsync(generationResults);
	}
}
