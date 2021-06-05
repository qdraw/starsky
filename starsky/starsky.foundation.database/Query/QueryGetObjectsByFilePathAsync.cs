using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query
{
	public partial class Query 
	{
		/// <summary>
		/// Uses Cache
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
				var fileName = FilenamesHelper.GetFileName(path);
				var (success, result) = CacheGetParentFolder(parentPath);
				var item = result.FirstOrDefault(p =>
					p.ParentDirectory == parentPath && p.FileName == fileName);
				
				if ( !success || item == null)
				{
					toQueryPaths.Add(path);
					continue;
				}
				resultFileIndexItemsList.Add(item);
			}
			var fileIndexItemsList = await GetObjectsByFilePathQuery(toQueryPaths.ToArray(), collections);
			resultFileIndexItemsList.AddRange(fileIndexItemsList);
			return resultFileIndexItemsList;
		}
		
		private async Task<List<FileIndexItem>> GetObjectsByFilePathQuery(string[] inputFilePaths, bool collections)
		{
			if ( collections )
			{
				return await GetObjectsByFilePathCollectionAsync(inputFilePaths.ToList());
			}
			return await GetObjectsByFilePathAsync(inputFilePaths.ToList());
		}
		
		
		/// <summary>
		/// Skip cache
		/// </summary>
		/// <param name="filePathList"></param>
		/// <returns></returns>
		internal async Task<List<FileIndexItem>> GetObjectsByFilePathAsync(List<string> filePathList)
		{
			async Task<List<FileIndexItem>> LocalQuery(ApplicationDbContext context)
			{
				return FormatOk(await context.FileIndex.Where(p => filePathList.Contains(p.FilePath)).ToListAsync());
			}

			try
			{
				return await LocalQuery(_context);
			}
			catch (ObjectDisposedException)
			{
				return await LocalQuery(new InjectServiceScope(_scopeFactory).Context());
			}
		}
	}
}
