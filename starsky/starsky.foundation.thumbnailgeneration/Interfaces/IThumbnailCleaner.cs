using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.thumbnailgeneration.Interfaces
{
	public interface IThumbnailCleaner
	{
		Task<List<string>> CleanAllUnusedFilesAsync(int chunkSize = 50);
	}
}
