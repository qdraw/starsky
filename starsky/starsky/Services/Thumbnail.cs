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
        
        
        
        
        // Feature used by the cli tool
        // Use FileIndexItem or database style path
        public void CreateThumb(string subpath = "/", string fileHash = null)
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

                if(fileHash == null) fileHash = FileHash.GetHashCode(fullFilePath);
                var thumbPath = _appSettings.ThumbnailTempFolder + fileHash + ".jpg"; //<<full
                if (Files.IsFolderOrFile(thumbPath) == FolderOrFileModel.FolderOrFileTypeList.File)
                {
                    Console.WriteLine("The file " + thumbPath + " already exists.");
                    return;
                }

                if (!_isErrorItem(_appSettings.DatabasePathToFilePath(subpath))) return;

                // Wrapper to check if the thumbservice is not waiting forever
                // In some scenarios thumbservice is waiting for days
                // Need to add var => else it will not await
                var wrap = ResizeThumbnailTimeoutWrap(fullFilePath,thumbPath).Result;
                if(wrap) Console.WriteLine(".");
                RemoveCorruptImage(thumbPath);
            }
        }

        // Create a new thumbnail
        public void CreateThumb(FileIndexItem item)
        {
            if(string.IsNullOrEmpty(item.FilePath) || string.IsNullOrEmpty(item.FileHash)) throw new FileNotFoundException("FilePath or FileHash == null");
            CreateThumb(item.FilePath, item.FileHash);
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
            if (task.Wait(TimeSpan.FromSeconds(100))) 
                return task.Result;

            Console.WriteLine(">>>>>>>>>>>            Timeout ThumbService "
                              + fullSourceImage 
                              + "            <<<<<<<<<<<<");
            
            // Log the corrupt image
//            CreateErrorLogItem(inputDatabaseFilePath);
            
            return false;
        }
        
        // Resize the thumbnail
        private bool ResizeThumbnailPlain(string fullSourceImage, string thumbPath)
        {
            Console.WriteLine("fullSourceImage >> " + fullSourceImage);
            
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
            return false;
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

        public void CreateErrorLogItem(string inputDatabaseFilePath)
        {
            var path = _GetErrorLogItemFullPath(inputDatabaseFilePath);
            if (File.Exists(path)) return;
            
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path)) 
            {
                sw.WriteLine(_thumbnailErrorMessage);
            }
        }
        
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