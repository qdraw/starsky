using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.metaupdate.Interfaces
{
	public interface IMetaReplaceService
	{
		Task<List<FileIndexItem>> Replace(string f, string fieldName, 
			string search, string replace, bool collections);
	}
}
