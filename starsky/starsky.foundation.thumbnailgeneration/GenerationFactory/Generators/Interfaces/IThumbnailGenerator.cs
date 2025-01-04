using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;

public interface IThumbnailGenerator
{
	Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(string singleSubPath,
		string fileHash,
		List<ThumbnailSize> thumbnailSizes);
}
