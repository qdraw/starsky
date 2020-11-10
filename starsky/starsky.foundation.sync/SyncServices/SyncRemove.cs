using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;

namespace starsky.foundation.sync.SyncServices
{
	public class SyncRemove
	{
		private IQuery _query;

		public SyncRemove(IQuery query)
		{
			_query = query;
		}

		public async Task Remove(string subPath)
		{
			_query.RemoveItem()
		}
	}
}
