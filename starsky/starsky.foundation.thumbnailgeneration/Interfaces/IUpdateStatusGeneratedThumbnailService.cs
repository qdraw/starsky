using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.Interfaces;

public interface IUpdateStatusGeneratedThumbnailService
{
	/// <summary>
	/// Ignores the not found items to update
	/// </summary>
	/// <param name="generationResults">items</param>
	/// <returns>updated data transfer list</returns>
	Task<List<ThumbnailResultDataTransferModel>> UpdateStatusAsync(
		List<GenerationResultModel> generationResults);

	/// <summary>
	/// Remove items with status not found
	/// </summary>
	/// <param name="generationResults">items</param>
	/// <returns>remove by fileHash</returns>
	Task<List<string>> RemoveNotfoundStatusAsync(
		List<GenerationResultModel> generationResults);
}
