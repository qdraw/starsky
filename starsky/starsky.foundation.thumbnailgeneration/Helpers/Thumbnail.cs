using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using starsky.foundation.database.Helpers;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.thumbnailgeneration.Helpers
{
	public class Thumbnail
	{
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IWebLogger _logger;

		public Thumbnail(IStorage iStorage, IStorage thumbnailStorage, IWebLogger logger)
		{
			_iStorage = iStorage;
			_thumbnailStorage = thumbnailStorage;
			_logger = logger;
		}

		/// <summary>
		///  This feature is used to crawl over directories and add this to the thumbnail-folder
		///  Or File
		/// </summary>
		/// <param name="subPath">folder subPath style</param>
		/// <returns>fail/pass</returns>
		/// <exception cref="FileNotFoundException">if folder/file not exist</exception>
		public async Task<List<(string, bool)>> CreateThumb(string subPath)
		{
			var isFolderOrFile = _iStorage.IsFolderOrFile(subPath);
			var result = new List<(string, bool)>();
			switch ( isFolderOrFile )
			{
				case FolderOrFileModel.FolderOrFileTypeList.Deleted:
					throw new FileNotFoundException("should enter some valid dir or file");
				case FolderOrFileModel.FolderOrFileTypeList.Folder:
				{
					var contentOfDir = _iStorage.GetAllFilesInDirectoryRecursive(subPath)
						.Where(ExtensionRolesHelper.IsExtensionExifToolSupported).ToList();
					foreach ( var singleSubPath in contentOfDir )
					{
						var hashResult =  await new FileHash(_iStorage).GetHashCodeAsync(singleSubPath);
						if ( hashResult.Value ) result.Add((singleSubPath, await CreateThumb(singleSubPath, hashResult.Key )));
					}
					return result;
				}
				default:
				{
					var hashResult =  await new FileHash(_iStorage).GetHashCodeAsync(subPath);
					if ( hashResult.Value ) result.Add((subPath, await CreateThumb(subPath, hashResult.Key)));
					return result;
				}
			}
		}

		/// <summary>
		/// Create a Thumbnail file to load it faster in the UI. Use FileIndexItem or database style path, Feature used by the cli tool
		/// </summary>
		/// <param name="subPath">relative path to find the file in the storage folder</param>
		/// <param name="fileHash">the base32 hash of the subPath file</param>
		/// <returns>true, if successful</returns>
		public async Task<bool> CreateThumb(string subPath, string fileHash)
		{
			if ( string.IsNullOrWhiteSpace(fileHash) ) throw new ArgumentNullException(nameof(fileHash));
			
			// FileType=supported + subPath=exit + fileHash=NOT exist
			if ( !ExtensionRolesHelper.IsExtensionThumbnailSupported(subPath) ||
			     !_iStorage.ExistFile(subPath) || _thumbnailStorage.ExistFile(fileHash) ) return false;

			// File is already tested
			if( _iStorage.ExistFile( GetErrorLogItemFullPath(subPath)) )
				return false;
			
			// run resize sync
			var largeThumbnailHash = $"{fileHash}@2000";
			await ResizeThumbnailFromSourceImage(subPath, 2000, largeThumbnailHash);

			await (new List<int>{1000,300}).ForEachAsync(
				async (size) 
					=> await ResizeThumbnailFromThumbnailImage(largeThumbnailHash, size, 
						$"{fileHash}@{size}"),
				10);

			// check if output any good
			RemoveCorruptImage(fileHash);
			
			if ( ! _thumbnailStorage.ExistFile(fileHash) )
			{
				var stream = new PlainTextFileHelper().StringToStream("Thumbnail error");
				await _iStorage.WriteStreamAsync(stream, GetErrorLogItemFullPath(subPath));
				return false;
			}
			
			Console.Write(".");
			return true;
		}
		
		private string GetErrorLogItemFullPath(string subPath)
		{
			return Breadcrumbs.BreadcrumbHelper(subPath).LastOrDefault()
			       + "/"
				   + "_"
			       + Path.GetFileNameWithoutExtension(PathHelper.GetFileName(subPath)) 
			       + ".log";
		}
		
		/// <summary>
		/// Check if the image has the right first bytes, if not remove
		/// </summary>
		/// <param name="fileHash">the fileHash file</param>
		internal bool RemoveCorruptImage(string fileHash)
		{
			if (!_thumbnailStorage.ExistFile(fileHash+ "@2000")) return false;
			var imageFormat = ExtensionRolesHelper.GetImageFormat(_thumbnailStorage.ReadStream(fileHash+ "@2000",160));
			if ( imageFormat != ExtensionRolesHelper.ImageFormat.unknown ) return false;
			_thumbnailStorage.FileDelete(fileHash+ "@2000");
			return true;
		}

		public async Task<MemoryStream> ResizeThumbnailFromThumbnailImage(string fileHash, 
			int width, string thumbnailOutputHash = null,
			bool removeExif = false,
			ExtensionRolesHelper.ImageFormat imageFormat = ExtensionRolesHelper.ImageFormat.jpg)
		{
			var outputStream = new MemoryStream();

			try
			{
				// resize the image and save it to the output stream
				using (var inputStream = _thumbnailStorage.ReadStream(fileHash))
				using (var image = await Image.LoadAsync(inputStream))
				{
					ImageSharpImageResize(image, width, removeExif);
					await SaveThumbnailImageFormat(image, imageFormat, outputStream);

					// When thumbnailOutputHash is nothing return stream instead of writing down
					if ( string.IsNullOrEmpty(thumbnailOutputHash) ) return outputStream;
					
					// only when a hash exists
					await _thumbnailStorage.WriteStreamAsync(outputStream, thumbnailOutputHash);
					// Disposed in WriteStreamAsync
				}
	
			}
			catch (Exception ex)            
			{
				var message = ex.Message;
				if ( message.StartsWith("Image cannot be loaded") ) message = "Image cannot be loaded";
				_logger.LogError($"[ResizeThumbnailFromThumbnailImage] Exception {fileHash} {message}", ex);
				
				return null;
			}
			return outputStream;
		}
		
		
		public async Task<MemoryStream> ResizeThumbnailFromSourceImage(string subPath, 
			 int width, string thumbnailOutputHash = null,
			bool removeExif = false,
			ExtensionRolesHelper.ImageFormat imageFormat = ExtensionRolesHelper.ImageFormat.jpg)
		{
			var outputStream = new MemoryStream();

			try
			{
				// resize the image and save it to the output stream
				using (var inputStream = _iStorage.ReadStream(subPath))
				using (var image = await Image.LoadAsync(inputStream))
				{
					ImageSharpImageResize(image, width, removeExif);
					await SaveThumbnailImageFormat(image, imageFormat, outputStream);

					// When thumbnailOutputHash is nothing return stream instead of writing down
					if ( string.IsNullOrEmpty(thumbnailOutputHash) ) return outputStream;
					
					// only when a hash exists
					await _thumbnailStorage.WriteStreamAsync(outputStream, thumbnailOutputHash);
					// Disposed in WriteStreamAsync
				}
	
			}
			catch (Exception ex)            
			{
				var message = ex.Message;
				if ( message.StartsWith("Image cannot be loaded") ) message = "Image cannot be loaded";
				_logger.LogError($"[ResizeThumbnailFromSourceImage] Exception {subPath} {message}", ex);
				
				return null;
			}
			return outputStream;
		}

		internal void ImageSharpImageResize(Image image, int width, bool removeExif)
		{
			// Add original rotation to the image as json
			if (image.Metadata.ExifProfile != null && !removeExif)
			{
				image.Metadata.ExifProfile.SetValue(ExifTag.Software, "Starsky");
			}
					
			if (image.Metadata.ExifProfile != null && removeExif)
			{
				image.Metadata.ExifProfile = null;
				image.Metadata.IccProfile = null;
			}

			var height = 0;
			if ( image.Height >= image.Width )
			{
				height = width;
				width = 0;
			}
						
			image.Mutate(x => x.AutoOrient());
			image.Mutate(x => x
				.Resize(width, height)
			);
		}

		/// <summary>
		/// Used in ResizeThumbnailToStream to save based on the input settings
		/// </summary>
		/// <param name="image">Rgba32 image</param>
		/// <param name="imageFormat">Files ImageFormat</param>
		/// <param name="outputStream">input stream to save</param>
		internal async Task SaveThumbnailImageFormat(Image image, ExtensionRolesHelper.ImageFormat imageFormat, 
			MemoryStream outputStream)
		{
			if ( outputStream == null ) throw new ArgumentNullException(nameof(outputStream));
			
			if (imageFormat == ExtensionRolesHelper.ImageFormat.png)
			{
				await image.SaveAsync(outputStream, new PngEncoder{
					ColorType = PngColorType.Rgb, 
					CompressionLevel = PngCompressionLevel.Level9, 
				});
				return;
			}

			await image.SaveAsync(outputStream, new JpegEncoder{
				Quality = 100,
			});
		}
		
		/// <summary>
		/// Rotate an image, by rotating the pixels and resize the thumbnail.Please do not apply any orientation exif-tag on this file
		/// </summary>
		/// <param name="fileHash"></param>
		/// <param name="orientation">-1 > Rotage -90degrees, anything else 90 degrees</param>
		/// <param name="width">to resize, default 1000</param>
		/// <param name="height">to resize, default keep ratio (0)</param>
		/// <returns>Is successful? // private feature</returns>
		public async Task<bool> RotateThumbnail(string fileHash, int orientation, int width = 1000, int height = 0 )
		{
			if (!_thumbnailStorage.ExistFile(fileHash)) return false;

			// the orientation is -1 or 1
			var rotateMode = RotateMode.Rotate90;
			if (orientation == -1) rotateMode = RotateMode.Rotate270; 

			try
			{
				using (var inputStream = _thumbnailStorage.ReadStream(fileHash))
				using (var image = Image.Load(inputStream))
				using ( var stream = new MemoryStream() )
				{
					image.Mutate(x => x
						.Resize(width, height)
					);
					image.Mutate(x => x
						.Rotate(rotateMode));
					
					// Image<Rgba32> image, ExtensionRolesHelper.ImageFormat imageFormat, MemoryStream outputStream
					await SaveThumbnailImageFormat(image, ExtensionRolesHelper.ImageFormat.jpg, stream);
					await _thumbnailStorage.WriteStreamAsync(stream, fileHash);
				}
			}
			catch (Exception ex)            
			{
				Console.WriteLine(ex);
				return false;
			}
            
			return true;
		}
	}
}
