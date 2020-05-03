using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
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
		/// Add Item Sync
		/// </summary>
		/// <param name="updateStatusContent"></param>
		/// <returns></returns>
		public override async Task<FileIndexItem> AddItemAsync(FileIndexItem updateStatusContent)
		{
			// ReSharper disable once MethodHasAsyncOverload
			return _query.AddItem(updateStatusContent);
		}
	}
}
#pragma warning restore 1998

