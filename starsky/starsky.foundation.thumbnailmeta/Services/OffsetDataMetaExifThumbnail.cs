using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Exif.Makernotes;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.metathumbnail.Interfaces;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.readmeta.Models;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using Directory = MetadataExtractor.Directory;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.metathumbnail.Services
{
	[Service(typeof(IOffsetDataMetaExifThumbnail), InjectionLifetime = InjectionLifetime.Scoped)]
	public class OffsetDataMetaExifThumbnail : IOffsetDataMetaExifThumbnail
	{
		private readonly IStorage _iStorage;
		private readonly IWebLogger _logger;

		public OffsetDataMetaExifThumbnail(ISelectorStorage selectorStorage, IWebLogger logger)
		{
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_logger = logger;
		}

		public (List<Directory>, ExifThumbnailDirectory, int, int, FileIndexItem.Rotation)
			GetExifMetaDirectories(string subPath)
		{
			var (allExifItems,exifThumbnailDir) = ReadExifMetaDirectories(subPath);
			return ParseMetaThumbnail(allExifItems, exifThumbnailDir, subPath);
		}

		public OffsetModel ParseOffsetPreviewData(List<Directory> allExifItems, string subPath)
		{
			var sonyMakerNote =
				allExifItems.FirstOrDefault(p =>
					p.Name == "Sony Makernote") as SonyType1MakernoteDirectory;
			if ( sonyMakerNote == null ) return new OffsetModel{Success = false};

			var tags = sonyMakerNote.Tags.FirstOrDefault(p => p.Name.Contains("0x0088"));
			
		
			
			Console.WriteLine(sonyMakerNote);
			
			throw new System.NotImplementedException();
		}

		internal (List<Directory>, ExifThumbnailDirectory) ReadExifMetaDirectories(string subPath)
		{
			using ( var stream = _iStorage.ReadStream(subPath) )
			{
				var allExifItems =
					ImageMetadataReader.ReadMetadata(stream).ToList();
				var exifThumbnailDir =
					allExifItems.FirstOrDefault(p =>
						p.Name == "Exif Thumbnail") as ExifThumbnailDirectory;

				return ( allExifItems,  exifThumbnailDir);
			}
		}
		
		internal (List<Directory>, ExifThumbnailDirectory, int, int, FileIndexItem.Rotation) ParseMetaThumbnail(List<Directory> allExifItems, 
			ExifThumbnailDirectory exifThumbnailDir, string reference = null)
		{

			if ( exifThumbnailDir == null )
			{
				return (null, null, 0, 0, FileIndexItem.Rotation.DoNotChange );
			}
				
			var jpegTags = allExifItems.FirstOrDefault(p =>
				p.Name == "JPEG")?.Tags;

			var rotation = new ReadMetaExif(null).GetOrientationFromExifItem(
				allExifItems.FirstOrDefault(p => p.Name == "Exif IFD0"));
					
			int.TryParse(
				jpegTags?.FirstOrDefault(p => p.Name == "Image Height")?
					.Description.Replace(" pixels",string.Empty), out var height);
				
			int.TryParse(
				jpegTags?.FirstOrDefault(p => p.Name == "Image Width")?
					.Description.Replace(" pixels",string.Empty), out var width);
				
			if ( height == 0||  width == 0)
			{
				_logger.LogInformation($"[] ${reference} has no height or width {width}x{height} ");
			}
			return (allExifItems, exifThumbnailDir, width, height, rotation);
		}

		public OffsetModel ParseOffsetData(ExifThumbnailDirectory exifThumbnailDir, string subPath)
		{
			if ( exifThumbnailDir == null )  return new OffsetModel {Success = false, Reason = "ExifThumbnailDirectory null"};

			long thumbnailOffset = long.Parse(exifThumbnailDir.GetDescription(
				ExifThumbnailDirectory.TagThumbnailOffset).Split(' ')[0]);
			const int maxIssue35Offset = 12;
			int thumbnailLength = int.Parse(exifThumbnailDir.GetDescription(
				ExifThumbnailDirectory.TagThumbnailLength).Split(' ')[0]) + maxIssue35Offset;
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
				return new OffsetModel {Success = false, Reason = "offsetLength"};
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
