using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;

namespace starsky.foundation.sync.SyncServices
{
	public class SyncRemove
	{
		private readonly AppSettings _appSettings;
		private readonly SetupDatabaseTypes _setupDatabaseTypes;
		private readonly IQuery _query;

		public SyncRemove(AppSettings appSettings, IQuery query)
		{
			_appSettings = appSettings;
			_setupDatabaseTypes = new SetupDatabaseTypes(appSettings);
			_query = query;
		}

		public SyncRemove(AppSettings appSettings, SetupDatabaseTypes setupDatabaseTypes,
			IQuery query)
		{
			_appSettings = appSettings;
			_setupDatabaseTypes = setupDatabaseTypes;
			_query = query;
		}

		/// <summary>
		/// remove path from database
		/// </summary>
		/// <param name="subPath">subPath</param>
		/// <returns></returns>
		public async Task<List<FileIndexItem>> Remove(string subPath)
		{
			return await Remove(new List<string> {subPath});
		}

		/// <summary>
		/// Remove list from database, Does not check if the file exist on disk
		/// </summary>
		/// <param name="subPaths">list of sub paths</param>
		/// <returns>file with status</returns>
		private async Task<List<FileIndexItem>> Remove(List<string> subPaths)
		{
			var toDeleteList = await _query.GetObjectsByFilePathAsync(subPaths);
			
			await toDeleteList
				.ForEachAsync(async item =>
				{
					var query = new QueryFactory(_setupDatabaseTypes, _query).Query();
					await query.RemoveItemAsync(item);
					item.Status = FileIndexItem.ExifStatus.NotFoundNotInIndex;
					return item;
				}, _appSettings.MaxDegreesOfParallelism);

			// Add items that are not in the database
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
