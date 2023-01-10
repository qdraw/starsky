using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.Services;

[Service(typeof(IUpdateStatusGeneratedThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public class UpdateStatusGeneratedThumbnailService : IUpdateStatusGeneratedThumbnailService
{
	private readonly IThumbnailQuery _thumbnailQuery;

	public UpdateStatusGeneratedThumbnailService(IThumbnailQuery thumbnailQuery)
	{
		_thumbnailQuery = thumbnailQuery;
	}

	public async Task UpdateStatusAsync(List<GenerationResultModel> generationResults)
	{
		// in the next step only the fileHash is included
		var dtoObjects = generationResults
			.Where(p => !p.IsNotFound)
			.DistinctBy(p => p.FileHash)
			.Select(p => p.FileHash)
			.Select(fileHash => new ThumbnailResultDataTransferModel(fileHash)).ToList();

		foreach ( var generationResult in generationResults.Where(p => !p.IsNotFound) )
		{
			var index = dtoObjects.FindIndex(p => p.FileHash == generationResult.FileHash);
			if ( generationResult.Size == ThumbnailSize.Unknown || index == -1 )
			{
				continue;
			}
			dtoObjects[index].Change(generationResult.Size, generationResult.Success);
			dtoObjects[index].Reasons = generationResult.ErrorMessage;
		}

		await _thumbnailQuery.AddThumbnailRangeAsync(dtoObjects);
	}
}
