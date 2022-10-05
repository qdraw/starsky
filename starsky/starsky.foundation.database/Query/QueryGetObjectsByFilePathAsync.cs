using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query
{
	public partial class Query : IQuery
	{
		/// <summary>
		/// Query
		/// </summary>
		/// <param name="inputFilePaths">list of paths</param>
		/// <param name="collections">uses collections </param>
		/// <returns>list with items</returns>
		public async Task<List<FileIndexItem>> GetObjectsByFilePathAsync(List<string> inputFilePaths, bool collections)
		{
			var resultFileIndexItemsList = new List<FileIndexItem>();
			var toQueryPaths = new List<string>();
			foreach ( var path in  inputFilePaths)
			{
				var parentPath = FilenamesHelper.GetParentPath(path);
				
				var (success, cachedResult) = CacheGetParentFolder(parentPath);

				List<FileIndexItem> item = null;
				switch ( collections )
				{
					case false:
						if ( !success ) break;
						item = cachedResult.Where(p =>
							p.ParentDirectory == parentPath && 
							p.FileName == FilenamesHelper.GetFileName(path)).ToList();
						break;
					case true:
						if ( !success ) break;
						item = cachedResult.Where(p =>
							p.ParentDirectory == parentPath && 
							p.FileCollectionName == FilenamesHelper.GetFileNameWithoutExtension(path)).ToList();
						break;
				}

				if ( !success || !item.Any())
				{
					toQueryPaths.Add(path);
					continue;
				}
				resultFileIndexItemsList.AddRange(item);
			}
			var fileIndexItemsList = await GetObjectsByFilePathQuery(toQueryPaths.ToArray(), collections);
			resultFileIndexItemsList.AddRange(fileIndexItemsList);
			return resultFileIndexItemsList;
		}
		
		/// <summary>
		/// Switch between collections and non-collections
		/// </summary>
		/// <param name="inputFilePaths">list of paths</param>
		/// <param name="collections">[when true] hide raws or everything with the same name (without extension)</param>
		/// <returns></returns>
		private async Task<List<FileIndexItem>> GetObjectsByFilePathQuery(string[] inputFilePaths, bool collections)
		{
			if ( collections )
			{
				return await GetObjectsByFilePathCollectionQueryAsync(inputFilePaths.ToList());
			}
			return await GetObjectsByFilePathQueryAsync(inputFilePaths.ToList());
		}
		
		
		/// <summary>
		/// Skip cache
		/// </summary>
		/// <param name="filePathList"></param>
		/// <returns></returns>
		public async Task<List<FileIndexItem>> GetObjectsByFilePathQueryAsync(List<string> filePathList)
		{
			async Task<List<FileIndexItem>> LocalQuery(ApplicationDbContext context)
			{
				var result = await context.FileIndex.TagWith("GetObjectsByFilePathQueryAsync").Where(p =>
					filePathList.Contains(p.FilePath)).ToListAsync();
				return FormatOk(result);
			}

			try
			{
				return await LocalQuery(_context);
			}
			catch ( NullReferenceException ex1)
			{
				_logger.LogInformation($"catch-ed null ref exception: {string.Join( ",", filePathList.ToArray() )} {ex1.StackTrace}", ex1);
				await Task.Delay(10);
				// System.NullReferenceException: Object reference not set to an instance of an object.
				// at MySql.Data.MySqlClient.MySqlDataReader.ActivateResultSet()
				try
				{
					return await LocalQuery(new InjectServiceScope(_scopeFactory)
							.Context());
				}
				catch ( MySqlProtocolException )
				{
					// Packet received out-of-order. Expected 1; got 2.
					return await LocalQuery(
						new InjectServiceScope(_scopeFactory).Context());
				}
				catch ( NullReferenceException ex2)
				{
					_logger.LogInformation($"catch-ed null ref exception 2: {string.Join( ",", filePathList.ToArray() )} {ex2.StackTrace}", ex2);
					throw;
				}
			}
			catch ( InvalidOperationException )
			{
				// System.InvalidOperationException or ObjectDisposedException: Cannot Open when State is Connecting.
				return await LocalQuery(new InjectServiceScope(_scopeFactory).Context());
			}
		}
	}
}
