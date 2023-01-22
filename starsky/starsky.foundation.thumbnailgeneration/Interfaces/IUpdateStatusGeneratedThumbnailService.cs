using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.Interfaces;

public interface IUpdateStatusGeneratedThumbnailService
{
	Task<List<ThumbnailResultDataTransferModel>> UpdateStatusAsync(
		List<GenerationResultModel> generationResults);
}
