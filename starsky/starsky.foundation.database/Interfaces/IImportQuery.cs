using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Interfaces
{
	public interface IImportQuery
	{
		Task<bool> IsHashInImportDbAsync(string fileHashCode);
		bool TestConnection();
		Task<bool> AddAsync(ImportIndexItem updateStatusContent, bool writeConsole = true);
		List<ImportIndexItem> History();
		Task<List<ImportIndexItem>> AddRangeAsync(List<ImportIndexItem> importIndexItemList);
		Task<ImportIndexItem> RemoveItemAsync(ImportIndexItem importIndexItem, int maxAttemptCount = 3);
	}
}
