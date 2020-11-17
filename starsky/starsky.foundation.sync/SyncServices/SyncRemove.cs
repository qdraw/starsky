using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;

namespace starsky.foundation.sync.SyncServices
{
	public class SyncRemove
	{
		private readonly AppSettings _appSettings;
		private readonly SetupDatabaseTypes _setupDatabaseTypes;
		private readonly IQuery _query;

		public SyncRemove(AppSettings appSettings, IServiceScopeFactory serviceScopeFactory, IQuery query)
		{
			_appSettings = appSettings;
			_setupDatabaseTypes = new SetupDatabaseTypes(appSettings,
				serviceScopeFactory.CreateScope().ServiceProvider.GetService<IServiceCollection>());
			_query = query;
		}

		public async Task<List<FileIndexItem>> Remove(string subPath)
		{
			return await Remove(new List<string> {subPath});
		}

		private async Task<List<FileIndexItem>> Remove(List<string> subPaths)
		{
			var toDeleteList = new List<FileIndexItem>();

			async Task<IQuery> LocalQuery()
			{
				await using var context =  _setupDatabaseTypes.BuilderDbFactory();
				return _query.Clone(context);
			}
			
			await subPaths
				.ForEachAsync(
					async subPath =>
					{
						var query = await LocalQuery();

						var directItem = await query.GetObjectByFilePathAsync(subPath);
						if ( directItem != null )
						{
							toDeleteList.Add(directItem);
						}
						
						var item = await query.GetAllRecursiveAsync(subPath);
						toDeleteList.AddRange(item);
						return item;
					},
					_appSettings.MaxDegreesOfParallelism);

			await toDeleteList
				.ForEachAsync(async item =>
				{
					var query = await LocalQuery();
					await query.RemoveItemAsync(item);
					item.Status = FileIndexItem.ExifStatus.NotFoundNotInIndex;
					return item;
				}, _appSettings.MaxDegreesOfParallelism);

			foreach ( var subPath in subPaths.Where(subPath => 
				!toDeleteList.Exists(p => p.FilePath == subPath)) )
			{
				toDeleteList.Add(new FileIndexItem(subPath)
				{
					Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
				});
			}
			return toDeleteList;
		}
	}
}
