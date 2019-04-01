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
	    private IStorage _iStorage;
//	    private IReadMeta _readMeta;

	    public OverlayImage(IStorage iStorage, AppSettings appSettings, IExifTool exifTool)
        {
	        _iStorage = iStorage;
            _appSettings = appSettings;
            _exifTool = exifTool;
//	        _readMeta = readMeta;
        }

        public string FilePathOverlayImage(string sourceFilePath, AppSettingsPublishProfiles profile)
        {
            var outputFilePath = 
	            profile.Folder + _appSettings.GenerateSlug( Path.GetFileNameWithoutExtension(sourceFilePath),true ) + profile.Append + Path.GetExtension(sourceFilePath);
	        
            return outputFilePath;
        }

        
        public void ResizeOverlayImageThumbnails(string fileHash, string outputSubPath, AppSettingsPublishProfiles profile)
        {
	        if ( !_iStorage.ThumbnailExist(fileHash) ) throw new FileNotFoundException("fileHash " + fileHash);

	        if ( _iStorage.ExistFile(outputSubPath)  ) return;
	        
	        // only for overlay image
	        var hostFileSystem = new StorageHostFullPathFilesystem();

	        using ( var sourceImageStream = _iStorage.ThumbnailRead(fileHash))
	        using ( var sourceImage = Image.Load(sourceImageStream) )
	        using ( var overlayImageStream = hostFileSystem.ReadStream(profile.Path))
	        using ( var overlayImage = Image.Load(overlayImageStream) )
	        using ( var outputStream  = new MemoryStream() )
	        {
		        ResizeOverlayImageShared(sourceImage, overlayImage, outputStream, profile,
			        outputSubPath);
	        }

        }
	    
	    public void ResizeOverlayImageLarge(string subPath, string outputSubPath, AppSettingsPublishProfiles profile)
	    {
		    if ( !_iStorage.ExistFile(subPath) ) throw new FileNotFoundException("subPath " + subPath);

		    if ( _iStorage.ExistFile(outputSubPath)  ) return;
	        
		    // only for overlay image
		    var hostFileSystem = new StorageHostFullPathFilesystem();

		    using ( var sourceImageStream = _iStorage.ReadStream(subPath))
		    using ( var sourceImage = Image.Load(sourceImageStream) )
		    using ( var overlayImageStream = hostFileSystem.ReadStream(profile.Path))
		    using ( var overlayImage = Image.Load(overlayImageStream) )
		    using ( var outputStream  = new MemoryStream() )
		    {
			    ResizeOverlayImageShared(sourceImage, overlayImage, outputStream, profile,
				    outputSubPath);
		    }

	    }

	    private void ResizeOverlayImageShared(Image<Rgba32> sourceImage, Image<Rgba32> overlayImage,
		    Stream outputStream, AppSettingsPublishProfiles profile, string outputSubPath)
	    {
		    sourceImage.Mutate(x => x
			    .Resize(profile.SourceMaxWidth, 0)
		    );

		    overlayImage.Mutate(x => x
			    .Resize(profile.OverlayMaxWidth, 0)
		    );

		    int xPoint = sourceImage.Width - overlayImage.Width;
		    int yPoint = sourceImage.Height - overlayImage.Height;
			
		    // For ImageSharp-0006
		    // sourceImage.Mutate(x => x.DrawImage(overlayImage, new Point(xPoint, yPoint), 1F));
		    
		    // For ImageSharp-0005
		    sourceImage.Mutate(x => x.DrawImage(overlayImage, PixelBlenderMode.Normal, 1F, new Point(xPoint, yPoint)));

		    sourceImage.SaveAsJpeg(outputStream);

		    _iStorage.WriteStream(outputStream, outputSubPath);
	    }
	    
	    

	    //            using (var outputStream = new FileStream(outputSubPath, FileMode.CreateNew))
//            using (var inputStream = File.OpenRead(sourceLocation))
//            using (var overlayLogoStream = File.OpenRead(profile.Path))
//            using (var image = Image.Load(inputStream))
//            using (var overlayLogo = Image.Load(overlayLogoStream))
//            {
//                image.Mutate(x => x
//                    .Resize(profile.SourceMaxWidth, 0)
//                );
//                
//                overlayLogo.Mutate(x => x
//                    .Resize(profile.OverlayMaxWidth, 0)
//                );
//
//                int xPoint = image.Width - overlayLogo.Width;
//                int yPoint = image.Height - overlayLogo.Height;
//
//	            image.Mutate(x => x.DrawImage(overlayLogo, new Point(xPoint, yPoint),1F));
//
//                image.SaveAsJpeg(outputStream);
//            }
//
//            if (profile.MetaData)
//            {
//	            // todo: check if works
//	            var storage = new StorageHostFullPathFilesystem();
//	            new ExifCopy(storage, new ExifTool(storage, _appSettings),
//		            new ReadMeta(storage)).CopyExifPublish(sourceLocation, outputSubPath);
//            }
    }
}