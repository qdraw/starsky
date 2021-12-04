using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Query
{
	/// <summary>
	/// GetObjectsByFileHashAsync
	/// </summary>
	public partial class Query : IQuery
	{
		
		public async Task<List<FileIndexItem>> GetObjectsByFileHashAsync(List<string> fileHashesList)
		{
			async Task<List<FileIndexItem>> LocalQuery(ApplicationDbContext context)
			{
				var result = await context.
					FileIndex.TagWith("GetObjectsByFileHashAsync").Where(p =>
						fileHashesList.Contains(p.FileHash)).ToListAsync();
				foreach ( var fileHash in fileHashesList )
				{
					if ( result.FirstOrDefault(p => p.FileHash == fileHash) == null )
					{
						result.Add(new FileIndexItem(){FileHash = fileHash, 
							Status = FileIndexItem.ExifStatus.NotFoundNotInIndex});
					}
				}
				return FormatOk(result);
			}
			
			return await LocalQuery(_context);
		}
	}
}
