using System;
using System.IO;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace starskywebhtmlcli.Services
{
    public class OverlayImage
    {
        private readonly AppSettings _appSettings;
        private readonly IExifTool _exifTool;

        public OverlayImage(AppSettings appSettings, IExifTool exifTool)
        {
            _appSettings = appSettings;
            _exifTool = exifTool;
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
            if (FilesHelper.IsFolderOrFile(profile.Path) //< used for image overlay 
                != FolderOrFileModel.FolderOrFileTypeList.File) 
                throw new FileNotFoundException("ImageOverlayFullPath " + profile.Path);

            if (FilesHelper.IsFolderOrFile(sourceFilePath) 
                != FolderOrFileModel.FolderOrFileTypeList.File) 
                throw new FileNotFoundException("sourceFilePath " + sourceFilePath);

            if (FilesHelper.IsFolderOrFile(outputFilePath) 
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

//	            throw new NotImplementedException();
	            // image.Mutate(x => x.DrawImage(overlayLogo, PixelBlenderMode.Normal, 1F, new Point(xPoint, yPoint)));

                image.SaveAsJpeg(outputStream);
            }

            if (profile.MetaData)
            {
	            // todo: check if works
	            var storage = new StorageHostFullPathFilesystem();
	            new ExifCopy(storage, new ExifTool(storage, new AppSettings()),
		            new ReadMeta(storage)).CopyExifPublish(sourceFilePath, outputFilePath);
            }

         }
    }
}