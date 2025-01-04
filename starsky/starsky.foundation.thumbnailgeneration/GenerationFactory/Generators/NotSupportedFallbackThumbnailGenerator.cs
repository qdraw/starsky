using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;

public class NotSupportedFallbackThumbnailGenerator : IThumbnailGenerator
{
	public Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(string singleSubPath,
		string fileHash,
		List<ThumbnailSize> thumbnailSizes)
	{
		return Task.FromResult(new List<GenerationResultModel>().AsEnumerable());
	}
}
