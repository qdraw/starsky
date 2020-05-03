using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
#pragma warning disable 1998

namespace starsky.foundation.database.Query
{
	public class QueryNetFramework : Query
	{
		private readonly Query _query;

		public QueryNetFramework(ApplicationDbContext context, IMemoryCache memoryCache = null, 
			AppSettings appSettings = null, IServiceScopeFactory scopeFactory = null) : 
			base(context, memoryCache, appSettings, scopeFactory)
		{
			using ( var scope = scopeFactory.CreateScope() )
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				_query = new Query(dbContext,memoryCache,appSettings,scopeFactory);
			}
		}

		/// <summary>
		/// Add Range Item Sync
		/// </summary>
		/// <param name="fileIndexItem"></param>
		/// <returns></returns>
		public override async Task<FileIndexItem> AddItemAsync(FileIndexItem fileIndexItem)
		{
			// ReSharper disable once MethodHasAsyncOverload
			_query.AddItem(fileIndexItem);
			return fileIndexItem;
		}

		/// <summary>
		/// Add Range Item Sync
		/// </summary>
		/// <param name="fileIndexItemList"></param>
		/// <returns></returns>
		public override async Task<List<FileIndexItem>> AddRangeAsync(List<FileIndexItem> fileIndexItemList)
		{
			// ReSharper disable once MethodHasAsyncOverload
			_query.AddRange(fileIndexItemList);
			return fileIndexItemList;
		}
	}
}

