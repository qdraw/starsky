using System;
using System.IO;
using starsky.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Helpers;

namespace starsky.Services
{
    public static class Thumbnail
    {
        public static FileIndexItem CreateThumb(FileIndexItem item)
        {
            if(!File.Exists(item.FilePath)) { 
                using (Image<Rgba32> image = Image.Load(Files.PathToFull(item.FilePath)))
                {
                    image.Mutate(x => x
                        .Resize(900,0)
                    );
                    var savePath = AppSettingsProvider.ThumbnailTempFolder + item.FileHash + ".jpg";
                    image.Save(savePath); // automatic encoder selected based on extension.
                    image.Dispose();
                }
            }

            return item;
        }
    
    }
}
