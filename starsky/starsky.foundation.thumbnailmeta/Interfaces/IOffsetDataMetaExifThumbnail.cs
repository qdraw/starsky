using MetadataExtractor.Formats.Exif;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailmeta.Models;

namespace starsky.foundation.thumbnailmeta.Interfaces
{
	public interface IOffsetDataMetaExifThumbnail
	{
		OffsetModel ParseOffsetData(ExifThumbnailDirectory? exifThumbnailDir,
			string subPath);

		(ExifThumbnailDirectory?, int, int, FileIndexItem.Rotation) GetExifMetaDirectories(
			string subPath);
	}
}
