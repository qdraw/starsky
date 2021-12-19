using System.Collections.Generic;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using starsky.foundation.database.Models;
using starsky.foundation.readmeta.Models;

namespace starsky.foundation.metathumbnail.Interfaces
{
	public interface IOffsetDataMetaExifThumbnail
	{
		OffsetModel ParseOffsetData(ExifThumbnailDirectory exifThumbnailDir,
			string subPath);
	
		(List<Directory>, ExifThumbnailDirectory, int, int, FileIndexItem.Rotation) GetExifMetaDirectories(string subPath);
		OffsetModel ParseOffsetPreviewData(List<Directory> directory, string subPath);
	}
}
