using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.thumbnailgeneration.Interfaces
{
	public interface IThumbnailService
	{
		Task<List<(string, bool)>> CreateThumb(string subPath);
		Task<bool> CreateThumb(string subPath, string fileHash);
	}
}
