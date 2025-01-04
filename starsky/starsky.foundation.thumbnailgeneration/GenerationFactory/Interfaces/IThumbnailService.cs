using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;

public interface IThumbnailService
{
	Task<List<GenerationResultModel>> GenerateThumbnail(string fileOrFolderPath,
		bool skipExtraLarge = false);

	Task<List<GenerationResultModel>> GenerateThumbnail(string subPath, string fileHash,
		bool skipExtraLarge = false);

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
