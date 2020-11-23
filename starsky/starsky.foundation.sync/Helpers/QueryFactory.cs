using System.Threading.Tasks;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;

namespace starsky.foundation.sync.SyncServices
{
	public class QueryFactory
	{
		private readonly SetupDatabaseTypes _setupDatabaseTypes;
		private  readonly IQuery _query;

		public QueryFactory(SetupDatabaseTypes setupDatabaseTypes, IQuery query)
		{
			_setupDatabaseTypes = setupDatabaseTypes;
			_query = query;
		}
		
		public async Task<IQuery> Query()
		{
			await using var context =  _setupDatabaseTypes.BuilderDbFactory();
			return _query.Clone(context);
		}
	}
}
