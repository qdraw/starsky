using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.readmeta.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using Directory = MetadataExtractor.Directory;

namespace starsky.foundation.readmeta.MetaThumbnail
{
	public class ReadMetaThumbnail
	{
		private readonly IStorage _iStorage;
		private readonly IWebLogger _logger;

		public ReadMetaThumbnail(ISelectorStorage selectorStorage, IWebLogger logger)
		{
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_logger = logger;
		}

		public void ReadExifFromFile2(string subPath)
		{

			ExifThumbnailDirectory exifThumbnailDir = null;
			using ( var stream = _iStorage.ReadStream(subPath) )
			{
				var allExifItems =
					ImageMetadataReader.ReadMetadata(stream).ToList();
				exifThumbnailDir =
					allExifItems.FirstOrDefault(p =>
						p.Name == "Exif Thumbnail") as ExifThumbnailDirectory;
			}
			
			//Assert.IsTrue(thumbnails.GetDescription(ExifThumbnailDirectory.TagCompression).StartsWith("JPEG"));
			long thumbnailOffset = Int64.Parse(exifThumbnailDir.GetDescription(ExifThumbnailDirectory.TagThumbnailOffset).Split(' ')[0]);
			const int maxIssue35Offset = 12;
			int thumbnailLength = Int32.Parse(exifThumbnailDir.GetDescription(ExifThumbnailDirectory.TagThumbnailLength).Split(' ')[0]) + maxIssue35Offset;
			byte[] thumbnail = new byte[thumbnailLength];
			
			using (var imageStream = _iStorage.ReadStream(subPath))
			{
				imageStream.Seek(thumbnailOffset, SeekOrigin.Begin);
				imageStream.Read(thumbnail, 0, thumbnailLength);
			}

			// work around Metadata Extractor issue #35
			// Assert.IsTrue(thumbnailLength > maxIssue35Offset + 1);
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

			using (MemoryStream thumbnailStream = new MemoryStream(thumbnail, issue35Offset, thumbnailLength - issue35Offset))
			{
				_iStorage.WriteStream(thumbnailStream, "/temp/test.jpg");
				
				// JpegBitmapDecoder jpegDecoder = new JpegBitmapDecoder(thumbnailStream, BitmapCreateOptions.None, BitmapCacheOption.None);
				// WriteableBitmap writeableBitmap = new WriteableBitmap(jpegDecoder.Frames[0]);
			}

		}
		
		public Tuple<bool,string> ReadExifFromFile(string subPath)
		{
			List<Directory> allExifItems;
			using ( var stream = _iStorage.ReadStream(subPath) )
			{
				if ( stream == Stream.Null ) return new Tuple<bool, string>(false,"stream null");
				try
				{
					allExifItems = ImageMetadataReader.ReadMetadata(stream).ToList();
					var offSetStatus = GetOffset(allExifItems);
					if ( !offSetStatus.Success ) return new Tuple<bool, string>(false, offSetStatus.Reason);
					
					// skip first bytes
					stream.Seek(offSetStatus.Offset, SeekOrigin.Begin);
					
					byte[] buffer = new byte[offSetStatus.Size];
					stream.Read(buffer, 0, offSetStatus.Size);
					stream.Close();
					_iStorage.WriteStream(new MemoryStream(buffer),
						"/temp/test.jpg");
				}
				catch (Exception exception)
				{
					_logger.LogError("[ReadMetaThumbnail] image failed",exception);
					// ImageProcessing or System.Exception: Handler moved stream beyond end of atom
					stream.Dispose();
					return new Tuple<bool, string>(false,exception.Message);
				}
			}

			return new Tuple<bool, string>(true,"fileHash");
		}
		
		private OffsetModel GetOffset(List<Directory> allExifItems)
		{
			var exifThumbnailDir = allExifItems.FirstOrDefault(p => p.Name == "Exif Thumbnail");
			if ( exifThumbnailDir == null ) return new OffsetModel{Success = false, Reason = "no jpeg"};
			var offsetTag = exifThumbnailDir.Tags.FirstOrDefault(p => p.DirectoryName == "Exif Thumbnail" && p.Name == "Thumbnail Offset");
			var sizeTag = exifThumbnailDir.Tags.FirstOrDefault(p => p.DirectoryName == "Exif Thumbnail" && p.Name == "Thumbnail Length");

			if ( offsetTag == null || sizeTag == null || string.IsNullOrEmpty(offsetTag.Description) || string.IsNullOrEmpty(sizeTag.Description) )
			{
				return new OffsetModel{Success = false, Reason = "not included"};
			}

			if ( !sizeTag.Description.Contains("bytes") || !offsetTag.Description.Contains("bytes"))
			{
				return new OffsetModel{Success = false, Reason = $"not include bytes in meta tag size: {sizeTag.Description} offset: {offsetTag.Description}"};
			}

			// space before
			Int32.TryParse(offsetTag.Description.Replace(" bytes", string.Empty), out var offset);
			Int32.TryParse(sizeTag.Description.Replace(" bytes", string.Empty), out var size);
			
			return new OffsetModel{ Offset = offset, Size = size, Success = true, Reason = "done"};
		}

		private void WriteImageThumbnail(Stream stream)
		{
			
		}
		
	}
}
