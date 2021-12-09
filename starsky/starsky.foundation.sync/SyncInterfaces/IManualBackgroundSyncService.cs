using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.foundation.sync.SyncInterfaces
{
	public interface IManualBackgroundSyncService
	{
		Task<FileIndexItem.ExifStatus> ManualSync(string subPath,
			string operationId = null);
	}
}
