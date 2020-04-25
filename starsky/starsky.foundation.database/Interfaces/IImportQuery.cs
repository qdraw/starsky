using System.Threading.Tasks;

namespace starsky.foundation.database.Interfaces
{
	public interface IImportQuery
	{
		Task<bool> IsHashInImportDb(string fileHashCode);
	}
}
