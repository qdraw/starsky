using System;

namespace starsky.foundation.storage.Models
{
	public sealed class StorageInfo
	{
		public FolderOrFileModel.FolderOrFileTypeList IsFolderOrFile { get; set; }
		
		/// <summary>
		/// Size in bytes
		/// </summary>
		public long Size { get; set; }

		/// <summary>
		/// Input in UTC, witten down local
		/// </summary>
		public DateTime LastWriteTime { get; set; }
	}
}
