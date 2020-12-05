using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Query
{
	public partial class Query 
	{
		public async Task<List<FileIndexItem>> GetObjectsByFilePathAsync(List<string> filePathList)
		{
			async Task<List<FileIndexItem>> LocalQuery(ApplicationDbContext context)
			{
				var predicates = new List<Expression<Func<FileIndexItem,bool>>>();  

				// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
				foreach ( var filePath in filePathList )
				{
					predicates.Add(x => x.FilePath.ToLower().Contains(filePath));
				}
				
				var predicate = PredicateBuilder.False<FileIndexItem>();
				
				for ( var i = 0; i < predicates.Count; i++ )
				{
					if ( i == 0 )
					{

						predicate = predicates[i];
					}
					else
					{
						var item = predicates[i - 1];
						var item2 = predicates[i];
						predicate =  item.Or(item2);
					}
				}

				return await context.FileIndex.Where(predicate).ToListAsync();
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
