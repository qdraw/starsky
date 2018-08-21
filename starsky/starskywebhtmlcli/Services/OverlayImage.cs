using System.IO;
using starsky.Helpers;
using starsky.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace starskywebhtmlcli.Services
{
    public class OverlayImage
    {
        private AppSettings _appSettings;

        public OverlayImage(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        public void OverlayImageNow()
        {
            var sourceFilePath = "/Users/dionvanvelde/Desktop/Werk/test/import/20180718_075539_mbp.jpg";

            foreach (var profile in _appSettings.PublishProfiles)
            {
                if (Files.IsFolderOrFile(profile.OverlayFullPath) 
                    != FolderOrFileModel.FolderOrFileTypeList.File) 
                    throw new FileNotFoundException("ImageOverlayFullPath " + profile.OverlayFullPath);

                if (Files.IsFolderOrFile(sourceFilePath) 
                    != FolderOrFileModel.FolderOrFileTypeList.File) 
                    throw new FileNotFoundException("ImageOverlayFullPath " + sourceFilePath);

                var outputFile = Path.Combine(Path.GetDirectoryName(sourceFilePath),
                    profile.Folder,
                    Path.GetFileNameWithoutExtension(sourceFilePath)+ profile.Append + Path.GetExtension(sourceFilePath));
                
                using (var outputStream = new FileStream(outputFile, FileMode.CreateNew))
                using (var inputStream = File.OpenRead(sourceFilePath))
                using (var overlayLogoStream = File.OpenRead(profile.OverlayFullPath))
                using (var image = Image.Load(inputStream))
                using (var overlayLogo = Image.Load(overlayLogoStream))
                {
                    image.Mutate(x => x
                        .Resize(profile.SourceMaxWidth, 0)
                    );
                
                    overlayLogo.Mutate(x => x
                        .Resize(profile.OverlayMaxWidth, 0)
                    );

                    int xPoint = image.Width - overlayLogo.Width;
                    int yPoint = image.Height - overlayLogo.Height;
                    image.Mutate(x => x.DrawImage(overlayLogo, PixelBlenderMode.Normal, 1F, new Point(xPoint, yPoint)));

                    image.SaveAsJpeg(outputStream);
                }
            }
            
                
                
            

         }
    }
}