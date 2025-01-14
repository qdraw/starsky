using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query;

/// <summary>
///     QueryExistsAsync
/// </summary>
public partial class Query : IQuery
{
	public async Task<bool> ExistsAsync(string filePath)
	{
		if ( filePath != "/" )
		{
			filePath = PathHelper.RemoveLatestSlash(filePath);
		}

		async Task<bool> LocalQuery(ApplicationDbContext context)
		{
			return await context.FileIndex.AnyAsync(p => p.FilePath == filePath);
		}

		try
		{
			return await LocalQuery(_context);
		}
		catch ( ObjectDisposedException e )
		{
			_logger.LogInformation("[ExistsAsync] catch-ed ObjectDisposedException", e);
			return await LocalQuery(new InjectServiceScope(_scopeFactory).Context());
		}
	}
}
