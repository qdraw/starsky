using System;
using System.IO;
using starsky.Models;
using SixLabors.ImageSharp;

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

            // resize the image and save it to the output stream
            using (var outputStream = new FileStream(thumbPath, FileMode.CreateNew))
            using (var inputStream = File.OpenRead(FileIndexItem.DatabasePathToFilePath(item.FilePath)))
            using (var image = Image.Load(inputStream))
            {
                image.Mutate(x => x.AutoOrient());
                image.Mutate(x => x
                    .Resize(1000, 0)
                );
                image.SaveAsJpeg(outputStream);
            }
        }

    }
}
