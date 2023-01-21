using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.thumbnailgeneration.Models;

namespace starsky.foundation.thumbnailgeneration.Interfaces
{
	public interface IThumbnailService
	{
		/// <summary>
		/// Recursive by default
		/// </summary>
		/// <param name="subPath">folder to scan</param>
		/// <returns>list of items with success</returns>
		Task<List<GenerationResultModel>> CreateThumbnailAsync(string subPath);

		/// <summary>
		/// Single item with fileHash
		/// </summary>
		/// <param name="subPath">location</param>
		/// <param name="fileHash">fileHash</param>
		/// <param name="skipExtraLarge"></param>
		/// <returns></returns>
		Task<IEnumerable<GenerationResultModel>> CreateThumbAsync(string subPath, string fileHash, bool skipExtraLarge = false);

		/// <summary>
		/// Rotate a thumbnail
		/// </summary>
		/// <param name="fileHash">fileHash to rename</param>
		/// <param name="orientation">which direction</param>
		/// <param name="width">height of output</param>
		/// <param name="height">0 = keep in shape</param>
		/// <returns></returns>
		Task<bool> RotateThumbnail(string fileHash, int orientation,
			int width = 1000, int height = 0);
	}
}
