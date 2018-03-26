using System;
using System.IO;
using System.Threading.Tasks;
using starsky.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace starsky.Services
{
    public class Thumbnail
    {
        public void RenameThumb(string oldHashCode, string newHashCode)
        {
            if (!System.IO.Directory.Exists(AppSettingsProvider.ThumbnailTempFolder))
            {
                throw new FileNotFoundException("ThumbnailTempFolder not found " + AppSettingsProvider.ThumbnailTempFolder);
            }

            var oldThumbPath = AppSettingsProvider.ThumbnailTempFolder + oldHashCode + ".jpg";
            var newThumbPath = AppSettingsProvider.ThumbnailTempFolder + newHashCode + ".jpg";

            if (!System.IO.File.Exists(oldThumbPath))
            {
                return;
            }

            System.IO.File.Move(oldThumbPath, newThumbPath);

        }


        public static void CreateThumb(FileIndexItem item)
        {
            if (!System.IO.Directory.Exists(AppSettingsProvider.ThumbnailTempFolder))
            {
                throw new FileNotFoundException("ThumbnailTempFolder not found " + AppSettingsProvider.ThumbnailTempFolder);
            }

            var thumbPath = AppSettingsProvider.ThumbnailTempFolder + item.FileHash + ".jpg";

            if (!System.IO.File.Exists(FileIndexItem.DatabasePathToFilePath(item.FilePath)))
            {
                Console.WriteLine("File Not found: " + item.FilePath);
                return;
            }

            if (System.IO.File.Exists(thumbPath))
            {
                return;
            }
            
            // Need to add var => else it will not await
            var q = WrapSomeMethod(item.FilePath,thumbPath).Result;

        }
        
        private static async Task<bool> WrapSomeMethod(string someParam, string someParam2)
        {
            //adding .ConfigureAwait(false) may NOT be what you want but google it.
            return await Task.Run(() => ResizeThumbnailTimeOut(someParam, someParam2)).ConfigureAwait(false);
        }
        
        // Ignore Error CS1998
        #pragma warning disable 1998
        private static async Task<bool> ResizeThumbnailTimeOut(string inputFilePath, string thumbPath){
        #pragma warning restore 1998

            var task = Task.Run(() => resizeThumbnail(inputFilePath, thumbPath));
            if (task.Wait(TimeSpan.FromSeconds(8)))
                return task.Result;

            Console.WriteLine(">>>>>>>>>>>            Timeout ThumbService "
                              + inputFilePath 
                              + "            <<<<<<<<<<<<");
            return false;
        }
        
//        private static async Task<bool> ResizeAsync(string source, string destination)
//        {
//
//            using (FileStream filestream = new FileStream(destination,
//                FileMode.Append, FileAccess.Write, FileShare.None,
//                bufferSize: 4096, useAsync:true))
//            using (var memoryStream = new MemoryStream())
//            using (FileStream sourceStream = new FileStream(source,
//                FileMode.Append, FileAccess.Write, FileShare.None,
//                bufferSize: 4096, useAsync: false))
//            {
//
//                var image = Image.Load(sourceStream);
//
//                image.Mutate(x => x.AutoOrient());
//                image.Mutate(x => x
//                    .Resize(1000, 0)
//                );
//                image.SaveAsJpeg(memoryStream);
//                
//                await memoryStream.CopyToAsync(filestream);
//
//                return true;
//            }
//        }
        
        private static bool resizeThumbnail(string inputFilePath, string thumbPath)
        {
            // resize the image and save it to the output stream
            using (var outputStream = new FileStream(thumbPath, FileMode.CreateNew))
            using (var inputStream = File.OpenRead(FileIndexItem.DatabasePathToFilePath(inputFilePath)))
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
        

    }
}
