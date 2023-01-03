using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
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
		foreach ( var size in new List<ThumbnailSize>
		         { ThumbnailSize.Large, 
			       ThumbnailSize.Small, 
			       ThumbnailSize.ExtraLarge } )
		{
			var largeThumbnailsSuccess = generationResults.Where(p => p.Success && p.Size == size);
			await _thumbnailQuery.AddThumbnailRangeAsync(ThumbnailSize.Large, largeThumbnailsSuccess.Select(p => p.FileHash), true);

			var largeThumbnailsFail = generationResults.Where(p => !p.Success && p.Size == size);
			await _thumbnailQuery.AddThumbnailRangeAsync(ThumbnailSize.Large, largeThumbnailsFail.Select(p => p.FileHash), false);
		}

	}
}
