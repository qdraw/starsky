using System.Collections.Generic;
using MetadataExtractor;
using starsky.foundation.thumbnailmeta.Models;

namespace starsky.foundation.thumbnailmeta.ServicesPreviewSize.Interfaces;

public interface IOffsetDataMetaExifPreviewThumbnail
{
	OffsetModel ParseOffsetData(List<Directory> allExifItems,
		string subPath);

	List<Directory> ReadExifMetaDirectory(string subPath);
}
