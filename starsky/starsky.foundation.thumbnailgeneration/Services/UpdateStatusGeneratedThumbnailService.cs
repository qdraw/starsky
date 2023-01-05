using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Enums;
using starsky.foundation.storage.Storage;
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
		foreach ( var size in ThumbnailNameHelper.GeneratedThumbnailSizes )
		{
			var thumbnailsSuccess = generationResults.Where(p => p.Success && p.Size == size);
			await _thumbnailQuery.AddThumbnailRangeAsync(new List<ThumbnailSize>{size}, 
				thumbnailsSuccess.Select(p => p.FileHash).ToList(), true);

			var thumbnailsFail = generationResults.Where(p => !p.Success && p.Size == size);
			await _thumbnailQuery.AddThumbnailRangeAsync(new List<ThumbnailSize>{size}, 
				thumbnailsFail.Select(p => p.FileHash).ToList(), false);
		}
	}
}
