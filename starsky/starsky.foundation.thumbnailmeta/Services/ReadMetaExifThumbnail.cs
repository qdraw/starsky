using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.readmeta.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using Directory = MetadataExtractor.Directory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using starsky.foundation.metathumbnail.Helpers;
using starsky.foundation.platform.Helpers;
using starsky.foundation.readmeta.Services;

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

		private (ExifThumbnailDirectory, int, int) GetExifMetaDirectories(string subPath)
		{
			using ( var stream = _iStorage.ReadStream(subPath) )
			{
				var allExifItems =
					ImageMetadataReader.ReadMetadata(stream).ToList();
				var exifThumbnailDir =
					allExifItems.FirstOrDefault(p =>
						p.Name == "Exif Thumbnail") as ExifThumbnailDirectory;
				
				var jpegTags = allExifItems.FirstOrDefault(p =>
						p.Name == "JPEG")?.Tags;

				int.TryParse(
					jpegTags?.FirstOrDefault(p => p.Name == "Image Height")?
						.Description.Replace(" pixels",string.Empty), out var height);
				
				int.TryParse(
					jpegTags?.FirstOrDefault(p => p.Name == "Image Width")?
						.Description.Replace(" pixels",string.Empty), out var width);
				return (exifThumbnailDir, width, height);
			}
		}

		private OffsetModel GetOffset(ExifThumbnailDirectory exifThumbnailDir, string subPath)
		{
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
			return new OffsetModel
			{
				Index = issue35Offset,
				Count = thumbnailLength - issue35Offset,
				Data = thumbnail,
				// SourceHeight = sourceHeight,
				// SourceWidth = sourceWidth
			};
		}

		

		
 
		public async Task ReadExifFromFile2(string subPath)
		{
			var first50BytesStream = _iStorage.ReadStream(subPath,50);
			var imageFormat = ExtensionRolesHelper.GetImageFormat(first50BytesStream);
			await first50BytesStream.DisposeAsync();
			if ( imageFormat != ExtensionRolesHelper.ImageFormat.jpg )
			{
				return;
			}
			
			var (exifThumbnailDir, sourceWidth, sourceHeight) = GetExifMetaDirectories(subPath);
			var offsetData = GetOffset(exifThumbnailDir, subPath);
			
			
			using (var thumbnailStream = new MemoryStream(offsetData.Data, offsetData.Index, offsetData.Count ))
			{
				// await _iStorage.WriteStreamAsync(thumbnailStream, "/temp/test.jpg");

				
				using ( var smallImage = Image.Load(thumbnailStream, out var format) )
				{

					var result = NewImageSize.NewImageSizeCalc(smallImage.Width,
						smallImage.Height, sourceWidth, sourceHeight);


					Console.WriteLine();
				// 	
				// 	// sourceWidth, sourceHeight
				// 		
				// 	// smallImage.Height
				// 	// smallImage.Mutate(
				// 	// 	i => i.Resize(width, height)
				// 	// 		.Crop(new Rectangle(x, y, cropWidth, cropHeight)));
				// 	//
				// 	var outputStream = new MemoryStream();
				// 		
				// 	await smallImage.SaveAsync(outputStream, format);
				}
			}

		}


		
	}
}
