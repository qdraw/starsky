using System;
using System.IO;
using System.Threading.Tasks;
using starsky.Models;
using SixLabors.ImageSharp;

namespace starsky.Services
{
    public class Thumbnail
    {
        // Rename a thumbnail, used when you change exifdata,
        // the hash is also changed but the images remain the same
        public void RenameThumb(string oldHashCode, string newHashCode)
        {
            if (!Directory.Exists(AppSettingsProvider.ThumbnailTempFolder))
            {
                throw new FileNotFoundException("ThumbnailTempFolder not found " + AppSettingsProvider.ThumbnailTempFolder);
            }

            var oldThumbPath = AppSettingsProvider.ThumbnailTempFolder + oldHashCode + ".jpg";
            var newThumbPath = AppSettingsProvider.ThumbnailTempFolder + newHashCode + ".jpg";

            if (!File.Exists(oldThumbPath))
            {
                return;
            }

            File.Move(oldThumbPath, newThumbPath);
        }

        // Create a new thumbnail
        public static void CreateThumb(FileIndexItem item)
        {
            if (!Directory.Exists(AppSettingsProvider.ThumbnailTempFolder))
            {
                throw new FileNotFoundException("ThumbnailTempFolder not found " + AppSettingsProvider.ThumbnailTempFolder);
            }

            var thumbPath = AppSettingsProvider.ThumbnailTempFolder + item.FileHash + ".jpg";

            if (!File.Exists(FileIndexItem.DatabasePathToFilePath(item.FilePath)))
            {
                Console.WriteLine("File Not found: " + item.FilePath);
                return;
            }

            if (File.Exists(thumbPath))
            {
                return;
            }
            
            // Wrapper to check if the thumbservice is not waiting forever
            // In some scenarios thumbservice is waiting for days
            // Need to add var => else it will not await
            var q = WrapSomeMethod(item.FilePath,thumbPath).Result;

        }
        
        // Wrapper to Make a sync task sync
        private static async Task<bool> WrapSomeMethod(string someParam, string someParam2)
        {
            //adding .ConfigureAwait(false) may NOT be what you want but google it.
            return await Task.Run(() => ResizeThumbnailTimeOut(someParam, someParam2)).ConfigureAwait(false);
        }
        
        // Timeout feature to check if the service is answering within 8 seconds
        // Ignore Error CS1998
        #pragma warning disable 1998
        private static async Task<bool> ResizeThumbnailTimeOut(string inputFilePath, string thumbPath){
        #pragma warning restore 1998

            var task = Task.Run(() => ResizeThumbnail(inputFilePath, thumbPath));
            if (task.Wait(TimeSpan.FromSeconds(8)))
                return task.Result;

            Console.WriteLine(">>>>>>>>>>>            Timeout ThumbService "
                              + inputFilePath 
                              + "            <<<<<<<<<<<<");
            return false;
        }
        
        // Resize the thumbnail
        private static bool ResizeThumbnail(string inputFilePath, string thumbPath)
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