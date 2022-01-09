using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.thumbnailgeneration.Interfaces
{
	public interface IThumbnailService
	{
		/// <summary>
		/// Recursive by default
		/// </summary>
		/// <param name="subPath">folder to scan</param>
		/// <returns>list of items with success</returns>
		Task<List<(string, bool)>> CreateThumb(string subPath);
		
		/// <summary>
		/// Single item with fileHash
		/// </summary>
		/// <param name="subPath">location</param>
		/// <param name="fileHash">fileHash</param>
		/// <returns></returns>
		Task<bool> CreateThumb(string subPath, string fileHash);
	}
}
