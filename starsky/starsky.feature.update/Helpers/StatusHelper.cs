using System.Collections.Generic;
using starsky.foundation.database.Models;

namespace starsky.feature.update.Helpers
{
	public class StatusHelper
	{
		/// <summary>
		/// Does decide if the loop should be stopped, true = stop
		/// Uses FileCollectionsCheck
		/// Add for all types exept for OK/Readonly!
		/// </summary>
		/// <param name="statusModel">the main object to return later</param>
		/// <param name="statusResults">the status by FileCollectionsCheck</param>
		/// <param name="fileIndexResultsList">list of object that will be returned</param>
		/// <returns>If true skip the next code</returns>
		public bool ReturnExifStatusError(FileIndexItem statusModel, 
			FileIndexItem.ExifStatus statusResults, List<FileIndexItem> fileIndexResultsList)
		{
			switch (statusResults)
			{
				case FileIndexItem.ExifStatus.DirReadOnly:
					statusModel.IsDirectory = true;
					statusModel.Status = FileIndexItem.ExifStatus.DirReadOnly;
					fileIndexResultsList.Add(statusModel);
					return true;
				case FileIndexItem.ExifStatus.NotFoundNotInIndex:
					statusModel.Status = FileIndexItem.ExifStatus.NotFoundNotInIndex;
					fileIndexResultsList.Add(statusModel);
					return true;
				case FileIndexItem.ExifStatus.NotFoundSourceMissing:
					statusModel.Status = FileIndexItem.ExifStatus.NotFoundSourceMissing;
					fileIndexResultsList.Add(statusModel);
					return true;
			}
			return false;
		}
	}
}
