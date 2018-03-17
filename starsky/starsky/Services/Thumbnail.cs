using System;
using System.IO;
using starsky.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Helpers;

namespace starsky.Services
{
    public static class Thumbnail
    {

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


            //FileStream stream = new FileStream(
            //    thumbPath,
            //    System.IO.FileMode.Create);


            //using (Image<Rgba32> image = Image.Load(Files.PathToFull(item.FilePath)))
            //{
            //    image.Mutate(x => x.AutoOrient());
            //    image.Mutate(x => x
            //        .Resize(1000, 0)
            //    );
            //    image.SaveAsJpeg(stream);
            //    image.Dispose();
            //}


            //try
            //{
            //    CreateThumb(stream, item);
            //}
            //finally
            //{
            //    stream.Close();
            //    stream.Dispose();
            //    Console.WriteLine("%");
            //}
        }

    }
}
