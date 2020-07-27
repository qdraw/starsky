using System;
using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.feature.export.Interfaces
{
	public interface IExport
	{
		List<string> CreateListToExport(List<FileIndexItem> fileIndexResultsList, bool thumbnail);
		List<string> FilePathToFileName(List<string> filePaths, bool thumbnail);

		Tuple<string, List<FileIndexItem>> CreateZip(string[] inputFilePaths,
			bool collections = true,
			bool thumbnail = false);
	}
}
