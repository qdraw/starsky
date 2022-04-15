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
		private readonly IWebLogger _logger;

		public ImportQueryFactory(SetupDatabaseTypes setupDatabaseTypes, IImportQuery importQuery, IConsole console, IWebLogger logger)
		{
			_setupDatabaseTypes = setupDatabaseTypes;
			_importQuery = importQuery;
			_console = console;
			_logger = logger;
		}
		
		public IImportQuery ImportQuery()
		{
			var context = _setupDatabaseTypes.BuilderDbFactory();
			if ( _importQuery.GetType() == typeof(ImportQuery) )
			{
				return new ImportQuery(null,_console,_logger,context);
			}
			return Activator.CreateInstance(_importQuery.GetType(), null,_console,_logger, context) as IImportQuery;
		}
	}
}
