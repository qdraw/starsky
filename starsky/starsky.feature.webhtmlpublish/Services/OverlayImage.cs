using System.Threading.Tasks;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.injection;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;

namespace starsky.feature.webhtmlpublish.Services;

/// <summary>
///     Entry point for the logo merge, the image library is chosen by IOverlayImageFactory
/// </summary>
[Service(typeof(IOverlayImage), InjectionLifetime = InjectionLifetime.Scoped)]
public class OverlayImage(IOverlayImageFactory factory) : IOverlayImage
{
	private readonly IOverlayImage _overlayImage = factory.Create();

	public string FilePathOverlayImage(string sourceFilePath, AppSettingsPublishProfiles profile)
	{
		return _overlayImage.FilePathOverlayImage(sourceFilePath, profile);
	}

	public string FilePathOverlayImage(string outputParentFullFilePathFolder,
		string sourceFilePath, AppSettingsPublishProfiles profile)
	{
		return _overlayImage.FilePathOverlayImage(outputParentFullFilePathFolder,
			sourceFilePath, profile);
	}

	public Task<bool> ResizeOverlayImageThumbnails(string itemFileHash, ThumbnailSize size,
		string outputFullFilePath, AppSettingsPublishProfiles profile)
	{
		return _overlayImage.ResizeOverlayImageThumbnails(itemFileHash, size,
			outputFullFilePath, profile);
	}

	public Task<bool> ResizeOverlayImageLarge(string itemFilePath, string outputFullFilePath,
		AppSettingsPublishProfiles profile)
	{
		return _overlayImage.ResizeOverlayImageLarge(itemFilePath, outputFullFilePath, profile);
	}
}
