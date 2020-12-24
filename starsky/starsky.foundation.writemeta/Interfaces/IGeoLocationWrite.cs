using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.foundation.writemeta.Interfaces
{
	public interface IGeoLocationWrite
	{
		void LoopFolder(List<FileIndexItem> metaFilesInDirectory,
			bool syncLocationNames);
	}
}
