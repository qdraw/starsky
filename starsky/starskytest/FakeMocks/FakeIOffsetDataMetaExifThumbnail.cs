using MetadataExtractor.Formats.Exif;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailmeta.Models;
using starsky.foundation.thumbnailmeta.ServicesTinySize.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIOffsetDataMetaExifThumbnail : IOffsetDataMetaExifThumbnail
{
	public OffsetModel ParseOffsetData(ExifThumbnailDirectory? exifThumbnailDir,
		string subPath)
	{
		return new OffsetModel { Success = true };
	}

	public (ExifThumbnailDirectory?, int, int, RotationModel.Rotation) GetExifMetaDirectories(
		string subPath)
	{
		return ( null, 0, 0, RotationModel.Rotation.Horizontal );
	}
}
