using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.Interfaces;

public interface IUpdateStatusGeneratedThumbnailService
{
	Task UpdateStatusAsync(List<GenerationResultModel> generationResults);
}
