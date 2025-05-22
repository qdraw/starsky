using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;

namespace starsky.foundation.metaupdate.Helpers;

public static class AddNotFoundInIndexStatus
{
	internal static void Update(IEnumerable<string> inputFilePaths,
		List<FileIndexItem> fileIndexResultsList)
	{
		foreach ( var subPath in inputFilePaths )
		{
			// when item is not in the database
			if ( fileIndexResultsList.Exists(p => p.FilePath == subPath) )
			{
				continue;
			}

			StatusCodesHelper.ReturnExifStatusError(new FileIndexItem(subPath),
				FileIndexItem.ExifStatus.NotFoundNotInIndex,
				fileIndexResultsList);
		}
	}
}
