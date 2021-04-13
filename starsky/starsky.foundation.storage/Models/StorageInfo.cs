using System;

namespace starsky.foundation.storage.Models
{
	public class StorageInfo
	{
		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile { get; set; }
		public long Size { get; set; }

		/// <summary>
		/// In UTC
		/// </summary>
		public DateTime LastWriteTime { get; set; }
	}
}
