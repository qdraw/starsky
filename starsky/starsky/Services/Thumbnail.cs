using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;
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
        
        // Rename a thumbnail, used when you change exifdata,
        // the hash is also changed but the images remain the same
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
        
        
        // Create a new thumbnail
        // used by Thumb.dir / web ui
        public bool CreateThumb(FileIndexItem item)
        {
            if(string.IsNullOrEmpty(item.FilePath) || string.IsNullOrEmpty(item.FileHash)) throw new FileNotFoundException("FilePath or FileHash == null");
            return CreateThumb(item.FilePath, item.FileHash);
        }

        public string GetThumbnailPath(string fileHash)
        {
            if (_appSettings == null) throw new FileLoadException("add appsettings first"); 
            return _appSettings.ThumbnailTempFolder + fileHash + ".jpg"; //<<full
        }
        
        // Feature used by the cli tool
        // Use FileIndexItem or database style path
        public bool CreateThumb(string subpath = "/", string fileHash = null)
        {
            if (!Directory.Exists(_appSettings.ThumbnailTempFolder))
            {
                throw new FileNotFoundException("ThumbnailTempFolder not found " 
                                                + _appSettings.ThumbnailTempFolder);
            }
            
            var fullFilePath = _appSettings.DatabasePathToFilePath(subpath);;
            
            if (Files.IsFolderOrFile(fullFilePath) 
                == FolderOrFileModel.FolderOrFileTypeList.File)
            {
                
                // Add addional check for raw/tiff based files, those are not supported by this helper 
                if(!Files.ExtensionThumbSupportedList.Contains(Path.GetExtension(fullFilePath).Replace(".",string.Empty).ToLower()))
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

                if (!_isErrorItem(GetErrorLogItemFullPath(fullFilePath))) return false;
                // File is already tested
                

                // Wrapper to check if the thumbservice is not waiting forever
                // In some scenarios thumbservice is waiting for days
                // Need to add var => else it will not await
                var isSuccesResult = ResizeThumbnailTimeoutWrap(fullFilePath,thumbPath).Result;
                if(isSuccesResult) Console.WriteLine(".");
                RemoveCorruptImage(thumbPath);
                // Log the corrupt image
                if(!isSuccesResult) CreateErrorLogItem(GetErrorLogItemFullPath(fullFilePath));
                
                return isSuccesResult;
            }
            return false; // not succesfull
        }



        // Wrapper to Make a sync task sync
        private async Task<bool> ResizeThumbnailTimeoutWrap(string fullSourceImage, string thumbPath)
        {
            //adding .ConfigureAwait(false) may NOT be what you want but google it.
            return await Task.Run(() => ResizeThumbnailTimeOut(fullSourceImage, thumbPath)).ConfigureAwait(false);
        }
        
        // Timeout feature to check if the service is answering within 8 seconds
        // Ignore Error CS1998
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
        
        // Resize the thumbnail
        // Is successfull? // private feature
        public bool ResizeThumbnailPlain(string fullSourceImage, string thumbPath)
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

        
        // Resize the thumbnail to object
        // quality = value 0,100
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
                    if (image.MetaData.ExifProfile != null && removeExif == false)
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
        private void ResizeThumbnailImageFormat(Image<Rgba32> image, Files.ImageFormat imageFormat, MemoryStream outputStream, bool removeExif = false, int quality = 75)
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
        
        // Resize the thumbnail
        // Is successfull? // private feature
        public bool RotateThumbnail(string fullPath, int orientation)
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
                        .Resize(1000, 0)
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
        
        
        
        
        
        private static readonly string _thumbnailErrorMessage = "Thumbnail error";
        private static readonly string _thumbnailPrefix = "_";
        private static readonly string _thumbnailSuffix = "_starksy-error.log";

//        // todo: replace this code with something good
        
        private string GetErrorLogItemFullPath(string inputFullFilePath)
        {
             return Path.GetDirectoryName(inputFullFilePath) 
                  + Path.DirectorySeparatorChar 
                  + _thumbnailPrefix
                  + Path.GetFileNameWithoutExtension(inputFullFilePath)
                  + ".log";
        }

        public void CreateErrorLogItem(string path)
        {
            if (File.Exists(path)) return;
            
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path)) 
            {
                sw.WriteLine(_thumbnailErrorMessage);
            }
        }
        
        // todo: Use PlainTextFileHelper
        private bool _isErrorItem(string inputDatabaseFilePath)
        {
            var path = GetErrorLogItemFullPath(inputDatabaseFilePath);
            if (!File.Exists(path)) return true;
            
            using (StreamReader sr = File.OpenText(path)) 
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    if (s.Contains(_thumbnailErrorMessage)) return false;
                }
            }
            return true;
        }
    }
}