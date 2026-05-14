using System;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.database.Import;

public class ImportQueryFactory(
	SetupDatabaseTypes setupDatabaseTypes,
	IImportQuery importQuery,
	IConsole console,
	IWebLogger logger)
{
	public IImportQuery? ImportQuery()
	{
		var context = setupDatabaseTypes.BuilderDbFactory();
		if ( importQuery is ImportQuery )
		{
			return new ImportQuery(null, console, logger, context);
		}

		return Activator.CreateInstance(importQuery.GetType(), null, console, logger, context) as
			IImportQuery;
	}
}
