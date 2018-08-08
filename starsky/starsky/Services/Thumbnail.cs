using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.Helpers;
using starsky.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;

namespace starsky.Services
{
    public class Thumbnail
    {
        private readonly AppSettings _appSettings;

        public Thumbnail(AppSettings appSettings)
        {
            _appSettings = appSettings;
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
                if(!Files.ExtensionThumbSupportedList.Contains(Path.GetExtension(fullFilePath).Replace(".",string.Empty)))
                {
                    Console.WriteLine("File not supported (and ignored) > " + fullFilePath );
                    return false; // creating is not succesfull
                }

                if(fileHash == null) fileHash = FileHash.GetHashCode(fullFilePath);
                var thumbPath = _appSettings.ThumbnailTempFolder + fileHash + ".jpg"; //<<full
                if (Files.IsFolderOrFile(thumbPath) == FolderOrFileModel.FolderOrFileTypeList.File)
                {
                    Console.WriteLine("The file " + thumbPath + " already exists.");
                    return true; // creating is succesfull; already exist
                }

                if (!_isErrorItem(_appSettings.DatabasePathToFilePath(subpath))) return false;
                // File is already tested
                

                // Wrapper to check if the thumbservice is not waiting forever
                // In some scenarios thumbservice is waiting for days
                // Need to add var => else it will not await
                var isSuccesResult = ResizeThumbnailTimeoutWrap(fullFilePath,thumbPath).Result;
                if(isSuccesResult) {Console.WriteLine(".");}
                RemoveCorruptImage(thumbPath);
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
            
            // Log the corrupt image
            CreateErrorLogItem(fullSourceImage);
            
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
                    image.Mutate(x => x.AutoOrient());
                    image.Mutate(x => x
                        .Resize(1000, 0)
                    );
                    image.SaveAsJpeg(outputStream);
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
        
        
        private static readonly string _thumbnailErrorMessage = "Thumbnail error";
        private static readonly string _thumbnailPrefix = "_";
        private static readonly string _thumbnailSuffix = "_starksy-error.log";

//        // todo: replace this code with something good
        
        private string _GetErrorLogItemFullPath(string inputDatabaseFilePath)
        {
            var parentDatabaseFolder = Breadcrumbs.BreadcrumbHelper(inputDatabaseFilePath).LastOrDefault();
            var fileName = inputDatabaseFilePath.Replace(parentDatabaseFolder, "");
            fileName = fileName.Replace(".jpg", _thumbnailSuffix);
            fileName = fileName.Replace("/", "");
            var logFileDatabasePath = parentDatabaseFolder + "/" + _thumbnailPrefix + fileName;

            
            var logFile = _appSettings.DatabasePathToFilePath(logFileDatabasePath,false);
            return logFile;
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
            var path = _GetErrorLogItemFullPath(inputDatabaseFilePath);
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