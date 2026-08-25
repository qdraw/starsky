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
		try
		{
			return await QueryRecursiveAsync(_context, filePathList);
		}
		catch ( ObjectDisposedException )
		{
			var scope = new InjectServiceScope(_scopeFactory);
			return await scope.ExecuteAsync(context => QueryRecursiveAsync(context, filePathList));
		}
		catch ( InvalidOperationException )
		{
			var scope = new InjectServiceScope(_scopeFactory);
			return await scope.ExecuteAsync(context => QueryRecursiveAsync(context, filePathList));
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
			return await scope.ExecuteAsync(context => QueryRecursiveAsync(context, filePathList));
		}
	}

	private static async Task<List<FileIndexItem>> QueryRecursiveAsync(
		ApplicationDbContext context, List<string> filePathList)
	{
		var predicate = BuildRecursivePredicate(filePathList);

		return await context.FileIndex.AsNoTracking().Where(predicate)
			.OrderBy(r => r.FilePath).ToListAsync();
	}

	private static Expression<Func<FileIndexItem, bool>> BuildRecursivePredicate(
		List<string> filePathList)
	{
		var predicates = filePathList.Select(BuildRecursivePredicate).ToList();
		return PredicateBuilder.OrLoop(predicates);
	}

	private static Expression<Func<FileIndexItem, bool>> BuildRecursivePredicate(string filePath)
	{
		var prefix = BuildRecursivePrefix(filePath);

		if ( prefix == "/" )
		{
			return p => p.FilePath != null &&
			            p.FilePath != "/" &&
			            p.FilePath.StartsWith(prefix);
		}

		return p => p.FilePath != null && p.FilePath.StartsWith(prefix);
	}

	private static string BuildRecursivePrefix(string filePath)
	{
		var subPath = PathHelper.RemoveLatestSlash(filePath);
		return string.IsNullOrEmpty(subPath) ? "/" : $"{subPath}/";
	}
}
