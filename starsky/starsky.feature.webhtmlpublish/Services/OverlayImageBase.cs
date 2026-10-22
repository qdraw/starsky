using System;
using System.IO;
using System.Threading.Tasks;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Helpers.Slug;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.webhtmlpublish.Services;

/// <summary>
///     Storage, path and validation logic that is shared between the image providers,
///     the providers only implement the pixel work
/// </summary>
public abstract class OverlayImageBase : IOverlayImage
{
	private readonly AppSettings _appSettings;
	private readonly IStorage _iStorage;
	private readonly IStorage _thumbnailStorage;
	protected readonly IStorage HostFileSystem;

	protected OverlayImageBase(ISelectorStorage selectorStorage, AppSettings appSettings)
	{
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		HostFileSystem = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
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
		ThumbnailSize size, string outputFullFilePath, AppSettingsPublishProfiles profile)
	{
		if ( string.IsNullOrWhiteSpace(itemFileHash) )
		{
			throw new ArgumentNullException(nameof(itemFileHash));
		}

		var fileHashWithExtension = ThumbnailNameHelper.Combine(itemFileHash,
			size, _appSettings.ThumbnailImageFormat);

		if ( !_thumbnailStorage.ExistFile(fileHashWithExtension) )
		{
			throw new FileNotFoundException("fileHash " + fileHashWithExtension);
		}

		if ( HostFileSystem.ExistFile(outputFullFilePath) )
		{
			return Task.FromResult(false);
		}

		if ( !HostFileSystem.ExistFile(profile.Path) )
		{
			throw new FileNotFoundException(
				$"overlayImage is missing in profile.Path: {profile.Path}");
		}

		return ResizeOverlayImageThumbnailsInternal(fileHashWithExtension,
			outputFullFilePath, profile);
	}

	/// <summary>
	///     Read from _iStorage to HostFileSystem
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

		if ( HostFileSystem.ExistFile(outputFullFilePath) )
		{
			return Task.FromResult(false);
		}

		if ( !HostFileSystem.ExistFile(profile.Path) )
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
	private async Task<bool> ResizeOverlayImageThumbnailsInternal(
		string fileHashWithExtension,
		string outputFullFilePath, AppSettingsPublishProfiles profile)
	{
		using ( var sourceImageStream = _thumbnailStorage.ReadStream(fileHashWithExtension) )
		using ( var overlayImageStream =
		       HostFileSystem.ReadStream(profile.Path) ) // for example a logo
		{
			return await ResizeOverlayImageShared(sourceImageStream, overlayImageStream,
				profile, outputFullFilePath);
		}
	}

	/// <summary>
	///     [Internal] Without checks if input is valid - Read from _iStorage to HostFileSystem
	/// </summary>
	private async Task<bool> ResizeOverlayImageLargeInternal(string itemFilePath,
		string outputFullFilePath, AppSettingsPublishProfiles profile)
	{
		using ( var sourceImageStream = _iStorage.ReadStream(itemFilePath) )
		using ( var overlayImageStream = HostFileSystem.ReadStream(profile.Path) )
		{
			return await ResizeOverlayImageShared(sourceImageStream, overlayImageStream,
				profile, outputFullFilePath);
		}
	}

	/// <summary>
	///     Orient, resize to profile.SourceMaxWidth, draw the resized overlay (profile.OverlayMaxWidth)
	///     at the bottom right, save as Jpeg to the host file system
	/// </summary>
	/// <param name="sourceImageStream">image to publish</param>
	/// <param name="overlayImageStream">logo</param>
	/// <param name="profile">image profile that contains sizes</param>
	/// <param name="outputSubPath">location on the host file system</param>
	protected abstract Task<bool> ResizeOverlayImageShared(Stream sourceImageStream,
		Stream overlayImageStream, AppSettingsPublishProfiles profile, string outputSubPath);
}
