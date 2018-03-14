using System;
using System.IO;
using starsky.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Helpers;

namespace starsky.Services
{
    public static class Thumbnail
    {
        public static FileIndexItem CreateThumb(FileStream fs, FileIndexItem item)
        {
            using (Image<Rgba32> image = Image.Load(Files.PathToFull(item.FilePath)))
            {
                image.Mutate(x => x.AutoOrient());
                image.Mutate(x => x
                    .Resize(1000, 0)
                );
                image.SaveAsJpeg(fs);
                //image.Save(savePath); // automatic encoder selected based on extension.
                //image.Dispose();
                Console.Write("%");

            }

            return item;
        }

        public static FileIndexItem CreateThumb(FileIndexItem item)
        {
            if (!System.IO.Directory.Exists(AppSettingsProvider.ThumbnailTempFolder))
            {
                throw new FileNotFoundException("ThumbnailTempFolder not found "+ AppSettingsProvider.ThumbnailTempFolder);
            }

            var thumbPath = AppSettingsProvider.ThumbnailTempFolder + item.FileHash + ".jpg";

            if (!System.IO.File.Exists(Files.PathToFull(item.FilePath)))
            {
                Console.WriteLine("File Not found: " + item.FilePath);
                return null;
            }

                


            if (System.IO.File.Exists(thumbPath))
            {
                return null;
            }


            FileStream stream = new FileStream(
                thumbPath,
                System.IO.FileMode.Create);

            try
            {
                return CreateThumb(stream, item ); ;
            }
            finally
            {
                stream.Close();
            }
        }

    }
}
