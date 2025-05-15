using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query;

// QueryGetAllFiles
public partial class Query
{
	/// <summary>
	///     Get a list of all files inside an folder (NOT recursive)
	///     But this uses a database as source
	/// </summary>
	/// <param name="subPath">relative database path</param>
	/// <returns>list of FileIndex-objects</returns>
	public async Task<List<FileIndexItem>> GetAllFilesAsync(string subPath)
	{
		return await GetAllFilesAsync(new List<string> { subPath });
	}

	/// <summary>
	///     Get a list of all files inside an folder (NOT recursive)
	///     But this uses a database as source
	/// </summary>
	/// <param name="filePaths">relative database paths</param>
	/// <param name="timeout">when fail retry once after milliseconds</param>
	/// <returns>list of FileIndex-objects</returns>
	public async Task<List<FileIndexItem>> GetAllFilesAsync(List<string> filePaths,
		int timeout = 1000)
	{
		try
		{
			return FormatOk(await GetAllFilesQuery(_context, filePaths)
				.ToListAsync());
		}
		// InvalidOperationException can also be disposed
		catch ( InvalidOperationException invalidOperationException )
		{
			_logger.LogDebug($"[GetAllFilesAsync] catch-ed and retry after {timeout}",
				invalidOperationException);
			await Task.Delay(timeout);
			return FormatOk(
				await GetAllFilesQuery(new InjectServiceScope(_scopeFactory).Context(), filePaths)
					.ToListAsync());
		}
	}

	/// <summary>
	///     Get a list of all files inside an folder (NOT recursive)
	///     But this uses a database as source
	/// </summary>
	/// <param name="subPath">relative database path</param>
	/// <returns>list of FileIndex-objects</returns>
	public List<FileIndexItem> GetAllFiles(string subPath)
	{
		try
		{
			return GetAllFilesQuery(_context, new List<string> { subPath }).ToList()!;
		}
		catch ( ObjectDisposedException )
		{
			return GetAllFilesQuery(new InjectServiceScope(_scopeFactory).Context(),
				new List<string> { subPath }).ToList()!;
		}
	}

	internal static List<FileIndexItem> FormatOk(IReadOnlyCollection<FileIndexItem?>? input,
		FileIndexItem.ExifStatus fromStatus = FileIndexItem.ExifStatus.Default)
	{
		if ( input == null )
		{
			return new List<FileIndexItem>();
		}

		return input.Where(p => p != null).Select(p =>
		{
			// status check for some referenced based code
			if ( p!.Status == fromStatus )
			{
				p.Status = FileIndexItem.ExifStatus.Ok;
			}

			return p;
		}).ToList();
	}

	private static string RemoveLatestSlash(string filePath)
	{
		var subPath = PathHelper.RemoveLatestSlash(filePath);
		if ( filePath == "/" )
		{
			subPath = "/";
		}

		return subPath;
	}

	private static IOrderedQueryable<FileIndexItem?> GetAllFilesQuery(ApplicationDbContext context,
		List<string> filePathList)
	{
		var normalizedPaths = filePathList.Select(RemoveLatestSlash).ToList();

		return context.FileIndex
			.Where(p => p.IsDirectory == false &&
			            normalizedPaths.Contains(p.ParentDirectory ?? string.Empty))
			.OrderBy(r => r.FileName);
	}
}
