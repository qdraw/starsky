using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.metaupdate.Interfaces
{
	public interface IDeleteItem
	{
		Task<List<FileIndexItem>> DeleteAsync(string f, bool collections);
	}
}
