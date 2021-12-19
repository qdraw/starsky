using System.Collections.Generic;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using starsky.foundation.database.Models;
using starsky.foundation.metathumbnail.Interfaces;
using starsky.foundation.readmeta.Models;

namespace starskytest.FakeMocks
{
	public class FakeIOffsetDataMetaExifThumbnail : IOffsetDataMetaExifThumbnail
	{
		public OffsetModel ParseOffsetData(ExifThumbnailDirectory exifThumbnailDir,
			string subPath)
		{
			return new OffsetModel
			{
				Success = true
			};
		}

		public (List<Directory>, ExifThumbnailDirectory, int, int, FileIndexItem.Rotation) GetExifMetaDirectories(
			string subPath)
		{
			return (null, null, 0, 0, FileIndexItem.Rotation.Horizontal );
		}

		public OffsetModel ParseOffsetPreviewData(List<Directory> directory, string subPath)
		{
			throw new System.NotImplementedException();
		}
	}
}
