using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.export.Interfaces
{
	public interface IExport
	{
		Task CreateZip(List<FileIndexItem> fileIndexResultsList, bool thumbnail,
			string zipOutputFileName);

		Task<Tuple<string, List<FileIndexItem>>> PreflightAsync(
			string[] inputFilePaths,
			bool collections = true,
			bool thumbnail = false);

		Tuple<bool?, string> StatusIsReady(string zipOutputFileName);
	}
}
