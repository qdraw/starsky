using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.MetaData.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using starsky.foundation.database.Helpers;
using starsky.foundation.platform.Helpers;
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

		public Thumbnail(IStorage iStorage, IStorage thumbnailStorage)
		{
			_iStorage = iStorage;
			_thumbnailStorage = thumbnailStorage;
		}

		/// <summary>
		///  This feature is used to crawl over directories and add this to the thumbnail-folder
		///  Or File
		/// </summary>
		/// <param name="subPath">folder subPath style</param>
		/// <returns>fail/pass</returns>
		/// <exception cref="FileNotFoundException">if folder/file not exist</exception>
		public bool CreateThumb(string subPath)
		{
			var isFolderOrFile = _iStorage.IsFolderOrFile(subPath);
			switch ( isFolderOrFile )
			{
				case FolderOrFileModel.FolderOrFileTypeList.Deleted:
					throw new FileNotFoundException("should enter some valid dir or file");
				case FolderOrFileModel.FolderOrFileTypeList.Folder:
				{
					var contentOfDir = _iStorage.GetAllFilesInDirectoryRecursive(subPath)
						.Where(ExtensionRolesHelper.IsExtensionExifToolSupported);
					foreach ( var singleSubPath in contentOfDir )
					{
						var fileHash = new FileHash(_iStorage).GetHashCode(singleSubPath).Key;
						CreateThumb(singleSubPath, fileHash);
					}
					return true;
				}
				default:
				{
					var fileHash = new FileHash(_iStorage).GetHashCode(subPath).Key;
					CreateThumb(subPath, fileHash);
					return true;
				}
			}
		}

		/// <summary>
		/// Create a Thumbnail file to load it faster in the UI. Use FileIndexItem or database style path, Feature used by the cli tool
		/// </summary>
		/// <param name="subPath">relative path to find the file in the storage folder</param>
		/// <param name="fileHash">the base32 hash of the subPath file</param>
		/// <returns>true, if successful</returns>
		public bool CreateThumb(string subPath, string fileHash)
		{
			if ( string.IsNullOrWhiteSpace(fileHash) ) throw new ArgumentNullException(nameof(fileHash));
			// FileType=supported + subPath=exit + fileHash=NOT exist
			if ( !ExtensionRolesHelper.IsExtensionThumbnailSupported(subPath) ||
			     !_iStorage.ExistFile(subPath) || _thumbnailStorage.ExistFile(fileHash) ) return false;

			// File is already tested
			if( _iStorage.ExistFile( GetErrorLogItemFullPath(subPath)) )
				return false;
			
			// run resize sync
			ResizeThumbnail(subPath, 1000, fileHash);
			
			// check if output any good
			RemoveCorruptImage(fileHash);
			
			if ( ! _thumbnailStorage.ExistFile(fileHash) )
			{
				var stream = new PlainTextFileHelper().StringToStream("Thumbnail error");
				_iStorage.WriteStream(stream, GetErrorLogItemFullPath(subPath));
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
			if (!_thumbnailStorage.ExistFile(fileHash)) return false;
			var imageFormat = ExtensionRolesHelper.GetImageFormat(_thumbnailStorage.ReadStream(fileHash,160));
			if ( imageFormat != ExtensionRolesHelper.ImageFormat.unknown ) return false;
			_thumbnailStorage.FileDelete(fileHash);
			return true;
		}

		public MemoryStream ResizeThumbnail(string subPath, 
			 int width, string thumbnailOutputHash = null,
			bool removeExif = false,
			ExtensionRolesHelper.ImageFormat imageFormat = ExtensionRolesHelper.ImageFormat.jpg)
		{
			var outputStream = new MemoryStream();
				
			try
			{
				// resize the image and save it to the output stream
				using (var inputStream = _iStorage.ReadStream(subPath))
				using (var image = Image.Load(inputStream))
				{
					// Add orginal rotation to the image as json
					if (image.MetaData.ExifProfile != null && !removeExif)
					{
						image.MetaData.ExifProfile.SetValue(ExifTag.Software, "Starsky");
					}
	
					if (image.MetaData.ExifProfile != null && removeExif)
					{
						image.MetaData.ExifProfile = null;
						image.MetaData.IccProfile = null;
					}
						
					image.Mutate(x => x.AutoOrient());
					image.Mutate(x => x
						.Resize(width, 0)
					);
						
					ResizeThumbnailImageFormat(image, imageFormat, outputStream);
	
					if ( !string.IsNullOrEmpty(thumbnailOutputHash) )
					{
						_thumbnailStorage.WriteStream(outputStream, thumbnailOutputHash);  
						outputStream.Dispose();
						return null;
					}
				}
	
			}
			catch (Exception ex)            
			{
				Console.WriteLine(subPath);
				Console.WriteLine(ex);
				return null;
			}
			return outputStream;
			
		}

		/// <summary>
		/// Used in ResizeThumbnailToStream to save based on the input settings
		/// </summary>
		/// <param name="image">Rgba32 image</param>
		/// <param name="imageFormat">Files ImageFormat</param>
		/// <param name="outputStream">input stream to save</param>
		private void ResizeThumbnailImageFormat(Image<Rgba32> image, ExtensionRolesHelper.ImageFormat imageFormat, 
			MemoryStream outputStream)
		{
			if ( outputStream == null ) throw new ArgumentNullException(nameof(outputStream));
			
			if (imageFormat == ExtensionRolesHelper.ImageFormat.png)
			{
				image.Save(outputStream, new PngEncoder{
					ColorType = PngColorType.Rgb, 
					CompressionLevel = 9, 
				});
				return;
			}

			image.Save(outputStream, new JpegEncoder{
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
		/// <returns>Is successfull? // private feature</returns>
		public bool RotateThumbnail(string fileHash, int orientation, int width = 1000, int height = 0 )
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
					ResizeThumbnailImageFormat(image, ExtensionRolesHelper.ImageFormat.jpg, stream);
					_thumbnailStorage.WriteStream(stream, fileHash);
				}
			}
			catch (Exception ex)            
			{
				if (!(ex is ImageFormatException) && !(ex is ArgumentException)) throw;
				Console.WriteLine(ex);
				return false;
			}
            
			return true;
		}
	}
}
