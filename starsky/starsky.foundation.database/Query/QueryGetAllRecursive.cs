using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

// QueryGetAllRecursiveAsync	
namespace starsky.foundation.database.Query;

public partial class Query
{
	/// <summary>
	///     Includes sub items in file
	///     Used for Orphan Check
	///     All files in
	/// </summary>
	/// <param name="subPath">local path</param>
	/// <returns>results</returns>
	public async Task<List<FileIndexItem>> GetAllRecursiveAsync(string subPath = "/")
	{
		return await GetAllRecursiveAsync([subPath]);
	}

	/// <summary>
	///     Includes sub Items
	/// </summary>
	/// <param name="filePathList">list of paths</param>
	/// <returns>items from database</returns>
	public async Task<List<FileIndexItem>> GetAllRecursiveAsync(List<string> filePathList)
	{
		async Task<List<FileIndexItem>> LocalQuery(ApplicationDbContext context)
		{
			var predicates = new List<Expression<Func<FileIndexItem, bool>>>();
			var recursivePrefixes = filePathList
				.Select(filePath =>
				{
					var subPath = PathHelper.RemoveLatestSlash(filePath);
					return string.IsNullOrEmpty(subPath) ? "/" : $"{subPath}/";
				})
				.Distinct()
				.ToList();

			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach ( var recursivePrefix in recursivePrefixes )
			{
				var prefix = recursivePrefix;
				if ( prefix == "/" )
				{
					predicates.Add(p => p.FilePath != null &&
					                 p.FilePath != "/" &&
					                 p.FilePath.StartsWith(prefix));
					continue;
				}

				predicates.Add(p => p.FilePath != null && p.FilePath.StartsWith(prefix));
			}

			var predicate = PredicateBuilder.OrLoop(predicates);

			return await context.FileIndex.AsNoTracking().Where(predicate)
				.OrderBy(r => r.FilePath).ToListAsync();
		}

		try
		{
			return await LocalQuery(_context);
		}
		catch ( ObjectDisposedException )
		{
			var scope = new InjectServiceScope(_scopeFactory);
			return await scope.ExecuteAsync(LocalQuery);
		}
		catch ( InvalidOperationException )
		{
			var scope = new InjectServiceScope(_scopeFactory);
			return await scope.ExecuteAsync(LocalQuery);
		}
		catch ( MySqlException exception )
		{
			// https://github.com/qdraw/starsky/issues/1243
			// https://github.com/qdraw/starsky/issues/1628
			if ( exception.ErrorCode is not (MySqlErrorCode.QueryTimeout or
			    MySqlErrorCode.LockWaitTimeout or
			    MySqlErrorCode.QueryInterrupted) )
			{
				_logger.LogError(
					$"[GetAllRecursiveAsync] MySqlException ErrorCode: {exception.ErrorCode}");
				throw;
			}

			_logger.LogInformation($"[GetAllRecursiveAsync] Next Retry Timeout/interrupted " +
			                       $"{exception.ErrorCode} in GetAllRecursiveAsync");

			await Task.Delay(1000);
			var scope = new InjectServiceScope(_scopeFactory);
			return await scope.ExecuteAsync(LocalQuery);
		}
	}
}
