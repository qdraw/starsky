using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
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
			_appSettings = appSettings;
			if ( selectorStorage == null ) return;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_hostFileSystem = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		}

		public string FilePathOverlayImage(string sourceFilePath, AppSettingsPublishProfiles profile)
		{
			var result = profile.Folder + _appSettings.GenerateSlug(
				                            Path.GetFileNameWithoutExtension(sourceFilePath), true)
			                            + profile.Append + Path.GetExtension(sourceFilePath).ToLowerInvariant();
			return result;
		}
	    
		public string FilePathOverlayImage(string outputParentFullFilePathFolder, string sourceFilePath, 
			AppSettingsPublishProfiles profile)
		{
			var result = PathHelper.AddBackslash(outputParentFullFilePathFolder) +
			             FilePathOverlayImage(sourceFilePath, profile);
			return result;
		}
        
		public async Task ResizeOverlayImageThumbnails(string itemFileHash, string outputFullFilePath, AppSettingsPublishProfiles profile)
		{
			if ( string.IsNullOrWhiteSpace(itemFileHash) ) throw new ArgumentNullException(nameof(itemFileHash));
			if ( !_thumbnailStorage.ExistFile(itemFileHash) ) throw new FileNotFoundException("fileHash " + itemFileHash);

			if ( _hostFileSystem.ExistFile(outputFullFilePath)  ) return;
			if ( !_hostFileSystem.ExistFile(profile.Path) )
			{
				throw new FileNotFoundException($"overlayImage is missing in profile.Path: {profile.Path}");
			}
	        
			using ( var sourceImageStream = _thumbnailStorage.ReadStream(itemFileHash))
			using ( var sourceImage = await Image.LoadAsync(sourceImageStream) )
			using ( var overlayImageStream = _hostFileSystem.ReadStream(profile.Path)) // for example a logo
			using ( var overlayImage = await Image.LoadAsync(overlayImageStream) )
			using ( var outputStream  = new MemoryStream() )
			{
				await ResizeOverlayImageShared(sourceImage, overlayImage, outputStream, profile,
					outputFullFilePath);
			}
		}
	    
		/// <summary>
		/// Read from _iStorage to _hostFileSystem
		/// </summary>
		/// <param name="itemFilePath">input Image</param>
		/// <param name="outputFullFilePath">location where to store</param>
		/// <param name="profile">image profile that contains sizes</param>
		/// <exception cref="FileNotFoundException">source image not found</exception>
		public async Task ResizeOverlayImageLarge(string itemFilePath, 
			string outputFullFilePath, AppSettingsPublishProfiles profile)
		{
			if ( string.IsNullOrWhiteSpace(itemFilePath) ) throw new 
				ArgumentNullException(nameof(itemFilePath));
			if ( !_iStorage.ExistFile(itemFilePath) ) throw new FileNotFoundException("subPath " + itemFilePath);

			if ( _hostFileSystem.ExistFile(outputFullFilePath)  ) return;
			if ( !_hostFileSystem.ExistFile(profile.Path) )
			{
				throw new FileNotFoundException($"overlayImage is missing in profile.Path: {profile.Path}");
			}
		    
			using ( var sourceImageStream = _iStorage.ReadStream(itemFilePath))
			using ( var sourceImage = await Image.LoadAsync(sourceImageStream) )
			using ( var overlayImageStream = _hostFileSystem.ReadStream(profile.Path))
			using ( var overlayImage = await Image.LoadAsync(overlayImageStream) )
			using ( var outputStream  = new MemoryStream() )
			{
				await ResizeOverlayImageShared(sourceImage, overlayImage, outputStream, profile,
					outputFullFilePath);
			}
		}

		private async Task ResizeOverlayImageShared(Image sourceImage, Image overlayImage,
			Stream outputStream, AppSettingsPublishProfiles profile, string outputSubPath)
		{
			sourceImage.Mutate(x => x.AutoOrient());

			sourceImage.Mutate(x => x
				.Resize(profile.SourceMaxWidth, 0)
			);

			overlayImage.Mutate(x => x
				.Resize(profile.OverlayMaxWidth, 0)
			);

			int xPoint = sourceImage.Width - overlayImage.Width;
			int yPoint = sourceImage.Height - overlayImage.Height;
			
			sourceImage.Mutate(x => x.DrawImage(overlayImage, 
				new Point(xPoint, yPoint), 1F));

			await sourceImage.SaveAsJpegAsync(outputStream);
			outputStream.Seek(0, SeekOrigin.Begin);

			await _hostFileSystem.WriteStreamAsync(outputStream, outputSubPath);
		}
	}
}
