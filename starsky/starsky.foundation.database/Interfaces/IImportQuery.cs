using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Interfaces
{
	public interface IImportQuery
	{
		Task<bool> IsHashInImportDbAsync(string fileHashCode);
		bool TestConnection();
		Task<bool> AddAsync(ImportIndexItem updateStatusContent);
		List<ImportIndexItem> History();
	}
}
