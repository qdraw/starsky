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
		public async Task<List<FileIndexItem>> GetAllObjectsAsync(string subPath)
		{
			return await GetAllObjectsAsync(new List<string> {subPath});
		}

		public async Task<List<FileIndexItem>> GetAllObjectsAsync(List<string> filePaths)
		{
			try
			{
				return FormatOk(await GetAllObjectsQuery(_context, filePaths).ToListAsync());
			}
			catch ( ObjectDisposedException )
			{
				return FormatOk(await GetAllObjectsQuery(new InjectServiceScope(_scopeFactory).Context(),filePaths).ToListAsync());
			}
		}
		
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
