using System;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.database.Import
{
	public class ImportQueryFactory
	{
		private readonly SetupDatabaseTypes _setupDatabaseTypes;
		private readonly IImportQuery _importQuery;
		private readonly IConsole _console;

		public ImportQueryFactory(SetupDatabaseTypes setupDatabaseTypes, IImportQuery importQuery, IConsole console)
		{
			_setupDatabaseTypes = setupDatabaseTypes;
			_importQuery = importQuery;
			_console = console;
		}
		
		public IImportQuery ImportQuery()
		{
			var context = _setupDatabaseTypes.BuilderDbFactory();
			if ( _importQuery.GetType() == typeof(ImportQuery) )
			{
				return new ImportQuery(null,_console,context);
			}
			return Activator.CreateInstance(_importQuery.GetType(), null,_console,context) as IImportQuery;
		}
	}
}
