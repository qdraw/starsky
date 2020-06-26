using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.feature.update.Interfaces
{
	public interface IPreflightUpdate
	{
		List<FileIndexItem> Preflight(FileIndexItem inputModel, string[] inputFilePaths, bool collections);
	}
}
