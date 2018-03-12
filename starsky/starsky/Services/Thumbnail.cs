using starsky.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Helpers;

namespace starsky.Services
{
    public class Thumbnail
    {
        public void CreateThumb(FileIndexItem item)
        {
            using (Image<Rgba32> image = Image.Load(Files.PathToFull(item.FilePath)))
            {
                image.Mutate(x => x
                    .Resize(1024, 1024)
                    .Grayscale());
                var savePath = AppSettingsProvider.ThumbnailTempFolder + item.FileHash;
                image.Save(savePath); // automatic encoder selected based on extension.
            }
        }
    
    }
}
