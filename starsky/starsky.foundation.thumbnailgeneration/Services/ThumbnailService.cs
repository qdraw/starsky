using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Helpers;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.Services;

[Service(typeof(IThumbnailService), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class ThumbnailService : IThumbnailService
{
	private readonly Thumbnail _thumbnail;
	private readonly IUpdateStatusGeneratedThumbnailService _updateStatusGeneratedThumbnailService;

	public ThumbnailService(ISelectorStorage selectorStorage, IWebLogger logger,
		AppSettings appSettings,
		IUpdateStatusGeneratedThumbnailService updateStatusGeneratedThumbnailService)
	{
		_updateStatusGeneratedThumbnailService = updateStatusGeneratedThumbnailService;
		var iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		var thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_thumbnail = new Thumbnail(iStorage, thumbnailStorage, logger, appSettings);
	}

	/// <summary>
	///     Create a thumbnail for a file or folder
	///     and update index!
	/// </summary>
	/// <param name="subPath">location on disk</param>
	/// <returns>object with status</returns>
	public async Task<List<GenerationResultModel>> CreateThumbnailAsync(string subPath)
	{
		var generationResults = await _thumbnail.CreateThumbnailAsync(subPath);

		// new ThumbnailVideo(_thumbnail.Storage, _thumbnail.Logger, _videoProcess)
		// 	.CreateThumbnailAsync(subPath);

		await _updateStatusGeneratedThumbnailService.AddOrUpdateStatusAsync(generationResults);
		return generationResults;
	}

	/// <summary>
	///     Create for 1 image multiple thumbnails based on the default sizes
	/// </summary>
	/// <param name="subPath">path on disk (subPath) based</param>
	/// <param name="fileHash">output name</param>
	/// <param name="skipExtraLarge">skip xl</param>
	/// <returns>true if success</returns>
	public Task<IEnumerable<GenerationResultModel>> CreateThumbAsync(string? subPath,
		string fileHash, bool skipExtraLarge = false)
	{
		return _thumbnail.CreateThumbAsync(subPath, fileHash);
	}

	/// <summary>
	///     Rotate a thumbnail
	/// </summary>
	/// <param name="fileHash">fileHash to rename</param>
	/// <param name="orientation">which direction</param>
	/// <param name="width">height of output</param>
	/// <param name="height">0 = keep in shape</param>
	/// <returns></returns>
	public Task<bool> RotateThumbnail(string fileHash, int orientation,
		int width = 1000, int height = 0)
	{
		return _thumbnail.RotateThumbnail(fileHash, orientation, width, height);
	}
}
