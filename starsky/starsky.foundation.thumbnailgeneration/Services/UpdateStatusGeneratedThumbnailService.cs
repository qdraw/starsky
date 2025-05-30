using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.Services;

[Service(typeof(IUpdateStatusGeneratedThumbnailService),
	InjectionLifetime = InjectionLifetime.Scoped)]
public class UpdateStatusGeneratedThumbnailService : IUpdateStatusGeneratedThumbnailService
{
	private readonly IThumbnailQuery _thumbnailQuery;

	public UpdateStatusGeneratedThumbnailService(IThumbnailQuery thumbnailQuery)
	{
		_thumbnailQuery = thumbnailQuery;
	}

	/// <summary>
	///     Ignores the not found items to update
	/// </summary>
	/// <param name="generationResults">items</param>
	/// <returns>updated data transfer list</returns>
	public async Task<List<ThumbnailResultDataTransferModel>> AddOrUpdateStatusAsync(
		List<GenerationResultModel> generationResults)
	{
		// in the next step only the fileHash is included
		var dtoObjects = generationResults
			.Where(p => !p.IsNotFound)
			.DistinctBy(p => p.FileHash)
			.Where(p => !string.IsNullOrWhiteSpace(p.FileHash))
			.Select(p => p.FileHash)
			.Select(fileHash => new ThumbnailResultDataTransferModel(fileHash)).ToList();

		// add the other sizes
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
		return dtoObjects;
	}

	/// <summary>
	///     Remove items with status not found
	/// </summary>
	/// <param name="generationResults">items</param>
	/// <returns>remove by fileHash</returns>
	public async Task<List<string>> RemoveNotfoundStatusAsync(
		List<GenerationResultModel> generationResults)
	{
		// in the next step only the fileHash is included
		var dtoObjects = generationResults
			.Where(p => p.IsNotFound) // for not found items
			.DistinctBy(p => p.FileHash)
			.Select(p => p.FileHash)
			.ToList();

		await _thumbnailQuery.RemoveThumbnailsAsync(dtoObjects);
		return dtoObjects;
	}
}
