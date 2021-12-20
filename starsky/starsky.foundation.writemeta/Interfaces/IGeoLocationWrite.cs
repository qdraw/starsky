using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.foundation.writemeta.Interfaces
{
	public interface IGeoLocationWrite
	{
		Task LoopFolderAsync(List<FileIndexItem> metaFilesInDirectory,
			bool syncLocationNames);
	}
}
