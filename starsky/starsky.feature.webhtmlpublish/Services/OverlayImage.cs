using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starskywebhtmlcli.Services
{
    public class OverlayImage
    {
        private readonly AppSettings _appSettings;
        private readonly IStorage _thumbnailStorage;
	    private readonly IStorage _iStorage;
	    private readonly IStorage _hostFileSystem;

	    public OverlayImage(ISelectorStorage selectorStorage, AppSettings appSettings)
        {
	        _iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
	        _thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
	        _hostFileSystem = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
            _appSettings = appSettings;
        }

        public string FilePathOverlayImage(string sourceFilePath, AppSettingsPublishProfiles profile)
        {
            var outputFilePath = 
	            profile.Folder + _appSettings.GenerateSlug( Path.GetFileNameWithoutExtension(sourceFilePath),true )
	                           + profile.Append + Path.GetExtension(sourceFilePath);
	        
            return outputFilePath;
        }

        
        public void ResizeOverlayImageThumbnails(string fileHash, string outputSubPath, AppSettingsPublishProfiles profile)
        {
	        if ( !_thumbnailStorage.ExistFile(fileHash) ) throw new FileNotFoundException("fileHash " + fileHash);

	        if ( _iStorage.ExistFile(outputSubPath)  ) return;
	        

	        using ( var sourceImageStream = _thumbnailStorage.ReadStream(fileHash))
	        using ( var sourceImage = Image.Load(sourceImageStream) )
	        using ( var overlayImageStream = _hostFileSystem.ReadStream(profile.Path))
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
    
    }
}
