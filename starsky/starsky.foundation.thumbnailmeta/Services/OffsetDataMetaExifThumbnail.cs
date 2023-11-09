using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.thumbnailmeta.Interfaces;
using starsky.foundation.thumbnailmeta.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.readmeta.Models;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using Directory = MetadataExtractor.Directory;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.thumbnailmeta.Services
{
	[Service(typeof(IOffsetDataMetaExifThumbnail), InjectionLifetime = InjectionLifetime.Scoped)]
	public sealed class OffsetDataMetaExifThumbnail : IOffsetDataMetaExifThumbnail
	{
		private readonly IStorage _iStorage;
		private readonly IWebLogger _logger;

		public OffsetDataMetaExifThumbnail(ISelectorStorage selectorStorage, IWebLogger logger)
		{
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_logger = logger;
		}

		public (ExifThumbnailDirectory?, int, int, FileIndexItem.Rotation)
			GetExifMetaDirectories(string subPath)
		{
			var (allExifItems,exifThumbnailDir) = ReadExifMetaDirectories(subPath);
			return ParseMetaThumbnail(allExifItems, exifThumbnailDir, subPath);
		}
		
		internal (List<Directory>?, ExifThumbnailDirectory?) ReadExifMetaDirectories(string subPath)
		{
			using ( var stream = _iStorage.ReadStream(subPath) )
			{
				var allExifItems =
					ImageMetadataReader.ReadMetadata(stream).ToList();
				var exifThumbnailDir =
					allExifItems.Find(p =>
						p.Name == "Exif Thumbnail") as ExifThumbnailDirectory;

				return ( allExifItems,  exifThumbnailDir);
			}
		}
		
		internal (ExifThumbnailDirectory?, int, int, FileIndexItem.Rotation) ParseMetaThumbnail(List<Directory>? allExifItems, 
			ExifThumbnailDirectory? exifThumbnailDir, string? reference = null)
		{

			if ( exifThumbnailDir == null || allExifItems == null )
			{
				return ( null, 0, 0, FileIndexItem.Rotation.DoNotChange );
			}
				
			var jpegTags = allExifItems.OfType<JpegDirectory>().FirstOrDefault()?.Tags;

			var heightPixels = jpegTags?.FirstOrDefault(p => p.Type == JpegDirectory.TagImageHeight)?.Description;
			var widthPixels = jpegTags?.FirstOrDefault(p => p.Type == JpegDirectory.TagImageWidth)?.Description;

			if ( string.IsNullOrEmpty(heightPixels) && string.IsNullOrEmpty(widthPixels) )
			{
				var exifSubIfdDirectories = allExifItems.OfType<ExifSubIfdDirectory>().ToList();
				foreach ( var exifSubIfdDirectoryTags in exifSubIfdDirectories.Select(p => p.Tags) )
				{
					var heightValue =  exifSubIfdDirectoryTags.FirstOrDefault(p => p.Type == ExifDirectoryBase.TagImageHeight)?.Description;
					var widthValue =  exifSubIfdDirectoryTags.FirstOrDefault(p => p.Type == ExifDirectoryBase.TagImageWidth)?.Description;
					if ( heightValue == null || widthValue == null ) continue;
					heightPixels = heightValue;
					widthPixels = widthValue;
				}
			}

			var rotation = ReadMetaExif.GetOrientationFromExifItem(allExifItems);
					
			var heightParseResult = int.TryParse(heightPixels?.Replace(
				" pixels",string.Empty), out var height);
				
			var widthParseResult = int.TryParse(widthPixels?.Replace(" pixels",string.Empty), out var width);
			
			if ( !heightParseResult || !widthParseResult || height == 0||  width == 0)
			{
				_logger.LogInformation($"[ParseMetaThumbnail] ${reference} has no height or width {width}x{height} ");
			}
			return (exifThumbnailDir, width, height, rotation);
		}

		public OffsetModel ParseOffsetData(ExifThumbnailDirectory? exifThumbnailDir, string subPath)
		{
			if ( exifThumbnailDir == null )  return new OffsetModel
			{
				Success = false, 
				Reason = $"{FilenamesHelper.GetFileName(subPath)} ExifThumbnailDirectory null"
			};

			long thumbnailOffset = long.Parse(exifThumbnailDir!.GetDescription(
				ExifThumbnailDirectory.TagThumbnailOffset)!.Split(' ')[0]);
			const int maxIssue35Offset = 12;
			int thumbnailLength = int.Parse(exifThumbnailDir.GetDescription(
				ExifThumbnailDirectory.TagThumbnailLength)!.Split(' ')[0]) + maxIssue35Offset;
			byte[] thumbnail = new byte[thumbnailLength];
			
			using (var imageStream = _iStorage.ReadStream(subPath))
			{
				imageStream.Seek(thumbnailOffset, SeekOrigin.Begin);
				imageStream.Read(thumbnail, 0, thumbnailLength);
			}
			
			// work around Metadata Extractor issue #35
			if ( thumbnailLength <= maxIssue35Offset + 1 )
			{
				_logger.LogInformation($"[ParseOffsetData] thumbnailLength : {thumbnailLength} {maxIssue35Offset + 1}");
				return new OffsetModel {Success = false, Reason =  $"{FilenamesHelper.GetFileName(subPath)} offsetLength"};
			}
			
			int issue35Offset = 0;
			for (int offset = 0; offset <= maxIssue35Offset; ++offset)
			{
				// 0xffd8 is the JFIF start of image segment indicator
				if ((thumbnail[offset] == 0xff) && (thumbnail[offset + 1] == 0xd8))
				{
					issue35Offset = offset;
					break;
				}
			}
			return new OffsetModel
			{
				Success = true,
				Index = issue35Offset,
				Count = thumbnailLength - issue35Offset,
				Data = thumbnail, // byte array
			};

		}
	}

}
