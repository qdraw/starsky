using MetadataExtractor.Formats.Exif;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailmeta.Interfaces;
using starsky.foundation.thumbnailmeta.Models;

namespace starskytest.FakeMocks
{
	public class FakeIOffsetDataMetaExifThumbnail : IOffsetDataMetaExifThumbnail
	{
		public OffsetModel ParseOffsetData(ExifThumbnailDirectory? exifThumbnailDir,
			string subPath)
		{
			return new OffsetModel { Success = true };
		}

		public (ExifThumbnailDirectory?, int, int, ImageRotation.Rotation) GetExifMetaDirectories(
			string subPath)
		{
			return ( null, 0, 0, ImageRotation.Rotation.Horizontal );
		}
	}
}
