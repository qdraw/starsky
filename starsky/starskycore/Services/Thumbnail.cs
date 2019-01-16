using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using starsky.Helpers;
using starsky.Models;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.MetaData.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace starsky.Services
{
    public class Thumbnail
    {
        private readonly AppSettings _appSettings;
        private readonly IExiftool _exiftool;

        public Thumbnail(AppSettings appSettings, IExiftool exiftool = null)
        {
            _appSettings = appSettings;
            _exiftool = exiftool;
        }
        

        /// <summary>
        /// Rename a thumbnail, used when you change exifdata, the hash is also changed but the images remain the same
        /// </summary>
        /// <param name="oldHashCode">the old base32 hashcode</param>
        /// <param name="newHashCode">the new base32 hashcode></param>
        /// <exception cref="FileNotFoundException">the ThumbnailTempFolder does not exist</exception>
        public void RenameThumb(string oldHashCode, string newHashCode)
        {
            if (!Directory.Exists(_appSettings.ThumbnailTempFolder))
            {
                throw new FileNotFoundException("ThumbnailTempFolder not found " 
                                                + _appSettings.ThumbnailTempFolder);
            }

            var oldThumbPath = _appSettings.ThumbnailTempFolder + oldHashCode + ".jpg";
            var newThumbPath = _appSettings.ThumbnailTempFolder + newHashCode + ".jpg";

            if (!File.Exists(oldThumbPath))
            {
                return;
            }
            
            if (File.Exists(newThumbPath))
            {
                return;
            }

            File.Move(oldThumbPath, newThumbPath);
        }

        /// <summary>
        /// For recalc new thumb objects
        /// </summary>
        /// <param name="listOfOldFileIndexItems"></param>
        public void RenameThumb(IEnumerable<FileIndexItem> listOfOldFileIndexItems)
        {
            foreach (var item in listOfOldFileIndexItems)
            {
                var oldHashCode = item.FileHash;
                var newHashCode = FileHash.GetHashCode(_appSettings.DatabasePathToFilePath(item.FilePath));
                RenameThumb(oldHashCode,newHashCode);
            }
        }


        /// <summary>
        /// Create a new thumbnail, used by Thumb.dir / web ui
        /// </summary>
        /// <param name="item">source item with Filepath and hash to create a new thumbnail</param>
        /// <returns>true, if succesfull</returns>
        /// <exception cref="FileNotFoundException">Filepath and hash are missing in source item</exception>
        public bool CreateThumb(FileIndexItem item)
        {
            if(string.IsNullOrEmpty(item.FilePath) || string.IsNullOrEmpty(item.FileHash)) 
	            throw new FileNotFoundException("FilePath or FileHash == null");
            return CreateThumb(item.FilePath, item.FileHash);
        }

        /// <summary>
        /// Get the fullpath of the thumbnail
        /// </summary>
        /// <param name="fileHash">filehash of the file</param>
        /// <returns>string with fullpath</returns>
        /// <exception cref="FileLoadException">use with appsettings, to set a ThumbnailTempFolder</exception>
        public string GetThumbnailPath(string fileHash)
        {
            if (_appSettings == null) throw new FileLoadException("use with appsettings, to set a ThumbnailTempFolder"); 
            return _appSettings.ThumbnailTempFolder + fileHash + ".jpg"; //<<full
        }
        
        /// <summary>
        /// Create a Thumbnail file to load it faster in the UI. Use FileIndexItem or database style path, Feature used by the cli tool
        /// </summary>
        /// <param name="subpath">relative path to find the file in the storage folder</param>
        /// <param name="fileHash">the base32 hash of the subpath file</param>
        /// <returns>true, if succesfull</returns>
        /// <exception cref="FileNotFoundException">use with appsettings, to set a ThumbnailTempFolder</exception>
        public bool CreateThumb(string subpath = "/", string fileHash = null)
        {
            if (!Directory.Exists(_appSettings.ThumbnailTempFolder))
            {
                throw new FileNotFoundException("ThumbnailTempFolder not found " 
                                                + _appSettings.ThumbnailTempFolder);
            }
            
            var fullFilePath = _appSettings.DatabasePathToFilePath(subpath);
            
            if (Files.IsFolderOrFile(fullFilePath) 
                == FolderOrFileModel.FolderOrFileTypeList.File)
            {
                
                // Add addional check for raw/tiff based files, those are not supported by this helper 
//                if(!Files.ExtensionThumbSupportedList.Contains(Path.GetExtension(fullFilePath).Replace(".",string.Empty).ToLower()))

                if(!Files.IsExtensionThumbnailSupported(fullFilePath))
                {
                    Console.WriteLine("File not supported (and ignored) > " + fullFilePath );
                    return false; // creating is not succesfull
                }

                if(fileHash == null) fileHash = FileHash.GetHashCode(fullFilePath);
                var thumbPath = GetThumbnailPath(fileHash); //<<full
                
                if (Files.IsFolderOrFile(thumbPath) == FolderOrFileModel.FolderOrFileTypeList.File)
                {
                    Console.WriteLine("The file " + thumbPath + " already exists.");
                    return true; // creating is succesfull; already exist
                }

	            if( new PlainTextFileHelper().ReadFile(GetErrorLogItemFullPath(fullFilePath)).Contains("Thumbnail error") )
		            return false;
	            // File is already tested
         

                // Wrapper to check if the thumbservice is not waiting forever
                // In some scenarios thumbservice is waiting for days
                // Need to add var => else it will not await
                var isSuccesResult = ResizeThumbnailTimeoutWrap(fullFilePath,thumbPath).Result;
                if(isSuccesResult) Console.WriteLine(".");
                RemoveCorruptImage(thumbPath);
                // Log the corrupt image
				if ( !isSuccesResult )
				{
					new PlainTextFileHelper().WriteFile(GetErrorLogItemFullPath(fullFilePath),"Thumbnail error");
				}
                
                return isSuccesResult;
            }
            return false; // not succesfull
        }



        /// <summary>
        /// Wrapper to Make a sync task sync
        /// </summary>
        /// <param name="fullSourceImage">full path to source</param>
        /// <param name="thumbPath">outputpath (full)</param>
        /// <returns>async true, is succesfull</returns>
        private async Task<bool> ResizeThumbnailTimeoutWrap(string fullSourceImage, string thumbPath)
        {
            //adding .ConfigureAwait(false) may NOT be what you want but google it.
            return await Task.Run(() => ResizeThumbnailTimeOut(fullSourceImage, thumbPath)).ConfigureAwait(false);
        }


        /// <summary>
        /// Timeout feature to check if the service is answering within 8 seconds, Ignore Error CS1998
        /// </summary>
        /// <param name="fullSourceImage">full path to source</param>
        /// <param name="thumbPath">outputpath (full)</param>
        /// <returns>async true, is succesfull</returns>
        #pragma warning disable 1998
        private async Task<bool> ResizeThumbnailTimeOut(string fullSourceImage, string thumbPath){
        #pragma warning restore 1998
            
            var task = Task.Run(() => ResizeThumbnailPlain(fullSourceImage, thumbPath));
            if (task.Wait(TimeSpan.FromSeconds(120))) 
                return task.Result;

            Console.WriteLine(">>>>>>>>>>>            Timeout ThumbService "
                              + fullSourceImage 
                              + "            <<<<<<<<<<<<");
              
            return false;
        }
        
        /// <summary>
        /// Resize the thumbnail without timer, Is successfull? // private feature
        /// </summary>
        /// <param name="fullSourceImage">full path to source</param>
        /// <param name="thumbPath">outputpath (full)</param>
        /// <returns>true, is succesfull</returns>
        private bool ResizeThumbnailPlain(string fullSourceImage, string thumbPath)
        {
            Console.WriteLine("fullSourceImage >> " + fullSourceImage + " " + thumbPath);
            try
            {
                // resize the image and save it to the output stream
                using (var outputStream = new FileStream(thumbPath, FileMode.CreateNew))
                using (var inputStream = File.OpenRead(fullSourceImage))
                using (var image = Image.Load(inputStream))
                {
                    // Add orginal rotation to the image as json
                    if (image.MetaData.ExifProfile != null)
                    {
                        image.MetaData.ExifProfile.SetValue(ExifTag.Software, _appSettings.Name + " ");
                    }
                    
                    image.Mutate(x => x.AutoOrient());
                    image.Mutate(x => x
                        .Resize(1000, 0)
                    );

                    image.Save(outputStream,new JpegEncoder{Quality = 90, IgnoreMetadata = false});
                }

                new ExifToolCmdHelper(_appSettings, _exiftool).CopyExif(fullSourceImage, thumbPath);

            }
            catch (Exception ex)            
            {
                 if (!(ex is ImageFormatException) && !(ex is ArgumentException)) throw;
                Console.WriteLine(ex);
                return false;
            }
            
            return true;
        }

        
        /// <summary>
        /// Resize the thumbnail to object
        /// </summary>
        /// <param name="fullSourceImage">full filepath</param>
        /// <param name="width">the width of the output image</param>
        /// <param name="height">use 0 to keep ratio</param>
        /// <param name="quality">only for jpeg, value 0 - 100</param>
        /// <param name="removeExif">dont store exif in output memorystream</param>
        /// <param name="imageFormat">jpeg, or png in Enum</param>
        /// <returns>MemoryStream with resized image</returns>
        public MemoryStream ResizeThumbnailToStream(string fullSourceImage, int width, 
            int height = 0, int quality = 75, bool removeExif = false, 
            Files.ImageFormat imageFormat = Files.ImageFormat.jpg)
        {
            var outputStream = new MemoryStream();
            try
            {
                // resize the image and save it to the output stream
                using (var inputStream = File.OpenRead(fullSourceImage))
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
        private void ResizeThumbnailImageFormat(Image<Rgba32> image, Files.ImageFormat imageFormat, 
	        MemoryStream outputStream, bool removeExif = false, int quality = 75)
        {
            if (imageFormat == Files.ImageFormat.png)
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
        /// Check if the image has the right first bytes, if not remove
        /// </summary>
        /// <param name="thumbPath">the fullfile path of the file</param>
        private void RemoveCorruptImage(string thumbPath)
        {
            if (!File.Exists(thumbPath)) return;
            
            var imageFormat = Files.GetImageFormat(thumbPath);
           
            if(_appSettings.Verbose) Console.WriteLine(Files.GetImageFormat(thumbPath));
            switch (imageFormat)
            {
                case Files.ImageFormat.jpg:
                    return;
                case Files.ImageFormat.unknown:
                    try
                    {
                        File.Delete(thumbPath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Rotate an image, by rotating the pixels and resize the thumbnail.Please do not apply any orientation exiftag on this file
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="orientation">-1 > Rotage -90degrees, anything else 90 degrees</param>
        /// <param name="width">to resize, default 1000</param>
        /// <param name="height">to resize, default keep ratio (0)</param>
        /// <returns>Is successfull? // private feature</returns>
        public bool RotateThumbnail(string fullPath, int orientation, int width = 1000, int height = 0 )
        {
            if (!File.Exists(fullPath)) return false;

            // the orientation is -1 or 1
            var rotateMode = RotateMode.Rotate90;
            if (orientation == -1) rotateMode = RotateMode.Rotate270; 

            try
            {
                using (Image<Rgba32> image = Image.Load(fullPath))
                {
                    image.Mutate(x => x
                        .Resize(width, height)
                    );
                    image.Mutate(x => x
                        .Rotate(rotateMode));
                    image.Save(fullPath); // Automatic encoder selected based on extension.
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

	    private const string ThumbnailPrefix = "_";

	    private string GetErrorLogItemFullPath(string inputFullFilePath)
		{
			return Path.GetDirectoryName(inputFullFilePath) 
				+ Path.DirectorySeparatorChar 
				+ ThumbnailPrefix
				+ Path.GetFileNameWithoutExtension(inputFullFilePath)
				+ ".log";
		}

    }
}