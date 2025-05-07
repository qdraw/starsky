using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;

public interface IThumbnailService
{
	Task<List<GenerationResultModel>> GenerateThumbnail(string fileOrFolderPath,
		bool skipExtraLarge = false);

	Task<List<GenerationResultModel>> GenerateThumbnail(string subPath, string fileHash,
		bool skipExtraLarge = false);

	/// <summary>
	///     Get a single thumbnail
	/// </summary>
	/// <param name="subPath">only one file</param>
	/// <param name="fileHash">what is the hash?</param>
	/// <param name="size">what is the size</param>
	/// <returns></returns>
	Task<(Stream?, GenerationResultModel)> GenerateThumbnail(string subPath, string fileHash,
		ThumbnailImageFormat imageFormat,
		ThumbnailSize size);

	/// <summary>
	///     Rotate a thumbnail
	/// </summary>
	/// <param name="fileHash">fileHash to rename</param>
	/// <param name="orientation">which direction</param>
	/// <param name="width">height of output</param>
	/// <param name="height">0 = keep in shape</param>
	/// <returns></returns>
	Task<bool> RotateThumbnail(string fileHash, int orientation,
		int width = 1000, int height = 0);
}
