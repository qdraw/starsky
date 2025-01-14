using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.import.Interfaces;

public interface IImportThumbnailService
{
	IEnumerable<ThumbnailResultDataTransferModel> MapToTransferObject(
		List<string> thumbnailNames);

	List<string> GetThumbnailNamesWithSuffix(List<string> tempImportPaths);

	Task<bool> WriteThumbnails(List<string> tempImportPaths,
		List<string> thumbnailNames);
}
