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
		Task<IEnumerable<(bool,string)>> AddMetaThumbnail(IEnumerable<(string, string)> subPathsAndHash);
		Task<List<(bool,string)>> AddMetaThumbnail(string subPath);
		Task<(bool,string)> AddMetaThumbnail(string subPath, string fileHash);
	}
}
