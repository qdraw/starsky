using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query;

/// <summary>
///     QueryGetObjectsByFileHashAsync and GetObjectsByFileHashAsync
/// </summary>
public partial class Query
{
	public async Task<List<FileIndexItem>> GetObjectsByFileHashAsync(List<string> fileHashesList,
		int retryCount = 2)
	{
		if ( fileHashesList.Count == 0 )
		{
			return [];
		}

		async Task<List<FileIndexItem>> LocalQuery(ApplicationDbContext context)
		{
			var result = await context.FileIndex
				.TagWith("GetObjectsByFileHashAsync").Where(p =>
					fileHashesList.Contains(p.FileHash!)).ToListAsync();
			var nonNullFileHashes = fileHashesList.Where(fileHash =>
				result.Find(p => p.FileHash == fileHash) == null);
			var toAddRange = nonNullFileHashes.Select(fileHash =>
				new FileIndexItem
				{
					FileHash = fileHash, Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
				});
			result.AddRange(toAddRange);

			return FormatOk(result);
		}

		return await RetryHelper.DoAsync(LocalDefaultQuery, TimeSpan.FromSeconds(3), retryCount);

		async Task<List<FileIndexItem>> LocalDefaultQuery()
		{
			try
			{
				return await LocalQuery(_context);
			}
			// InvalidOperationException can also be disposed
			catch ( InvalidOperationException )
			{
				if ( _scopeFactory == null )
				{
					throw;
				}

				return await LocalQuery(new InjectServiceScope(_scopeFactory).Context());
			}
		}
	}
}
