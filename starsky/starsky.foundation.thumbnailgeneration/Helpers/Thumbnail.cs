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
using starsky.foundation.storage.Storage;

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
		/// <param name="recursive">recursive</param>
		/// <returns>fail/pass</returns>
		/// <exception cref="FileNotFoundException">if folder/file not exist</exception>
		public async Task<List<(string, bool)>> CreateThumb(string subPath, bool recursive = true)
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
		/// <param name="skipExtraLarge">skip the extra large variant</param>
		/// <returns>true, if successful</returns>
		public Task<bool> CreateThumb(string subPath, string fileHash, bool skipExtraLarge = false)
		{
			if ( string.IsNullOrWhiteSpace(fileHash) ) throw new ArgumentNullException(nameof(fileHash));

			return CreateThumbInternal(subPath, fileHash, skipExtraLarge);
		}


		/// <summary>
		/// Private use => CreateThumb
		/// Create a Thumbnail file to load it faster in the UI. Use FileIndexItem or database style path, Feature used by the cli tool
		/// </summary>
		/// <param name="subPath">relative path to find the file in the storage folder</param>
		/// <param name="fileHash">the base32 hash of the subPath file</param>
		/// <param name="skipExtraLarge">skip the extra large image</param>
		/// <returns>true, if successful</returns>
		private async Task<bool> CreateThumbInternal(string subPath, string fileHash, bool skipExtraLarge = false)
		{
			// FileType=supported + subPath=exit + fileHash=NOT exist
			if ( !ExtensionRolesHelper.IsExtensionThumbnailSupported(subPath) ||
			     !_iStorage.ExistFile(subPath) )
			{
				return false;
			}

			// File is already tested
			if( _iStorage.ExistFile( GetErrorLogItemFullPath(subPath)) )
				return false;

			var thumbnailToSourceSize = ThumbnailSize.ExtraLarge;
			if ( skipExtraLarge ) thumbnailToSourceSize = ThumbnailSize.Large;

			var largeThumbnailHash = ThumbnailNameHelper.Combine(fileHash, thumbnailToSourceSize);

			if ( !_thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(
				fileHash,thumbnailToSourceSize)) )
			{
				// run resize sync
				var (_, resizeSuccess, resizeMessage) = (await ResizeThumbnailFromSourceImage(subPath, 
					ThumbnailNameHelper.GetSize(thumbnailToSourceSize), 
					largeThumbnailHash ));

				// check if output any good
				RemoveCorruptImage(fileHash, thumbnailToSourceSize);

				if ( !resizeSuccess || ! _thumbnailStorage.ExistFile(
					ThumbnailNameHelper.Combine(fileHash, thumbnailToSourceSize)) )
				{
					_logger.LogError($"[ResizeThumbnailFromSourceImage] " +
					                 $"output is null or corrupt for subPath {subPath}");
					await WriteErrorMessageToBlockLog(subPath, resizeMessage);
					return false;
				}
				Console.Write(".");
			}

			var thumbnailFromThumbnailUpdateList = new List<ThumbnailSize>();
			void Add(ThumbnailSize size)
			{
				if ( !_thumbnailStorage.ExistFile(
					ThumbnailNameHelper.Combine(
						fileHash, size))
				)
				{
					thumbnailFromThumbnailUpdateList.Add(size);
				}
			}
			new List<ThumbnailSize>
			{
				ThumbnailSize.Small, 
				ThumbnailSize.Large // <- will be false when skipExtraLarge = true
			}.ForEach(Add);

			await ( thumbnailFromThumbnailUpdateList ).ForEachAsync(
				async (size)
					=> await ResizeThumbnailFromThumbnailImage(
						largeThumbnailHash,
						ThumbnailNameHelper.GetSize(size),
						ThumbnailNameHelper.Combine(fileHash, size)),
				10);

			Console.Write(".");
			return thumbnailFromThumbnailUpdateList.Any();
		}

		private async Task WriteErrorMessageToBlockLog(string subPath, string resizeMessage)
		{
			var stream = new PlainTextFileHelper().StringToStream("Thumbnail error " + resizeMessage);
			await _iStorage.WriteStreamAsync(stream, GetErrorLogItemFullPath(subPath));
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
		/// <param name="thumbnailToSourceSize">size of output thumbnail Large/ExtraLarge</param>
		internal bool RemoveCorruptImage(string fileHash,
			ThumbnailSize thumbnailToSourceSize)
		{
			if (!_thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(fileHash,thumbnailToSourceSize))) return false;
			var imageFormat = ExtensionRolesHelper.GetImageFormat(_thumbnailStorage.ReadStream(
				ThumbnailNameHelper.Combine(fileHash,thumbnailToSourceSize),160));
			if ( imageFormat != ExtensionRolesHelper.ImageFormat.unknown ) return false;
			_thumbnailStorage.FileDelete(ThumbnailNameHelper.Combine(fileHash,thumbnailToSourceSize));
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
		
		
		public async Task<(MemoryStream, bool, string)> ResizeThumbnailFromSourceImage(string subPath, 
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
					if ( string.IsNullOrEmpty(thumbnailOutputHash) )
					{
						return (outputStream, true, "Ok give stream back instead of disk write");
					}
					
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
				return (null, false, message);
			}
			
			return (null, true, "Ok but written to disk");
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
				.Resize(width, height, KnownResamplers.Lanczos3)
			);
		}

		/// <summary>
		/// Used in ResizeThumbnailToStream to save based on the input settings
		/// </summary>
		/// <param name="image">Rgba32 image</param>
		/// <param name="imageFormat">Files ImageFormat</param>
		/// <param name="outputStream">input stream to save</param>
		internal Task SaveThumbnailImageFormat(Image image,
			ExtensionRolesHelper.ImageFormat imageFormat,
			MemoryStream outputStream)
		{
			if ( outputStream == null )
				throw new ArgumentNullException(nameof(outputStream));

			return SaveThumbnailImageFormatInternal(image, imageFormat, outputStream);
		}

		/// <summary>
		/// Private: use => SaveThumbnailImageFormat
		/// Used in ResizeThumbnailToStream to save based on the input settings
		/// </summary>
		/// <param name="image">Rgba32 image</param>
		/// <param name="imageFormat">Files ImageFormat</param>
		/// <param name="outputStream">input stream to save</param>
		private async Task SaveThumbnailImageFormatInternal(Image image, ExtensionRolesHelper.ImageFormat imageFormat, 
			MemoryStream outputStream)
		{
			if (imageFormat == ExtensionRolesHelper.ImageFormat.png)
			{
				await image.SaveAsync(outputStream, new PngEncoder{
					ColorType = PngColorType.Rgb, 
					CompressionLevel = PngCompressionLevel.BestSpeed, 
					IgnoreMetadata = true,
					TransparentColorMode = PngTransparentColorMode.Clear,
				});
				return;
			}

			await image.SaveAsync(outputStream, new JpegEncoder{
				Quality = 90,
			});
		}
		
		/// <summary>
		/// Rotate an image, by rotating the pixels and resize the thumbnail.Please do not apply any orientation exif-tag on this file
		/// </summary>
		/// <param name="fileHash"></param>
		/// <param name="orientation">-1 > Rotate -90degrees, anything else 90 degrees</param>
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
				using (var image = await Image.LoadAsync(inputStream))
				using ( var stream = new MemoryStream() )
				{
					image.Mutate(x => x
						.Resize(width, height, KnownResamplers.Lanczos3)
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
