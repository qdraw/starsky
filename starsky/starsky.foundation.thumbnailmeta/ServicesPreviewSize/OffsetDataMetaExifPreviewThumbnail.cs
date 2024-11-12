using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.Models;
using starsky.foundation.thumbnailmeta.ServicesPreviewSize.Interfaces;
using Directory = MetadataExtractor.Directory;

namespace starsky.foundation.thumbnailmeta.ServicesPreviewSize;

public class OffsetDataMetaExifPreviewThumbnail : IOffsetDataMetaExifPreviewThumbnail
{
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;

	public OffsetDataMetaExifPreviewThumbnail(ISelectorStorage selectorStorage, IWebLogger logger)
	{
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_logger = logger;
	}

	public OffsetModel ParseOffsetData(List<Directory> allExifItems, string subPath)
	{
		var exifIfd0Directories = allExifItems.OfType<ExifIfd0Directory>();
		var exifIfd0DirectoriesTags = exifIfd0Directories.FirstOrDefault()?.Tags;
		if ( exifIfd0DirectoriesTags == null )
		{
			return new OffsetModel
			{
				Success = false,
				Reason =
					$"{FilenamesHelper.GetFileName(subPath)} ExifIfd0Directory null"
			};
		}

		var (offsetSuccess, offset, byteSizeSuccess, byteSize) =
			GetOffsetAndByteSize(exifIfd0DirectoriesTags);
		var thumbnail = new byte[byteSize];

		using ( var imageStream = _iStorage.ReadStream(subPath) )
		{
			imageStream.Seek(offset, SeekOrigin.Begin);

			var actualRead = imageStream.Read(thumbnail, 0, byteSize);
			if ( actualRead != byteSize )
			{
				_logger.LogError("[ParseOffsetData] ReadStream: actualRead != maxRead");
			}
		}

		return new OffsetModel
		{
			Index = offset,
			Count = byteSize,
			Success = offsetSuccess && byteSizeSuccess,
			Reason = offsetSuccess && byteSizeSuccess ? null : "offset or byteSize failed",
			Data = thumbnail
		};
	}

	public List<Directory> ReadExifMetaDirectory(string subPath)
	{
		using var stream = _iStorage.ReadStream(subPath);
		var allExifItems =
			ImageMetadataReader.ReadMetadata(stream).ToList();
		return allExifItems;
	}

	private static (bool offsetSuccess, int offset, bool byteSizeSuccess, int byteSize)
		GetOffsetAndByteSize(IReadOnlyList<Tag> exifIfd0Directories)
	{
		// you can get offset of thumbnail by JpegIFOffset(0x0201) Tag in IFD1,
		// size of thumbnail by JpegIFByteCount(0x0202) 
		// https://www.media.mit.edu/pia/Research/deepview/exif.html

		// Unknown tag (0x0201) - 135330
		var offsetAsString = exifIfd0Directories.FirstOrDefault(p => p.Type == 513)
			?.Description;
		var offsetSuccess = int.TryParse(offsetAsString, out var offset);

		// Unknown tag (0x0202) - 840155
		var byteSizeAsString = exifIfd0Directories.FirstOrDefault(p => p.Type == 514)
			?.Description;
		var byteSizeSuccess = int.TryParse(byteSizeAsString, out var byteSize);

		return ( offsetSuccess, offset, byteSizeSuccess, byteSize );
	}
}
