using System;
using System.Threading.Tasks;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Query;

namespace starsky.foundation.sync.Helpers
{
	public class QueryFactory
	{
		private readonly SetupDatabaseTypes _setupDatabaseTypes;
		private readonly IQuery _query;

		public QueryFactory(SetupDatabaseTypes setupDatabaseTypes, IQuery query)
		{
			_setupDatabaseTypes = setupDatabaseTypes;
			_query = query;
		}
		
		public IQuery Query()
		{
			var context = _setupDatabaseTypes.BuilderDbFactory();
			if ( _query.GetType() == typeof(Query) )
			{
				return new Query(context);
			}
			return Activator.CreateInstance(_query.GetType(), context, null, null, null) as IQuery;
		}

		// public async Task<IQuery> Query(Type type)
		// {
		// 	await using var context =  _setupDatabaseTypes.BuilderDbFactory();
		// 	return Activator.CreateInstance(type, context, null, null, null) as IQuery;
		// }
	}
}
