using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.metaupdate.Interfaces
{
	public interface IMetaInfo
	{
		Task<List<FileIndexItem>> GetInfoAsync(List<string> inputFilePaths,
			bool collections);
	}
}
