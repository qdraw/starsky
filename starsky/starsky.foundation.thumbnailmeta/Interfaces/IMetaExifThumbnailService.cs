using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.metathumbnail.Interfaces
{
	public interface IMetaExifThumbnailService
	{
		/// <summary>
		/// File
		/// </summary>
		/// <param name="subPathsAndHash">(FilePath,FileHash)</param>
		/// <returns></returns>
		Task<bool> AddMetaThumbnail(IEnumerable<(string, string)> subPathsAndHash);
		Task<bool> AddMetaThumbnail(string subPath);
		Task<bool> AddMetaThumbnail(string subPath, string fileHash);
	}
}
