using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory;

public class CompositeThumbnailGenerator(List<IThumbnailGenerator> generators, IWebLogger logger)
	: IThumbnailGenerator
{
	public async Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(string singleSubPath,
		string fileHash, ThumbnailImageFormat imageFormat,
		List<ThumbnailSize> thumbnailSizes)
	{
		foreach ( var generator in generators )
		{
			try
			{
				var results =
					( await generator.GenerateThumbnail(singleSubPath, fileHash, imageFormat,
						thumbnailSizes) )
					.ToList();
				if ( results.All(p => p.Success) )
				{
					return results;
				}
			}
			catch ( Exception ex )
			{
				logger.LogError($"Generator {generator.GetType().Name} failed: {ex.Message}");
			}
		}

		return ErrorGenerationResultModel.FailedResult(thumbnailSizes, singleSubPath,
			string.Empty, true, "CompositeThumbnailGenerator failed");
	}
}
