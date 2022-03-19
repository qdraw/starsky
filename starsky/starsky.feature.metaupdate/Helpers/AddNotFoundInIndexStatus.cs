using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;

namespace starsky.feature.metaupdate.Helpers
{
	public static class AddNotFoundInIndexStatus
	{
		internal static void Update(string[] inputFilePaths, List<FileIndexItem> fileIndexResultsList)
		{
			foreach (var subPath in inputFilePaths)
			{
				// when item is not in the database
				if ( fileIndexResultsList.All(p => p.FilePath != subPath) )
				{
					StatusCodesHelper.ReturnExifStatusError(new FileIndexItem(subPath), 
						FileIndexItem.ExifStatus.NotFoundNotInIndex,
						fileIndexResultsList);
				}
			}
		}
	}
}
