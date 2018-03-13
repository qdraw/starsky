﻿using System;
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
                image.Mutate(x => x
                    .Resize(900, 0)
                );
                var savePath = AppSettingsProvider.ThumbnailTempFolder + item.FileHash + ".jpg";
                image.Save(savePath); // automatic encoder selected based on extension.
                image.Dispose();
                Console.Write("%");

            }

            return item;
        }

        public static FileIndexItem CreateThumb(FileIndexItem item)
        {
            FileStream stream = new FileStream(
                Files.PathToFull(item.FilePath),
                System.IO.FileMode.Open,
                System.IO.FileAccess.Read,
                System.IO.FileShare.ReadWrite);

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
