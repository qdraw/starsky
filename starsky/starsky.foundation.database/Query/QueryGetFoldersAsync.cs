using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query;

/// <summary>
///     QueryGetFoldersAsync
/// </summary>
public partial class Query : IQuery
{
	/// <summary>
	/// Get folders non-recursive, but this uses a database as source
	/// </summary>
	/// <param name="subPath">which path (subpath style)</param>
	/// <returns>items</returns>
	public async Task<List<FileIndexItem>> GetFoldersAsync(string subPath)
	{
		return await GetFoldersAsync(new List<string> { subPath });
	}

	/// <summary>
	/// Get folders non-recursive, but this uses a database as source
	/// </summary>
	/// <param name="filePaths">which paths (subpath style)</param>
	/// <returns>items</returns>
	public async Task<List<FileIndexItem>> GetFoldersAsync(List<string> filePaths)
	{
		try
		{
			return FormatOk(await GetAllFoldersQuery(_context, filePaths).ToListAsync());
		}
		catch ( ObjectDisposedException )
		{
			return FormatOk(
				await GetAllFoldersQuery(new InjectServiceScope(_scopeFactory).Context(), filePaths)
					.ToListAsync());
		}
	}

	private static IOrderedQueryable<FileIndexItem> GetAllFoldersQuery(ApplicationDbContext context,
		List<string> filePathList)
	{
		var predicates = new List<Expression<Func<FileIndexItem, bool>>>();

		// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
		foreach ( var filePath in filePathList )
		{
			var subPath = PathHelper.RemoveLatestSlash(filePath);
			if ( filePath == "/" )
			{
				subPath = "/";
			}

			predicates.Add(p => p.ParentDirectory == subPath && p.IsDirectory == true);
		}

		var predicate = PredicateBuilder.OrLoop(predicates);

		return context.FileIndex.Where(predicate).OrderBy(r => r.FileName);
	}
}
