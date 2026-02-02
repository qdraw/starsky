using System;
using System.Collections.Generic;
using System.Linq;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;

namespace starsky.feature.rename.Helpers;

public class FileItemsQueryHelpers(IQuery query, IWebLogger logger)
{
	internal Dictionary<string, FileIndexItem> FileItemsQuery<T>(List<string> filePaths,
		bool collections, List<T> mappings) where T : IFileItemQuery, new()
	{
		var fileItems = new Dictionary<string, FileIndexItem>();

		foreach ( var filePath in filePaths )
		{
			try
			{
				var detailView = query.SingleItem(filePath,
					null, collections, false);
				if ( detailView?.FileIndexItem == null )
				{
					mappings.Add(new T
					{
						SourceFilePath = filePath,
						HasError = true,
						ErrorMessage = "File not found in database"
					});
					continue;
				}

				fileItems[filePath] = detailView.FileIndexItem;
				if ( !collections )
				{
					continue;
				}

				foreach ( var collectionPath in detailView.FileIndexItem!.CollectionPaths.Where(p =>
					         p != filePath) )
				{
					if ( fileItems.ContainsKey(collectionPath) )
					{
						// when explicit adding files skip
						continue;
					}

					// implicit flow
					var collectionFileIndexItem = query.SingleItem(collectionPath,
						null, false, false)!.FileIndexItem;
					if ( collectionFileIndexItem != null )
					{
						fileItems[collectionPath] = collectionFileIndexItem;
					}
				}
			}
			catch ( Exception )
			{
				logger.LogError($"PreviewBatchRename: Failed to get item for {filePath}");
			}
		}

		return fileItems;
	}
}
