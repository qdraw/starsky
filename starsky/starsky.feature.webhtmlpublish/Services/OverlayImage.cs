using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Helpers.Slug;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.webhtmlpublish.Services;

[Service(typeof(IOverlayImage), InjectionLifetime = InjectionLifetime.Scoped)]
public class OverlayImage : IOverlayImage
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _hostFileSystem;
	private readonly IStorage _iStorage;
	private readonly IStorage _thumbnailStorage;

	public OverlayImage(ISelectorStorage selectorStorage, AppSettings appSettings)
	{
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		_hostFileSystem = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_appSettings = appSettings;
	}

	public string FilePathOverlayImage(string sourceFilePath,
		AppSettingsPublishProfiles profile)
	{
		var result = profile.Folder + GenerateSlugHelper.GenerateSlug(
			                            Path.GetFileNameWithoutExtension(sourceFilePath), true)
		                            + profile.Append +
		                            profile.GetExtensionWithDot(sourceFilePath);
		return result;
	}

	public string FilePathOverlayImage(string outputParentFullFilePathFolder,
		string sourceFilePath,
		AppSettingsPublishProfiles profile)
	{
		var result = PathHelper.AddBackslash(outputParentFullFilePathFolder) +
		             FilePathOverlayImage(sourceFilePath, profile);
		return result;
	}

	public Task<bool> ResizeOverlayImageThumbnails(string itemFileHash,
		string outputFullFilePath, AppSettingsPublishProfiles profile)
	{
		if ( string.IsNullOrWhiteSpace(itemFileHash) )
		{
			throw new ArgumentNullException(nameof(itemFileHash));
		}

		var fileHashWithExtension = ThumbnailNameHelper.Combine(itemFileHash,
			ThumbnailSize.Large, _appSettings.ThumbnailImageFormat);

		if ( !_thumbnailStorage.ExistFile(fileHashWithExtension) )
		{
			throw new FileNotFoundException("fileHash " + fileHashWithExtension);
		}

		if ( _hostFileSystem.ExistFile(outputFullFilePath) )
		{
			return Task.FromResult(false);
		}

		if ( !_hostFileSystem.ExistFile(profile.Path) )
		{
			throw new FileNotFoundException(
				$"overlayImage is missing in profile.Path: {profile.Path}");
		}

		return ResizeOverlayImageThumbnailsInternal(fileHashWithExtension,
			outputFullFilePath, profile);
	}

	/// <summary>
	///     Read from _iStorage to _hostFileSystem
	/// </summary>
	/// <param name="itemFilePath">input Image</param>
	/// <param name="outputFullFilePath">location where to store</param>
	/// <param name="profile">image profile that contains sizes</param>
	/// <exception cref="FileNotFoundException">source image not found</exception>
	public Task<bool> ResizeOverlayImageLarge(string itemFilePath,
		string outputFullFilePath, AppSettingsPublishProfiles profile)
	{
		if ( string.IsNullOrWhiteSpace(itemFilePath) )
		{
			throw new
				ArgumentNullException(nameof(itemFilePath));
		}

		if ( !_iStorage.ExistFile(itemFilePath) )
		{
			throw new FileNotFoundException("subPath " + itemFilePath);
		}

		if ( _hostFileSystem.ExistFile(outputFullFilePath) )
		{
			return Task.FromResult(false);
		}

		if ( !_hostFileSystem.ExistFile(profile.Path) )
		{
			throw new FileNotFoundException(
				$"overlayImage is missing in profile.Path: {profile.Path}");
		}

		return ResizeOverlayImageLargeInternal(itemFilePath,
			outputFullFilePath,
			profile);
	}


	/// <summary>
	///     [Internal] Without checks if input is valid - Read from thumbnail storage
	/// </summary>
	/// <param name="fileHashWithExtension">input Image</param>
	/// <param name="outputFullFilePath">location where to store</param>
	/// <param name="profile">image profile that contains sizes</param>
	private async Task<bool> ResizeOverlayImageThumbnailsInternal(
		string fileHashWithExtension,
		string outputFullFilePath, AppSettingsPublishProfiles profile)
	{
		using ( var sourceImageStream = _thumbnailStorage.ReadStream(fileHashWithExtension) )
		using ( var sourceImage = await Image.LoadAsync(sourceImageStream) )
		using ( var overlayImageStream =
		       _hostFileSystem.ReadStream(profile.Path) ) // for example a logo
		using ( var overlayImage = await Image.LoadAsync(overlayImageStream) )
		using ( var outputStream = new MemoryStream() )
		{
			return await ResizeOverlayImageShared(sourceImage, overlayImage, outputStream,
				profile,
				outputFullFilePath);
		}
	}

	/// <summary>
	///     [Internal] Without checks if input is valid - Read from _iStorage to _hostFileSystem
	/// </summary>
	/// <param name="itemFilePath">input Image</param>
	/// <param name="outputFullFilePath">location where to store</param>
	/// <param name="profile">image profile that contains sizes</param>
	private async Task<bool> ResizeOverlayImageLargeInternal(string itemFilePath,
		string outputFullFilePath, AppSettingsPublishProfiles profile)
	{
		using ( var sourceImageStream = _iStorage.ReadStream(itemFilePath) )
		using ( var sourceImage = await Image.LoadAsync(sourceImageStream) )
		using ( var overlayImageStream = _hostFileSystem.ReadStream(profile.Path) )
		using ( var overlayImage = await Image.LoadAsync(overlayImageStream) )
		using ( var outputStream = new MemoryStream() )
		{
			return await ResizeOverlayImageShared(sourceImage, overlayImage, outputStream,
				profile,
				outputFullFilePath);
		}
	}

	[SuppressMessage("Performance",
		"CA1859:Use concrete types when possible for improved performance")]
	private async Task<bool> ResizeOverlayImageShared(Image sourceImage, Image overlayImage,
		Stream outputStream, AppSettingsPublishProfiles profile, string outputSubPath)
	{
		sourceImage.Mutate(x => x.AutoOrient());

		sourceImage.Mutate(x => x
			.Resize(profile.SourceMaxWidth, 0, KnownResamplers.Lanczos3)
		);

		overlayImage.Mutate(x => x
			.Resize(profile.OverlayMaxWidth, 0, KnownResamplers.Lanczos3)
		);

		var xPoint = sourceImage.Width - overlayImage.Width;
		var yPoint = sourceImage.Height - overlayImage.Height;

		sourceImage.Mutate(x => x.DrawImage(overlayImage,
			new Point(xPoint, yPoint), 1F));

		await sourceImage.SaveAsJpegAsync(outputStream);
		outputStream.Seek(0, SeekOrigin.Begin);

		await _hostFileSystem.WriteStreamAsync(outputStream, outputSubPath);
		return true;
	}
}
