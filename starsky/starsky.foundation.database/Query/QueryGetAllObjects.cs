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

namespace starsky.foundation.database.Query
{
	/// <summary>
	/// QueryGetAllObjects
	/// </summary>
	public partial class Query : IQuery
	{
		/// <summary>
		/// Get all objects inside a folder
		/// </summary>
		/// <param name="subPath"></param>
		/// <returns></returns>
		public async Task<List<FileIndexItem>> GetAllObjectsAsync(string subPath)
		{
			return await GetAllObjectsAsync(new List<string> {subPath});
		}

		/// <summary>
		/// Get all objects inside a folder
		/// </summary>
		/// <param name="filePaths">parent paths</param>
		/// <returns>list of all objects inside the folder</returns>
		public async Task<List<FileIndexItem>> GetAllObjectsAsync(List<string> filePaths)
		{
			if ( !filePaths.Any() ) return new List<FileIndexItem>();

			async Task<List<FileIndexItem>> LocalGetAllObjectsAsync()
			{
				var dbContext = new InjectServiceScope(_scopeFactory).Context();
				var result =
					FormatOk(await GetAllObjectsQuery(dbContext, filePaths)
						.ToListAsync());
				await dbContext.DisposeAsync();
				return result;
			}
			
			try
			{
				return FormatOk(await GetAllObjectsQuery(_context, filePaths)
					.ToListAsync());
			}
			catch ( ObjectDisposedException )
			{
				return await LocalGetAllObjectsAsync();
			}
			catch ( InvalidOperationException )
			{
				// System.InvalidOperationException: ExecuteReader can only be called when the connection is open.
				return await LocalGetAllObjectsAsync();
			}
		}
		
		/// <summary>
		/// QueryFolder Async without cache
		/// </summary>
		/// <param name="context">database context</param>
		/// <param name="filePathList">list of paths</param>
		/// <returns></returns>
		private IOrderedQueryable<FileIndexItem> GetAllObjectsQuery(ApplicationDbContext context, List<string> filePathList)
		{
			var predicates = new List<Expression<Func<FileIndexItem,bool>>>();  

			// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
			foreach ( var filePath in filePathList )
			{
				var subPath = PathHelper.RemoveLatestSlash(filePath);
				if ( filePath == "/" ) subPath = "/";
				predicates.Add(p => p.ParentDirectory == subPath);
			}
				
			var predicate = PredicateBuilder.OrLoop(predicates);
			
			return context.FileIndex.Where(predicate).OrderBy(r => r.FileName);
		}
	}
}
