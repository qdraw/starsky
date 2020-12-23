using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Query
{
	public partial class Query 
	{
		public async Task<List<FileIndexItem>> GetObjectsByFilePathAsync(List<string> filePathList)
		{
			async Task<List<FileIndexItem>> LocalQuery(ApplicationDbContext context)
			{
				return await context.FileIndex.Where(p => filePathList.Contains(p.FilePath)).ToListAsync();
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
