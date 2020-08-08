using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.webhtmlpublish.Services
{
	[Service(typeof(IOverlayImage), InjectionLifetime = InjectionLifetime.Scoped)]
    public class OverlayImage : IOverlayImage
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
		    return profile.Folder + _appSettings.GenerateSlug( 
			                                        Path.GetFileNameWithoutExtension(sourceFilePath),true )
		                                        + profile.Append + Path.GetExtension(sourceFilePath);
	    }
	    
        public string FilePathOverlayImage(string outputParentFullFilePathFolder, string sourceFilePath, AppSettingsPublishProfiles profile)
        {
            return PathHelper.AddBackslash(outputParentFullFilePathFolder)  +
                                 FilePathOverlayImage(sourceFilePath,profile);
        }
        
        public void ResizeOverlayImageThumbnails(string fileHash, string outputFullFilePath, AppSettingsPublishProfiles profile)
        {
	        if ( string.IsNullOrWhiteSpace(fileHash) ) throw new ArgumentNullException(nameof(fileHash));
	        if ( !_thumbnailStorage.ExistFile(fileHash) ) throw new FileNotFoundException("fileHash " + fileHash);

	        if ( _hostFileSystem.ExistFile(outputFullFilePath)  ) return;
	        
	        using ( var sourceImageStream = _thumbnailStorage.ReadStream(fileHash))
	        using ( var sourceImage = Image.Load(sourceImageStream) )
	        using ( var overlayImageStream = _hostFileSystem.ReadStream(profile.Path)) // for example a logo
	        using ( var overlayImage = Image.Load(overlayImageStream) )
	        using ( var outputStream  = new MemoryStream() )
	        {
		        var result = ResizeOverlayImageShared(sourceImage, overlayImage, outputStream, profile,
			        outputFullFilePath);
		        _hostFileSystem.WriteStream(result, outputFullFilePath);
		        sourceImageStream.Dispose();
	        }
        }
	    
        /// <summary>
        /// Read from _iStorage to _hostFileSystem
        /// </summary>
        /// <param name="subPath">input Image</param>
        /// <param name="outputFullFilePath">location where to store</param>
        /// <param name="profile">image profile that contains sizes</param>
        /// <exception cref="FileNotFoundException">source image not found</exception>
	    public void ResizeOverlayImageLarge(string subPath, string outputFullFilePath, AppSettingsPublishProfiles profile)
	    {
		    if ( !_iStorage.ExistFile(subPath) ) throw new FileNotFoundException("subPath " + subPath);

		    if ( _hostFileSystem.ExistFile(outputFullFilePath)  ) return;
	        
		    using ( var sourceImageStream = _iStorage.ReadStream(subPath))
		    using ( var sourceImage = Image.Load(sourceImageStream) )
		    using ( var overlayImageStream = _hostFileSystem.ReadStream(profile.Path))
		    using ( var overlayImage = Image.Load(overlayImageStream) )
		    using ( var outputStream  = new MemoryStream() )
		    {
			    var result = ResizeOverlayImageShared(sourceImage, overlayImage, outputStream, profile,
				    outputFullFilePath);
			    _hostFileSystem.WriteStream(result, outputFullFilePath);
		    }
	    }

	    private Stream ResizeOverlayImageShared(Image<Rgba32> sourceImage, Image<Rgba32> overlayImage,
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
		    sourceImage.Mutate(x => x.DrawImage(overlayImage, 
			    PixelBlenderMode.Normal, 1F, new Point(xPoint, yPoint)));

		    sourceImage.SaveAsJpeg(outputStream);
		    return outputStream;
	    }
    }
}
