using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Generators.Interfaces;

public interface IThumbnailGenerator
{
	/// <summary>
	///     This is per thumbnail; please use ThumbnailService
	/// </summary>
	/// <param name="singleSubPath"></param>
	/// <param name="fileHash"></param>
	/// <param name="thumbnailSizes"></param>
	/// <returns></returns>
	Task<IEnumerable<GenerationResultModel>> GenerateThumbnail(string singleSubPath,
		string fileHash,
		ThumbnailImageFormat imageFormat,
		List<ThumbnailSize> thumbnailSizes);
}
