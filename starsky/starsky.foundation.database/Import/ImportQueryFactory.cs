using System;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;

namespace starsky.foundation.database.Import
{
	public class ImportQueryFactory
	{
		private readonly SetupDatabaseTypes _setupDatabaseTypes;
		private readonly IImportQuery _importQuery;

		public ImportQueryFactory(SetupDatabaseTypes setupDatabaseTypes, IImportQuery importQuery)
		{
			_setupDatabaseTypes = setupDatabaseTypes;
			_importQuery = importQuery;
		}
		
		public IImportQuery ImportQuery()
		{
			var context = _setupDatabaseTypes.BuilderDbFactory();
			if ( _importQuery.GetType() == typeof(ImportQuery) )
			{
				return new ImportQuery(null,context);
			}
			return Activator.CreateInstance(_importQuery.GetType(), null,context) as IImportQuery;
		}
	}
}
