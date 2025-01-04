using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query;

public partial class Query
{
	internal List<FileIndexItem> QueryGetNextPrevInFolder(string parentFolderPath,
		string currentFolder)
	{
		List<FileIndexItem> LocalQuery(ApplicationDbContext context)
		{
			var items = context.FileIndex.Where(
				p => p.ParentDirectory == parentFolderPath
				     && p.ImageFormat != ExtensionRolesHelper.ImageFormat.meta_json
				     && p.ImageFormat != ExtensionRolesHelper.ImageFormat.xmp).ToList();

			var groupedItems = items
				.GroupBy(item => item.FileCollectionName)
				.Select(group =>
					group.OrderByDescending(p =>
						ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(p.FilePath)
					).First()
				)
				.ToList();

			// For the case when there is a folder named: test and a file test.jpg
			var itemWithSameNameInQueryResult = items.Find(p => p.FilePath == currentFolder);
			if ( itemWithSameNameInQueryResult != null &&
			     !groupedItems.Exists(p => p.FileName == itemWithSameNameInQueryResult.FileName) )
			{
				groupedItems.Add(itemWithSameNameInQueryResult);
			}

			// Make sure the order is correct
			return [.. groupedItems.OrderBy(p => p.FileName)];
		}

		try
		{
			return LocalQuery(_context);
		}
		catch ( MySqlProtocolException )
		{
			var context = new InjectServiceScope(_scopeFactory).Context();
			return LocalQuery(context);
		}
		catch ( ObjectDisposedException )
		{
			var context = new InjectServiceScope(_scopeFactory).Context();
			return LocalQuery(context);
		}
	}
}
