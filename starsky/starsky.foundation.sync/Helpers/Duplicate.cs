using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;

namespace starsky.foundation.sync.Helpers
{
	public class Duplicate
	{
		private readonly IQuery _query;

		public Duplicate(IQuery query)
		{
			_query = query;
		}

		/// <summary>
		/// Check and remove duplicate 
		/// </summary>
		/// <param name="databaseSubFolderList"></param>
		/// <returns></returns>
		public async Task<List<FileIndexItem>> RemoveDuplicateAsync(List<FileIndexItem> databaseSubFolderList)
		{
			// Get a list of duplicate items
			var duplicateItemsByFilePath = databaseSubFolderList.GroupBy(item => item.FilePath)
				.SelectMany(grp => grp.Skip(1).Take(1)).ToList();
            
			// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
			foreach (var duplicateItemByName in duplicateItemsByFilePath)
			{
				var duplicateItems = databaseSubFolderList.Where(p => 
					p.FilePath == duplicateItemByName.FilePath).ToList();
				for (var i = 1; i < duplicateItems.Count; i++)
				{
					databaseSubFolderList.Remove(duplicateItems[i]);
					await _query.RemoveItemAsync(duplicateItems[i]);
				}
			}
			return databaseSubFolderList;
		}
	}
}
