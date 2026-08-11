using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query;

public partial class Query // QueryCollections
{
	internal static List<FileIndexItem> StackCollections(List<FileIndexItem> databaseSubFolderList)
	{
		// Get a list of duplicate items
		var stackItemsByFileCollectionName = databaseSubFolderList
			.GroupBy(item => item.FileCollectionName)
			.SelectMany(grp => grp.Skip(1).Take(1)).ToList();
		// databaseSubFolderList.ToList() > Collection was modified; enumeration operation may not execute.

		// duplicateItemsByFilePath > 
		// If you have 3 item with the same name it will include 1 name
		// So we do a linq query to search simalar items
		// We keep the first item
		// And Delete duplicate items

		var querySubFolderList = new List<FileIndexItem>();
		// Do not remove it from: databaseSubFolderList otherwise it will be deleted from cache

		foreach ( var stackItemByName in stackItemsByFileCollectionName )
		{
			var duplicateItems = databaseSubFolderList.Where(p =>
				p.FileCollectionName == stackItemByName.FileCollectionName).ToList();

			// Pick thumbnail based images first; fall back to first alphabetically so that
			// collections with no thumbnail-supported member (e.g. ARW+DNG) are not dropped.
			var thumbnailItems = duplicateItems
				.Where(item =>
					ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(item.FileName))
				.ToList();

			if ( thumbnailItems.Count != 0 )
			{
				querySubFolderList.AddRange(thumbnailItems);
			}
			else
			{
				var fallback = duplicateItems.MinBy(item => item.FileName);
				if ( fallback != null )
				{
					querySubFolderList.Add(fallback);
				}
			}
		}

		return AddNonDuplicateBackToList(databaseSubFolderList, stackItemsByFileCollectionName,
			querySubFolderList);
	}

	[SuppressMessage("Usage", "S3267:Loops should be simplified with LINQ expressions")]
	[SuppressMessage("Performance",
		"CA1859:Use concrete types when possible for improved performance")]
	private static List<FileIndexItem> AddNonDuplicateBackToList(
		IEnumerable<FileIndexItem> databaseSubFolderList,
		IReadOnlyCollection<FileIndexItem> stackItemsByFileCollectionName,
		ICollection<FileIndexItem> querySubFolderList)
	{
		// Then add the items that are non duplicate back to the list
		foreach ( var dbItem in databaseSubFolderList.ToList() )
		{
			// check if any item is duplicate
			if ( stackItemsByFileCollectionName.All(p =>
				    p.FileCollectionName != dbItem.FileCollectionName) )
			{
				querySubFolderList.Add(dbItem);
			}
		}

		return querySubFolderList.OrderBy(p => p.FileName).ToList();
	}
}
