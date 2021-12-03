using System.Collections.Generic;
using System.Threading.Tasks;

namespace starsky.foundation.thumbnailgeneration.Interfaces
{
	public interface IThumbnailCleaner
	{
		void CleanAllUnusedFiles();
		Task<List<string>> CleanAllUnusedFilesAsync();

	}
}
