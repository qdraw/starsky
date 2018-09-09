using System.IO;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace starskywebhtmlcli.Services
{
    public class OverlayImage
    {
        private readonly AppSettings _appSettings;
        private readonly IExiftool _exiftool;

        public OverlayImage(AppSettings appSettings, IExiftool exiftool)
        {
            _appSettings = appSettings;
            _exiftool = exiftool;
        }

        public string FilePathOverlayImage(string sourceFilePath, AppSettingsPublishProfiles profile)
        {
            var outputFilePath = Path.Combine(Path.GetDirectoryName(sourceFilePath),
                profile.Folder,
                _appSettings.GenerateSlug( Path.GetFileNameWithoutExtension(sourceFilePath),true ) +
                profile.Append + Path.GetExtension(sourceFilePath));
            return outputFilePath;
        }

        
        public void ResizeOverlayImage(string sourceFilePath, string outputFilePath, AppSettingsPublishProfiles profile)
        {
            if (Files.IsFolderOrFile(profile.Path) //< used for image overlay 
                != FolderOrFileModel.FolderOrFileTypeList.File) 
                throw new FileNotFoundException("ImageOverlayFullPath " + profile.Path);

            if (Files.IsFolderOrFile(sourceFilePath) 
                != FolderOrFileModel.FolderOrFileTypeList.File) 
                throw new FileNotFoundException("sourceFilePath " + sourceFilePath);

            if (Files.IsFolderOrFile(outputFilePath) 
                == FolderOrFileModel.FolderOrFileTypeList.File) return;
                
            using (var outputStream = new FileStream(outputFilePath, FileMode.CreateNew))
            using (var inputStream = File.OpenRead(sourceFilePath))
            using (var overlayLogoStream = File.OpenRead(profile.Path))
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

            if (profile.MetaData)
            {
                new ExifToolCmdHelper(_appSettings,_exiftool).CopyExifPublish(sourceFilePath,outputFilePath);
            }

         }
    }
}