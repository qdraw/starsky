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
        public void CreateThumb(string dbFilePath = "/")
        {
            
            var fullFilePath = _appSettings.DatabasePathToFilePath(dbFilePath);;

            var fileName = dbFilePath.Split("/").LastOrDefault();

            if (Files.IsFolderOrFile(fullFilePath) 
                == FolderOrFileModel.FolderOrFileTypeList.File)
            {

                var value = new FileIndexItem()
                {
                    FileName = fileName,
                    FileHash = FileHash.GetHashCode(fullFilePath)
                };
                CreateThumb(value);

            }
        }

        // Create a new thumbnail
        public void CreateThumb(FileIndexItem item)
        {
            if (!Directory.Exists(_appSettings.ThumbnailTempFolder))
            {
                throw new FileNotFoundException("ThumbnailTempFolder not found " 
                                                + _appSettings.ThumbnailTempFolder);
            }

            if(string.IsNullOrWhiteSpace(item.FileHash)) throw 
                new FileNotFoundException("(CreateThumb) FileHash is null " 
                                          + _appSettings.ThumbnailTempFolder);
            
            var thumbPath = _appSettings.ThumbnailTempFolder + item.FileHash + ".jpg";
//<<full
            if (!File.Exists(_appSettings.DatabasePathToFilePath(item.FilePath)))
            {
                Console.WriteLine("File Not found: " + item.FilePath);
                return;
            }

            Console.WriteLine("thumbPath" + thumbPath);
            Console.WriteLine("item.FilePath " + item.FilePath);
            Console.WriteLine("_appSettings.DatabasePathToFilePath(item.FilePath))" + _appSettings.DatabasePathToFilePath(item.FilePath));
            
            
            // If contains error with thumbnailing service then => skip
//            if (!_isErrorItem(_appSettings.DatabasePathToFilePath(item.FilePath))) return;
            
            
            // Return if thumnail already exist
            if (File.Exists(thumbPath)) return;
            
            // Wrapper to check if the thumbservice is not waiting forever
            // In some scenarios thumbservice is waiting for days
            // Need to add var => else it will not await
            var wrap = WrapSomeMethod(item.FilePath,thumbPath).Result;
            if(wrap) Console.WriteLine(".");
            
            _removeCorruptImage(thumbPath);

        }

        private void _removeCorruptImage(string thumbPath)
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
        
        // Wrapper to Make a sync task sync
        private async Task<bool> WrapSomeMethod(string someParam, string someParam2)
        {
            //adding .ConfigureAwait(false) may NOT be what you want but google it.
            return await Task.Run(() => ResizeThumbnailTimeOut(someParam, someParam2)).ConfigureAwait(false);
        }
        
        // Timeout feature to check if the service is answering within 8 seconds
        // Ignore Error CS1998
        #pragma warning disable 1998
        private async Task<bool> ResizeThumbnailTimeOut(string inputDatabaseFilePath, string thumbPath){
        #pragma warning restore 1998
            
            var task = Task.Run(() => ResizeThumbnail(inputDatabaseFilePath, thumbPath));
            if (task.Wait(TimeSpan.FromSeconds(100))) 
                return task.Result;

            Console.WriteLine(">>>>>>>>>>>            Timeout ThumbService "
                              + inputDatabaseFilePath 
                              + "            <<<<<<<<<<<<");
            
            // Log the corrupt image
//            CreateErrorLogItem(inputDatabaseFilePath);
            
            return false;
        }
        
        // Resize the thumbnail
        private bool ResizeThumbnail(string inputDbPath, string thumbPath)
        {
            Console.WriteLine("inputFilePath >> " + inputDbPath);
            
            // resize the image and save it to the output stream
            using (var outputStream = new FileStream(thumbPath, FileMode.CreateNew))
            using (var inputStream = File.OpenRead(_appSettings.DatabasePathToFilePath(inputDbPath)))
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