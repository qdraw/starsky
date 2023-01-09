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
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Models;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.thumbnailgeneration.Helpers
{
	public sealed class Thumbnail
	{
		private readonly IStorage _iStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IWebLogger _logger;
		private readonly AppSettings _appSettings;

		public Thumbnail(IStorage iStorage, IStorage thumbnailStorage, IWebLogger logger, AppSettings appSettings)
		{
			_iStorage = iStorage;
			_thumbnailStorage = thumbnailStorage;
			_logger = logger;
			_appSettings = appSettings;
		}

		internal async Task<List<GenerationResultModel>> CreateThumbnailAsync(string subPath)
		{
			var toAddFilePaths = new List<string>();
			switch ( _iStorage.IsFolderOrFile(subPath) )
			{
				case FolderOrFileModel.FolderOrFileTypeList.Deleted:
					return new List<GenerationResultModel>
					{
						new GenerationResultModel
						{
							SubPath = subPath,
							Success = false,
							IsNotFound = true,
							ErrorMessage = "File is deleted"
						}
					};
				case FolderOrFileModel.FolderOrFileTypeList.Folder:
				{
					var contentOfDir = _iStorage.GetAllFilesInDirectoryRecursive(subPath)
						.Where(ExtensionRolesHelper.IsExtensionExifToolSupported).ToList();
					toAddFilePaths.AddRange(contentOfDir);
					break;
				}
				case FolderOrFileModel.FolderOrFileTypeList.File:
				default:
				{
					toAddFilePaths.Add(subPath);
					break;
				}
			}
			
			var resultChunkList = await toAddFilePaths.ForEachAsync(
				async singleSubPath =>
				{
					var hashResult =  await new FileHash(_iStorage).GetHashCodeAsync(singleSubPath);
					return await CreateThumbAsync(singleSubPath, hashResult.Key);
				}, _appSettings.MaxDegreesOfParallelismThumbnail);
			
			var results = new List<GenerationResultModel>();
			foreach ( var resultChunk in resultChunkList )
			{
				results.AddRange(resultChunk);
			}

			return results;
		}
		

		/// <summary>
		/// Create a Thumbnail file to load it faster in the UI. Use FileIndexItem or database style path, Feature used by the cli tool
		/// </summary>
		/// <param name="subPath">relative path to find the file in the storage folder</param>
		/// <param name="fileHash">the base32 hash of the subPath file</param>
		/// <param name="skipExtraLarge">skip the extra large variant</param>
		/// <returns>true, if successful</returns>
		internal Task<IEnumerable<GenerationResultModel>> CreateThumbAsync(string subPath, string fileHash, bool skipExtraLarge = false)
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
		private async Task<IEnumerable<GenerationResultModel>> CreateThumbInternal(string subPath, string fileHash, bool skipExtraLarge = false)
		{
			// FileType=supported + subPath=exit + fileHash=NOT exist
			if ( !ExtensionRolesHelper.IsExtensionThumbnailSupported(subPath) ||
			     !_iStorage.ExistFile(subPath) )
			{
				return ThumbnailNameHelper.GeneratedThumbnailSizes.Select(size => new GenerationResultModel
				{
					SubPath = subPath,
					FileHash = fileHash,
					Success = false,
					IsNotFound = true,
					ErrorMessage = "File is deleted OR not supported",
					Size = size
				}).ToList();
			}

			// File is already tested
			if ( _iStorage.ExistFile(GetErrorLogItemFullPath(subPath)) )
			{
				return ThumbnailNameHelper.GeneratedThumbnailSizes.Select(size => new GenerationResultModel
				{
					SubPath = subPath,
					FileHash = fileHash,
					Success = false,
					IsNotFound = false,
					ErrorMessage = "File already failed before",
					Size = size
				}).ToList();
			}

			// First create from the source file a thumbnail image (large or extra large)
			var thumbnailToSourceSize = ThumbnailToSourceSize(skipExtraLarge);
			var largeThumbnailHash = LargeThumbnailHash(fileHash, thumbnailToSourceSize);
			var thumbnailFromThumbnailUpdateList = ListThumbnailToBeCreated(fileHash);
			var largeImageResult = await CreateLargestImageFromSource(fileHash, largeThumbnailHash, subPath, thumbnailToSourceSize);

			// when all images are already created
			if ( !thumbnailFromThumbnailUpdateList.Any() )
			{
				return ThumbnailNameHelper.GeneratedThumbnailSizes.Select(size => new GenerationResultModel
				{
					Success = _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(fileHash, size)), 
					Size = size, 
					FileHash = fileHash, 
					IsNotFound = false
				}).ToList();
			}

			var results = await thumbnailFromThumbnailUpdateList.ForEachAsync(
				async (size)
					=> await ResizeThumbnailFromThumbnailImage(
						largeThumbnailHash, // source location
						ThumbnailNameHelper.GetSize(size),
						subPath, // used for reference only
						ThumbnailNameHelper.Combine(fileHash, size)),
				3);

			_logger.LogInformation(".");

			// results return null if thumbnailFromThumbnailUpdateList has lenght 0
			return results.Select(p =>p.Item2).Append(largeImageResult);
		}

		private static ThumbnailSize ThumbnailToSourceSize(bool skipExtraLarge)
		{
			var thumbnailToSourceSize = ThumbnailSize.ExtraLarge;
			if ( skipExtraLarge ) thumbnailToSourceSize = ThumbnailSize.Large;
			return thumbnailToSourceSize;
		}

		private static string LargeThumbnailHash(string fileHash, ThumbnailSize thumbnailToSourceSize)
		{
			var largeThumbnailHash = ThumbnailNameHelper.Combine(fileHash, thumbnailToSourceSize);
			return largeThumbnailHash;
		}
		
		internal async Task<GenerationResultModel> CreateLargestImageFromSource(
			string fileHash, string largeThumbnailHash, string subPath,
			ThumbnailSize thumbnailToSourceSize)
		{
			var resultModel = new GenerationResultModel
			{
				IsNotFound = false,
				Success = false,
				ErrorMessage = string.Empty,
				FileHash = fileHash,
				SubPath = subPath,
				Size = thumbnailToSourceSize
			};

			if ( _thumbnailStorage.ExistFile(ThumbnailNameHelper.Combine(
				    fileHash, thumbnailToSourceSize)) )
			{
				// file already exist so skip
				resultModel.Success = true;
				return resultModel;
			}

			// run resize sync
			var (_, resizeSuccess, resizeMessage) = await ResizeThumbnailFromSourceImage(subPath, 
				ThumbnailNameHelper.GetSize(thumbnailToSourceSize), 
				largeThumbnailHash );

			// check if output any good
			RemoveCorruptImage(fileHash, thumbnailToSourceSize);

			if ( !resizeSuccess || ! _thumbnailStorage.ExistFile(
				    ThumbnailNameHelper.Combine(fileHash, thumbnailToSourceSize)) )
			{
				_logger.LogError($"[ResizeThumbnailFromSourceImage] " +
				                 $"output is null or corrupt for subPath {subPath}");
				await WriteErrorMessageToBlockLog(subPath, resizeMessage);

				resultModel.ErrorMessage = resizeMessage;
				return resultModel;
			}
			
			resultModel.Success = true;
			return resultModel;
		}

		private List<ThumbnailSize> ListThumbnailToBeCreated(string fileHash)
		{
			// And create then a thumbnail from the extra large thumbnail
			// to the small thumbnail
			var thumbnailFromThumbnailUpdateList = new List<ThumbnailSize>();
			void AddFileNames(ThumbnailSize size)
			{
				if ( !_thumbnailStorage.ExistFile(
					    ThumbnailNameHelper.Combine(
						    fileHash, size))
				   )
				{
					thumbnailFromThumbnailUpdateList.Add(size);
				}
			}
			// Large <- will be false when skipExtraLarge = true, its already created 
			ThumbnailNameHelper.SecondGeneratedThumbnailSizes.ToList().ForEach(AddFileNames);
			
			return thumbnailFromThumbnailUpdateList;
		}

		internal async Task WriteErrorMessageToBlockLog(string subPath, string resizeMessage)
		{
			var stream = PlainTextFileHelper.StringToStream("Thumbnail error " + resizeMessage);
			await _iStorage.WriteStreamAsync(stream, GetErrorLogItemFullPath(subPath));
		}
		
		private static string GetErrorLogItemFullPath(string subPath)
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

		/// <summary>
		/// Resize image from other thumbnail
		/// </summary>
		/// <param name="fileHash">source location</param>
		/// <param name="width">width in pixels</param>
		/// <param name="thumbnailOutputHash">name of output file</param>
		/// <param name="removeExif">remove meta data</param>
		/// <param name="imageFormat">jpg, or png</param>
		/// <param name="subPathReference">for reference only</param>
		/// <returns>(stream, fileHash, and is ok)</returns>
		public async Task<(MemoryStream?,GenerationResultModel)> ResizeThumbnailFromThumbnailImage(string fileHash, // source location
			int width,  string? subPathReference = null, string? thumbnailOutputHash = null,
			bool removeExif = false,
			ExtensionRolesHelper.ImageFormat imageFormat = ExtensionRolesHelper.ImageFormat.jpg
		)
		{
			var outputStream = new MemoryStream();
			var result = new GenerationResultModel
			{
				FileHash = ThumbnailNameHelper.RemoveSuffix(thumbnailOutputHash),
				IsNotFound = false,
				SizeInPixels = width,
				Success = true,
				SubPath = subPathReference!
			};
			
			try
			{
				// resize the image and save it to the output stream
				using (var inputStream = _thumbnailStorage.ReadStream(fileHash))
				using (var image = await Image.LoadAsync(inputStream))
				{

					ImageSharpImageResize(image, width, removeExif);
					await SaveThumbnailImageFormat(image, imageFormat, outputStream);

					// When thumbnailOutputHash is nothing return stream instead of writing down
					if ( string.IsNullOrEmpty(thumbnailOutputHash) ) return (outputStream, result);
					
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
				result.Success = false;
				result.ErrorMessage = message;
				return (null,result);
			}
			
			return (outputStream, result);
		}
		
		
		public async Task<(MemoryStream?, bool, string)> ResizeThumbnailFromSourceImage(string subPath, 
			int width, string? thumbnailOutputHash = null,
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

		private static void ImageSharpImageResize(Image image, int width, bool removeExif)
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
		internal static Task SaveThumbnailImageFormat(Image image,
			ExtensionRolesHelper.ImageFormat imageFormat,
			MemoryStream outputStream)
		{
			if ( outputStream == null )
			{
				throw new ArgumentNullException(nameof(outputStream));
			}

			return SaveThumbnailImageFormatInternal(image, imageFormat, outputStream);
		}

		/// <summary>
		/// Private: use => SaveThumbnailImageFormat
		/// Used in ResizeThumbnailToStream to save based on the input settings
		/// </summary>
		/// <param name="image">Rgba32 image</param>
		/// <param name="imageFormat">Files ImageFormat</param>
		/// <param name="outputStream">input stream to save</param>
		private static async Task SaveThumbnailImageFormatInternal(Image image, ExtensionRolesHelper.ImageFormat imageFormat, 
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
		internal async Task<bool> RotateThumbnail(string fileHash, int orientation, int width = 1000, int height = 0 )
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
