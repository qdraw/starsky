using System;

namespace starsky.foundation.storage.Models
{
	public class StorageInfo
	{
		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile { get; set; }
		public long Size { get; set; }

		/// <summary>
		/// Input in UTC, witten down local
		/// </summary>
		public DateTime LastWriteTime { get; set; }
	}
}
