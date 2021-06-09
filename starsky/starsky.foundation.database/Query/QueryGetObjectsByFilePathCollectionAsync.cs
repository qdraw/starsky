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
	/// QueryGetObjectsByFilePathCollectionAsync
	/// </summary>
	public partial class Query : IQuery
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="subPath"></param>
		/// <returns></returns>
		internal async Task<List<FileIndexItem>> GetObjectsByFilePathCollectionAsync(string subPath)
		{
			return await GetObjectsByFilePathCollectionQueryAsync(new List<string> {subPath});
		}

		internal async Task<List<FileIndexItem>> GetObjectsByFilePathCollectionQueryAsync(List<string> filePathList)
		{
			try
			{
				return FormatOk(await GetObjectsByFilePathCollectionQuery(_context, filePathList).ToListAsync());
			}
			catch ( ObjectDisposedException )
			{
				return FormatOk(await GetObjectsByFilePathCollectionQuery(
					new InjectServiceScope(_scopeFactory).Context(),filePathList).ToListAsync());
			}
		}
		
		private IOrderedQueryable<FileIndexItem> GetObjectsByFilePathCollectionQuery(ApplicationDbContext context, 
			IEnumerable<string> filePathList)
		{
			var predicates = new List<Expression<Func<FileIndexItem,bool>>>();  

			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach ( var path in filePathList )
			{
				var fileNameWithoutExtension = FilenamesHelper.GetFileNameWithoutExtension(path);
				predicates.Add(p => p.ParentDirectory == FilenamesHelper.GetParentPath(path) 
				                    && p.FileName.StartsWith(fileNameWithoutExtension) );
			}
			
			var predicate = PredicateBuilder.OrLoop(predicates);

			return context.FileIndex.Where(predicate).OrderBy(r => r.FileName);
		}
	}
}
