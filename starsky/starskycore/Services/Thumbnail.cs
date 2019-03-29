using System;
using System.IO;
using System.Threading.Tasks;
using starskycore.Helpers;
using starskycore.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.MetaData.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace starskycore.Services
{
	public class Thumbnail
	{
		private readonly IStorage _iStorage;
		private readonly IExifTool _exifTool;

		public Thumbnail(IStorage iStorage, IExifTool exifTool)
		{
			_iStorage = iStorage;
			_exifTool = exifTool;
		}

		/// <summary>
		/// Create a Thumbnail file to load it faster in the UI. Use FileIndexItem or database style path, Feature used by the cli tool
		/// </summary>
		/// <param name="subPath">relative path to find the file in the storage folder</param>
		/// <param name="fileHash">the base32 hash of the subpath file</param>
		/// <returns>true, if succesfull</returns>
		public bool CreateThumb(string subPath, string fileHash)
		{
			// FileType=supported + subPath=exit + fileHash=NOT exist
			if ( !ExtensionRolesHelper.IsExtensionThumbnailSupported(subPath) ||
			     !_iStorage.ExistFile(subPath) || _iStorage.ThumbnailExist(fileHash) ) return false;

			var resizeResult = ResizeThumbnailTimeoutWrap(subPath, fileHash, 1000).Result;
			// todo: RemoveCorruptImage(thumbPath);
			if ( resizeResult ) Console.Write(".");
			
			return resizeResult;
		}

		/// <summary>
		/// Wrapper to Make a sync task sync
		/// </summary>
		/// <param name="subPath">sub path</param>
		/// <param name="thumbHash">file hash</param>
		/// <param name="width">width in pixels</param>
		/// <param name="height">0 is keep aspect ratio</param>
		/// <param name="quality">range 0-100</param>
		/// <returns>async true, is good</returns>
		private async Task<bool> ResizeThumbnailTimeoutWrap(string subPath, string thumbHash, int width, int height = 0,  int quality = 75)
		{
			//adding .ConfigureAwait(false) may NOT be what you want but google it.
			return await Task.Run(() => ResizeThumbnailTimeOut(subPath, thumbHash, width, height, quality)).ConfigureAwait(false);
		}

		/// <summary>
		/// Timeout feature to check if the service is answering within 8 seconds, Ignore Error CS1998
		/// </summary>
		/// <param name="subPath">sub path</param>
		/// <param name="thumbHash">file hash</param>
		/// <param name="width">width in pixels</param>
		/// <param name="height">0 is keep aspect ratio</param>
		/// <param name="quality">range 0-100</param>
		/// <returns>async true, is good</returns>
		#pragma warning disable 1998
		private async Task<bool> ResizeThumbnailTimeOut(string subPath, string thumbHash, int width, int height = 0,  int quality = 75){
		#pragma warning restore 1998
            
			var task = Task.Run(() => ResizeThumbnailPlain(subPath, thumbHash, width, height, quality));
			if (task.Wait(TimeSpan.FromSeconds(120))) 
				return task.Result;

			Console.WriteLine(">>>>>>>>>>>            Timeout ThumbService "
			                  + subPath 
			                  + "            <<<<<<<<<<<<");
              
			return false;
		}

		private bool ResizeThumbnailPlain(string subPath, string thumbHash, int width, int height = 0,  int quality = 75 )
		{
			return _iStorage.ThumbnailWriteStream(ResizeThumbnail(subPath, width, height, quality), thumbHash);
		} 
		
		
		/// <summary>
		/// Resize the thumbnail to object
		/// </summary>
		/// <param name="subPath">location</param>
		/// <param name="width">the width of the output image</param>
		/// <param name="height">use 0 to keep ratio</param>
		/// <param name="quality">only for jpeg, value 0 - 100</param>
		/// <param name="removeExif">dont store exif in output memorystream</param>
		/// <param name="imageFormat">jpeg, or png in Enum</param>
		/// <returns>MemoryStream with resized image</returns>
		public Stream ResizeThumbnail(string subPath, int width, int height = 0, int quality = 75, 
			bool removeExif = false, ExtensionRolesHelper.ImageFormat imageFormat = ExtensionRolesHelper.ImageFormat.jpg)
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
                        image.MetaData.IccProfile?.Entries.Clear();
                    }
                    image.Mutate(x => x.AutoOrient());
                    image.Mutate(x => x
                        .Resize(width, height)
                    );
	                
                    ResizeThumbnailImageFormat(image, imageFormat, outputStream, removeExif, quality);
                }

            }
            catch (Exception ex)            
            {
                if (!(ex is ImageFormatException) && !(ex is ArgumentException)) throw;
                Console.WriteLine(ex);
                return null;
            }
	        
	        // todo: add exif tool copy
	        
            return outputStream;
        }
		
		/// <summary>
		/// Used in ResizeThumbnailToStream to save based on the input settings
		/// </summary>
		/// <param name="image">Rgba32 image</param>
		/// <param name="imageFormat">Files ImageFormat</param>
		/// <param name="outputStream">input stream to save</param>
		/// <param name="removeExif">true, remove exif, default false</param>
		/// <param name="quality">default 75, only for jpegs</param>
		private void ResizeThumbnailImageFormat(Image<Rgba32> image, ExtensionRolesHelper.ImageFormat imageFormat, 
			MemoryStream outputStream, bool removeExif = false, int quality = 75)
		{
			if ( outputStream == null ) throw new ArgumentNullException(nameof(outputStream));
			
			if (imageFormat == ExtensionRolesHelper.ImageFormat.png)
			{
				image.SaveAsPng(outputStream, new PngEncoder{
					ColorType = PngColorType.Rgb, 
					CompressionLevel = 9, 
					WriteGamma = false 
				});
			}
			else
			{
				image.SaveAsJpeg(outputStream, new JpegEncoder{
					IgnoreMetadata = removeExif,
					Quality = quality
				});
			}
		}
		
		
		/// <summary>
		/// Rotate an image, by rotating the pixels and resize the thumbnail.Please do not apply any orientation exiftag on this file
		/// </summary>
		/// <param name="fileHash"></param>
		/// <param name="orientation">-1 > Rotage -90degrees, anything else 90 degrees</param>
		/// <param name="width">to resize, default 1000</param>
		/// <param name="height">to resize, default keep ratio (0)</param>
		/// <returns>Is successfull? // private feature</returns>
		public bool RotateThumbnail(string fileHash, int orientation, int width = 1000, int height = 0 )
		{
			if (!_iStorage.ThumbnailExist(fileHash)) return false;

			// the orientation is -1 or 1
			var rotateMode = RotateMode.Rotate90;
			if (orientation == -1) rotateMode = RotateMode.Rotate270; 

			try
			{
				using (Image<Rgba32> image = Image.Load(fileHash))
				using ( var stream = new MemoryStream() )
				{
					image.Mutate(x => x
						.Resize(width, height)
					);
					image.Mutate(x => x
						.Rotate(rotateMode));
					
					ResizeThumbnailImageFormat(image, ExtensionRolesHelper.ImageFormat.jpg, stream, false, 90);
					_iStorage.ThumbnailWriteStream(stream, fileHash);
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
