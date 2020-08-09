using starsky.foundation.platform.Models;

namespace starsky.feature.webhtmlpublish.Interfaces
{
	public interface IOverlayImage
	{
		string FilePathOverlayImage(string sourceFilePath, AppSettingsPublishProfiles profile);
		string FilePathOverlayImage(string outputParentFullFilePathFolder,
			string sourceFilePath, AppSettingsPublishProfiles profile);

		void ResizeOverlayImageThumbnails(string itemFileHash, string outputFullFilePath, AppSettingsPublishProfiles profile);
		void ResizeOverlayImageLarge(string itemFilePath, string outputFullFilePath, AppSettingsPublishProfiles profile);
	}
}
